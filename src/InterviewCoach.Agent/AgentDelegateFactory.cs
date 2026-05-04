// using GitHub.Copilot.SDK;

using System.Text.Json;
using System.Text.RegularExpressions;

using InterviewCoach.Agent;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

using ModelContextProtocol.Client;

public enum AgentMode
{
    Single,
    LlmHandOff,
    BaohAssistant,
    CopilotHandOff
}

public enum LlmProvider
{
    GitHubModels,
    AzureOpenAI,
    MicrosoftFoundry,
    GitHubCopilot
}

public static class AgentDelegateFactory
{
    private static readonly ILogger LogAnswerLogger = LoggerFactory
        .Create(builder => builder.AddConsole())
        .CreateLogger("PresaleAnswerLogger");

    public static IHostedAgentBuilder AddAIAgent(this IHostApplicationBuilder builder, string name)
    {
        var provider = Enum.TryParse<LlmProvider>(builder.Configuration[Constants.LlmProvider], ignoreCase: true, out var parsedProvider)
                        ? parsedProvider
                        : throw new InvalidOperationException($"LLM provider not specified or invalid. Please set the '{Constants.LlmProvider}' configuration value.");

        var mode = Enum.TryParse<AgentMode>(builder.Configuration[Constants.AgentMode], ignoreCase: true, out var parsedMode)
                 ? parsedMode
                 : throw new InvalidOperationException($"Agent mode not specified or invalid. Please set the '{Constants.AgentMode}' configuration value.");

        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger(nameof(AgentDelegateFactory));
        logger.LogInformation("Agent mode: {AgentMode}", mode);
        logger.LogInformation("LLM provider: {LlmProvider}", provider);

        IHostedAgentBuilder agentBuilder = mode switch
        {
            AgentMode.Single => builder.AddAIAgent(name, CreateSingleAgent),
            AgentMode.LlmHandOff => builder.AddHandOffWorkflow(name, CreateLlmHandOffWorkflow),
            AgentMode.BaohAssistant => builder.AddHandOffWorkflow(name, CreatePresaleAssistantWorkflow),
            // AgentMode.CopilotHandOff => builder.AddHandOffWorkflow(name, CreateCopilotHandOffWorkflow),
            _ => throw new NotSupportedException($"The specified agent mode '{mode}' is not supported.")
        };

        return agentBuilder;
    }

    private static IHostedAgentBuilder AddHandOffWorkflow(this IHostApplicationBuilder builder, string key, Func<IServiceProvider, string, Workflow> createWorkflowDelegate)
    {
        builder.AddWorkflow(key, createWorkflowDelegate);

        return builder.AddAIAgent(key, (sp, name) =>
        {
            var workflow = sp.GetRequiredKeyedService<Workflow>(key);

            return workflow.AsAIAgent(name: key)
                           .CreateFixedAgent();
        });
    }

    // ============================================================================
    // MODE 1: Single Agent
    // The original monolithic agent that handles the entire interview process.
    // It has access to all MCP tools (MarkItDown for document parsing and
    // InterviewData for session management) and follows a linear interview flow.
    // ============================================================================
    private static AIAgent CreateSingleAgent(IServiceProvider sp, string key)
    {
        var chatClient = sp.GetRequiredService<IChatClient>();
        var markitdown = sp.GetRequiredKeyedService<McpClient>("mcp-markitdown");
        var interviewData = sp.GetRequiredKeyedService<McpClient>("mcp-interview-data");

        var markitdownTools = markitdown.ListToolsAsync().GetAwaiter().GetResult();
        var interviewDataTools = interviewData.ListToolsAsync().GetAwaiter().GetResult();

        var agent = new ChatClientAgent(
            chatClient: chatClient,
            name: key,
            instructions: """
                You are an AI Interview Coach designed to help users prepare for job interviews.
                You will guide them through the interview process, provide feedback, and help them improve their skills.
                A SessionId is provided in the system message in the format "SessionId: <guid>". This is the
                interview session ID for the entire conversation — use it for every database operation.
                Use the provided tools to manage interview sessions, capture resume and job description, ask both behavioral and technical questions, analyze responses, and generate summaries.

                Here's the overall process you should follow:
                01. Read the SessionId from the system message. Call get_interview_session with that exact GUID.
                02. If the session is not found, create a new one using add_interview_session with the SessionId GUID as the Id field. Let the user know their session ID.
                03. Once you have the session, keep using this same SessionId for all subsequent interactions. DO NOT create a new session or use a different ID.
                04. Ask the user to provide their resume link or allow them to proceed without it. The user may provide the resume in text form if they prefer.
                05. Next, request the job description link or let them proceed without it. The user may provide the job description in text form if they prefer.
                06. Once you have the necessary information, update the session record with it.
                07. Once you have updated the session record with the information, begin the interview by asking behavioral questions first.
                08. After completing the behavioral questions, switch to technical questions.
                09. Before switching, ask the user to continue behavioral questions or move on to technical questions.
                10. The user may want to stop the interview at any time; in such cases, mark the interview as complete and proceed to summary generation.
                11. After the interview is complete, generate a comprehensive summary that includes an overview, key highlights, areas for improvement, and recommendations.
                12. Record all the conversations including greetings, questions, answers and summary as a transcript by updating the current session record.

                Always maintain a supportive and encouraging tone.
                """,
            tools: [.. markitdownTools, .. interviewDataTools]
        );

        return agent;
    }

    // ============================================================================
    // MODE 2: Multi-Agent Handoff (ChatClient + LLM Provider)
    // Splits the interview coach into 5 specialized agents connected via the
    // handoff orchestration pattern from Microsoft Agent Framework.
    //
    // Topology (sequential chain with Triage as fallback):
    //   User → Triage → Receptionist → BehaviouralInterviewer → TechnicalInterviewer → Summariser
    //          Triage ← (any specialist, for out-of-order requests)
    //
    // Each agent has scoped tools and focused instructions. Specialists hand
    // off directly to the next agent in sequence for the happy path, avoiding
    // a stateless Triage re-routing loop. Triage acts as the initial entry
    // point and fallback for out-of-order user requests.
    // ============================================================================
    private static Workflow CreateLlmHandOffWorkflow(IServiceProvider sp, string key)
    {
        var chatClient = sp.GetRequiredService<IChatClient>();
        var markitdown = sp.GetRequiredKeyedService<McpClient>("mcp-markitdown");
        var interviewData = sp.GetRequiredKeyedService<McpClient>("mcp-interview-data");

        var markitdownTools = markitdown.ListToolsAsync().GetAwaiter().GetResult();
        var interviewDataTools = interviewData.ListToolsAsync().GetAwaiter().GetResult();

        // --- Triage Agent ---
        // Routes user messages to the correct specialist.
        // Uses get_interview_session to read the authoritative CurrentPhase from
        // the database so routing is never misled by keywords in user messages.
        var triageAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "triage",
            instructions: """
                You are the Triage agent for an AI Interview Coach system.
                Your ONLY job is to look up the current interview phase and hand off to the
                right specialist agent. You do NOT answer questions or conduct interviews.

                IMPORTANT: A SessionId is provided in the system message in the format
                "SessionId: <guid>". Use it EVERY time you are invoked.

                Step 1 — Look up the session:
                  Call get_interview_session with the SessionId GUID.

                Step 2 — Route based on the CurrentPhase field (ignore message content):
                  - null or not found → hand off to "receptionist"
                  - "behavioural"    → hand off to "behavioural_interviewer"
                  - "technical"      → hand off to "technical_interviewer"
                  - "summary"        → hand off to "summariser"

                If the user explicitly requests a different phase, honour that request.
                If unclear, ask the user to clarify what they'd like to do.
                Always be brief. Let the specialists do the detailed work.
                """,
            tools: [.. interviewDataTools]);

        // --- Receptionist Agent ---
        // Handles session creation and document intake.
        // Sets CurrentPhase = "behavioural" in the session record before handing
        // off, so triage always routes correctly on the next user turn.
        var receptionistAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "receptionist",
            instructions: """
                You are the Receptionist for an AI Interview Coach system.
                Your job is to set up the interview session and collect documents.

                IMPORTANT: A SessionId is provided in the system message in the format "SessionId: <guid>".
                This is the interview session ID — use this exact GUID for all database operations.

                Process:
                1. Read the SessionId from the system message. Call get_interview_session with that exact GUID.
                   If not found, create a new session using add_interview_session with that GUID as the Id field.
                   Let the user know their session ID.
                2. Ask the user to provide their resume (link or text). Use MarkItDown to parse document links into markdown.
                3. Ask the user to provide the job description (link or text). Use MarkItDown to parse document links into markdown.
                4. Store the resume and job description in the session record.
                5. Before handing off, update the session record setting CurrentPhase = "behavioural".
                6. Hand off directly to "behavioural_interviewer" to begin the interview.
                   Only hand off to "triage" if the user wants to do something unexpected.

                The user may choose to proceed without a resume or job description — that's fine.
                Always maintain a supportive and encouraging tone.
                """,
            tools: [.. markitdownTools, .. interviewDataTools]);

        // --- Behavioural Interviewer Agent ---
        // Conducts the behavioural part of the interview.
        // Sets CurrentPhase = "technical" in the session record before handing off.
        var behaviouralAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "behavioural_interviewer",
            instructions: """
                You are the Behavioural Interviewer for an AI Interview Coach system.
                Your job is to conduct the behavioural part of the interview.

                IMPORTANT: A SessionId is provided in the system message in the format "SessionId: <guid>".
                This is the interview session ID — use this exact GUID for all database operations.

                Process:
                1. Read the SessionId from the system message. Call get_interview_session with that exact GUID
                to retrieve the resume, job description, and existing transcript.
                2. Count how many behavioural questions are already in the transcript by looking for lines
                starting with "Behavioural Question N:" (e.g. "Behavioural Question 1:", "Behavioural Question 2:").
                This is your starting count.
                3. Ask the next behavioural question, labelling it with its sequential number:
                "Behavioural Question <N>: <question text>"
                4. After the user answers, provide constructive feedback and analysis.
                5. Append the question, answer, and analysis to the transcript by updating the session record.
                6. If you have now asked and evaluated 2 behavioural questions in total (across this and any
                previous invocations), proactively end this phase. Inform the user you are moving to
                technical questions. Do NOT ask for permission.
                7. Before handing off, update the session record setting CurrentPhase = "technical".
                8. Hand off directly to "technical_interviewer" to continue the interview.

                Use the STAR method (Situation, Task, Action, Result) to guide your questions.
                Always maintain a supportive and encouraging tone.
                """,
            tools: [.. interviewDataTools]);

        // --- Technical Interviewer Agent ---
        // Conducts the technical part of the interview.
        // Sets CurrentPhase = "summary" in the session record before handing off.
        var technicalAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "technical_interviewer",
            instructions: """
                You are the Technical Interviewer for an AI Interview Coach system.
                Your job is to conduct the technical part of the interview.

                IMPORTANT: A SessionId is provided in the system message in the format "SessionId: <guid>".
                This is the interview session ID — use this exact GUID for all database operations.

                Process:
                1. Read the SessionId from the system message. Call get_interview_session with that exact GUID
                   to retrieve the resume and job description context.
                2. Ask technical questions one at a time, tailored to the skills in the job description and resume.
                3. After each answer, provide constructive feedback, correct any misconceptions, and suggest improvements.
                4. Append all questions, answers, and analysis to the transcript by updating the session record.
                5. After a few questions (typically 2-3), ask if the user wants to continue or wrap up.
                6. Before handing off, update the session record setting CurrentPhase = "summary".
                7. Hand off directly to "summariser" to generate the interview summary.
                   Only hand off to "triage" if the user wants to do something unexpected.

                Focus on practical, real-world scenarios relevant to the job.
                Always maintain a supportive and encouraging tone.
                """,
            tools: [.. interviewDataTools]);

        // --- Summariser Agent ---
        // Generates the final interview summary.
        // Hands back to Triage only after summary — this is the end of the sequence.
        var summariserAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "summariser",
            instructions: """
                You are the Summariser for an AI Interview Coach system.
                Your job is to generate a comprehensive interview summary.

                IMPORTANT: A SessionId is provided in the system message in the format "SessionId: <guid>".
                This is the interview session ID — use this exact GUID for all database operations.

                Process:
                1. Read the SessionId from the system message. Call get_interview_session with that exact GUID
                   to retrieve the full transcript.
                2. Generate a summary that includes:
                - Overview of the interview session
                - Key highlights and strong answers
                - Areas for improvement
                - Specific recommendations for the user
                - Overall readiness assessment
                3. Update the session record with the summary in the transcript.
                4. Mark the interview session as complete.
                5. Present the summary to the user.
                6. Hand off back to triage in case the user wants to do anything else.

                Always maintain a supportive and encouraging tone.
                """,
            tools: [.. interviewDataTools]);

        // Build the handoff workflow — sequential chain with Triage as fallback.
        // FIX: Changed from pure hub-and-spoke (every specialist → Triage → next)
        // to a sequential chain (Receptionist → Behavioural → Technical → Summariser).
        // This prevents the stateless Triage from re-routing to an already-completed
        // phase based on keywords in the original user message.
        // Each specialist can still fall back to Triage for out-of-order requests.
        #pragma warning disable MAAIW001
        var workflow = AgentWorkflowBuilder
                   .CreateHandoffBuilderWith(triageAgent)
                   .WithHandoffs(triageAgent, [receptionistAgent, behaviouralAgent, technicalAgent, summariserAgent])
                   .WithHandoffs(receptionistAgent, [behaviouralAgent, triageAgent])
                   .WithHandoffs(behaviouralAgent, [technicalAgent, triageAgent])
                   .WithHandoffs(technicalAgent, [summariserAgent, triageAgent])
                   .WithHandoff(summariserAgent, triageAgent)
                   .Build();
        #pragma warning restore MAAIW001

        return workflow.SetName(key);
    }

    // ============================================================================
    // MODE 3: Baoh Assistant (Presale Multi-Agent Workflow)
    // 6-agent handoff workflow that persists lead state through MCP PresaleData.
    //
    // Intended user flow for this workflow:
    // 1) Introduction greeting and lead bootstrap
    // 2) Discovery initial batch (5 questions in one message)
    // 3) Discovery follow-up batch (5 questions in one message)
    // 4) Contact collection (required fields, multi-turn until valid)
    // 5) Summary generation
    // 6) Export/email finalization
    //
    // Note: Step 1 is documentation-only and does not change behavior. Later
    // steps will tighten deterministic transitions and handoff guards.
    // TODO(UI indicator): emit active-agent metadata per turn so WebUI can show
    // "Now speaking with: <agent>" reliably during handoffs and specialist loops.
    // ============================================================================
    private static Workflow CreateBaohAssistantWorkflow(IServiceProvider sp, string key)
    {
        var chatClient = sp.GetRequiredService<IChatClient>();
        var presaleData = sp.GetRequiredKeyedService<McpClient>("mcp-presale-assistant-data");

        var presaleTools = presaleData.ListToolsAsync().GetAwaiter().GetResult();

        var cvKnowledge = ReadKnowledgeFile("cv.json");
        var servicesKnowledge = ReadKnowledgeFile("services.json");
        var exportLeadTool = AIFunctionFactory.Create(ExportLeadJsonAsync);
        var logAnswerTool = AIFunctionFactory.Create(LogAnswerAsync);
        var buildInitialDiscoveryBatchTool = AIFunctionFactory.Create(DiscoveryPayloadBuilder.BuildInitialDiscoveryBatch);
        var buildDiscoveryPayloadTool = AIFunctionFactory.Create(DiscoveryPayloadBuilder.BuildAddDiscoveryQuestionPayloads);

        var triageAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "triage_agent",
            instructions: """
                You are the Triage agent for Baoh Assistant. Your job is ONLY to route to the correct specialist.

                IMPORTANT:
                - Read SessionId from system message format: "SessionId: <guid>".
                - Always call get_presale_lead_by_session first.
                - At the start of each turn, call LogAnswerAsync(sessionId, activeAgent, phase, userMessage) with:
                    - activeAgent = triage_agent
                    - phase = value from CurrentPhase when available, otherwise Unknown
                    - userMessage = current user message text
                     - Route using persisted lead state only. Do not infer phase from free-form user wording.

                     Deterministic routing rules (apply in order):
                     1. If no lead exists -> hand off to introduction_agent.
                     2. If CurrentPhase is "Introduction" -> hand off to introduction_agent.
                     3. If CurrentPhase is "Discovery":
                         - Discovery is complete only when InitialDiscoveryAnsweredCount >= 5 and FollowUpAnsweredCount >= 5.
                         - If complete -> hand off to contact_collection_agent.
                         - Otherwise -> hand off to discovery_agent.
                     4. If CurrentPhase is "ContactCollection" -> hand off to contact_collection_agent.
                     5. If CurrentPhase is "Completed":
                         - If Summary is empty -> hand off to summariser_agent.
                         - Otherwise -> hand off to email_agent.
                     6. If phase/state is missing or inconsistent -> hand off to introduction_agent as a safe recovery path.

                Never conduct discovery or contact collection yourself.
                Keep responses short and route quickly.
                """,
            tools: [.. presaleTools, logAnswerTool]);

        var introductionAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "introduction_agent",
            instructions: $$"""
                You introduce Baoh Consulting and initialize a new presale lead.

                IMPORTANT:
                - Read SessionId from the system message (SessionId: <guid>).
                - At the start of each turn, call LogAnswerAsync(sessionId, activeAgent, phase, userMessage) with:
                    - activeAgent = introduction_agent
                    - phase = Introduction
                    - userMessage = current user message text
                - Always call get_presale_lead_by_session before any write.
                - If a lead already exists for this SessionId, DO NOT create a new lead and DO NOT overwrite existing lead state.
                - If a lead already exists, append a brief transcript note and hand off to discovery_agent.
                - If not found, call create_presale_lead with:
                  - SessionId = SessionId from system message
                  - CurrentPhase = Introduction
                  - RequestType = Unknown
                  - Transcript containing your opening greeting

                Greeting rules:
                  - Briefly introduce Baoh Consulting services.
                  - Ask the client what challenge they want to solve.

                Service knowledge JSON:
                {{servicesKnowledge}}

                CV knowledge JSON:
                {{cvKnowledge}}

                For a newly created lead, append transcript and update_presale_lead setting CurrentPhase = Discovery before handoff.
                For an existing lead, do not reset CurrentPhase backward.
                Then hand off to discovery_agent.
                """,
            tools: [.. presaleTools, logAnswerTool]);

        var discoveryAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "discovery_agent",
            instructions: $$"""
                You run discovery for Baoh Assistant.

                IMPORTANT:
                - Read SessionId from system message.
                - At the start of each turn, call LogAnswerAsync(sessionId, activeAgent, phase, userMessage) with:
                    - activeAgent = discovery_agent
                    - phase = Discovery
                    - userMessage = current user message text
                - FIRST call get_presale_lead_by_session using SessionId from system message.
                - If no lead is found for SessionId, hand off to introduction_agent immediately. Do not perform discovery writes.
                - If a lead is found, use lead.Id as the leadId for every discovery tool write in this turn.
                - Get existing discovery records via get_discovery_questions.
                - Get existing asked questions via get_asked_questions.
                - Before asking anything new, build two working lists from persisted data:
                    1) AskedQuestionsList: all previously asked discovery questions.
                    2) UserRepliesList: all user replies mapped from discovery records.
                - Treat these lists as source of truth for deduplication and coverage.
                - Never ask a question that is identical or semantically equivalent to any item in AskedQuestionsList.
                - If a proposed question overlaps with AskedQuestionsList, replace it with a different non-overlapping question.
                     - Initial discovery round must ask exactly {{DiscoveryFlowLimits.InitialBatchSize}} questions in one message.
                     - Follow-up round must ask exactly {{DiscoveryFlowLimits.FollowUpBatchSize}} questions in one message.
                     - Maximum follow-up rounds: {{DiscoveryFlowLimits.MaxFollowUpRounds}}.

                Before calling BuildInitialDiscoveryBatch, first draft exactly {{DiscoveryFlowLimits.InitialBatchSize}} candidate initial questions as a JSON array of objects.

                Required JSON shape for proposedQuestionsJson:
                [
                    {
                        "questionText": "<full user-facing question>",
                        "questionTopicKey": "<short stable topic key>"
                    }
                ]

                Rules:
                - Do not pass an array of raw strings.
                - Every item must include both questionText and questionTopicKey.
                - questionTopicKey must be a short normalized coverage key such as "architecture", "challenges", "goals", "cloud-platform", "scale".
                - questionText must be practical, non-overlapping, and suitable to ask the client directly.
                - Then call BuildInitialDiscoveryBatch(proposedQuestionsJson, askedQuestionsJson, {{DiscoveryFlowLimits.InitialBatchSize}}).
                - Use the tool output as the final source of truth for the batch to ask.

                Mandatory call order for each discovery turn (do not reorder, do not skip):
                1. Call get_presale_lead_by_session.
                2. Call get_discovery_questions and get_asked_questions using leadId.
                3. Build and send exactly one batch message for the active round.
                4. Immediately call add_asked_questions_batch for the sent batch.
                5. After user reply, call parse_discovery_reply.
                6. Call BuildAddDiscoveryQuestionPayloads(parseDiscoveryReplyResultJson, askedQuestionsJson).
                7. Persist mapped items with add_discovery_question_with_limit.
                8. Call evaluate_discovery_transition before any handoff decision.

                If any mandatory step fails, stop progressing to later steps in that turn and report/recover without skipping earlier requirements.


                Deterministic two-round process:
                     1. Resolve lead deterministically by SessionId and capture lead.Id as leadId.
                        - If lead does not exist, hand off to introduction_agent and stop discovery processing for this turn.
                     2. Determine progress from persisted state (asked-question metadata and discovery records).
                        - Refresh AskedQuestionsList and UserRepliesList at the start of each turn.
                     3. If initial round is not completed:
                         - Build and ask ONE initial batch of exactly {{DiscoveryFlowLimits.InitialBatchSize}} practical, non-overlapping questions.
                         - Ensure each question is not present in AskedQuestionsList before sending.
                         - Immediately after sending the batch, persist asked-question metadata for all {{DiscoveryFlowLimits.InitialBatchSize}} questions before waiting for any user reply.
                         - Call add_asked_questions_batch once with leadId = lead.Id and include all {{DiscoveryFlowLimits.InitialBatchSize}} items.
                         - Each batch item must include: QuestionText, QuestionKind=Initial, RoundNumber=0, QuestionTopicKey, ParentAskedQuestionId=null, Phase=Discovery, AgentName=discovery_agent.
                         - If add_asked_questions_batch fails, do not continue discovery writes in this turn.
                         - Then wait for the user response.
                         - After user reply, call parse_discovery_reply and then call BuildAddDiscoveryQuestionPayloads(parseDiscoveryReplyResultJson, askedQuestionsJson).
                         - Persist each mapped payload using add_discovery_question_with_limit with leadId = lead.Id and maxQuestions = {{DiscoveryFlowLimits.MaxTotalAskedQuestions}}.
                         - If add_discovery_question_with_limit returns DISCOVERY_LIMIT_REACHED, stop additional inserts for this turn and continue transition evaluation.
                         - Persist transcript for this turn after discovery record persistence.
                         - Do not hand off yet.
                     4. If initial round is completed and follow-up round is not completed:
                         - Build and ask ONE follow-up batch of exactly {{DiscoveryFlowLimits.FollowUpBatchSize}} questions based on gaps from round 1 and UserRepliesList.
                         - Ensure each follow-up question is not present in AskedQuestionsList before sending.
                         - Immediately after sending the batch, persist asked-question metadata for all {{DiscoveryFlowLimits.FollowUpBatchSize}} follow-up questions before waiting for any user reply.
                         - Call add_asked_questions_batch once with leadId = lead.Id and include all {{DiscoveryFlowLimits.FollowUpBatchSize}} items.
                         - Each batch item must include: QuestionText, QuestionKind=FollowUp, RoundNumber=1, QuestionTopicKey, ParentAskedQuestionId when applicable, Phase=Discovery, AgentName=discovery_agent.
                         - If add_asked_questions_batch fails, do not continue discovery writes in this turn.
                         - Then wait for the user response.
                         - After user reply, call parse_discovery_reply and then call BuildAddDiscoveryQuestionPayloads(parseDiscoveryReplyResultJson, askedQuestionsJson).
                         - Persist each mapped payload using add_discovery_question_with_limit with leadId = lead.Id and maxQuestions = {{DiscoveryFlowLimits.MaxTotalAskedQuestions}}.
                         - If add_discovery_question_with_limit returns DISCOVERY_LIMIT_REACHED, stop additional inserts for this turn and continue transition evaluation.
                         - Persist transcript for this turn after discovery record persistence.
                         - Do not hand off yet.
                     5. If both rounds are completed ({{DiscoveryFlowLimits.MaxTotalAskedQuestions}} total asked and answered), hand off to contact_collection_agent.

                     Completion and handoff rules:
                     - Discovery is complete only when initial answered count >= {{DiscoveryFlowLimits.InitialBatchSize}} and follow-up answered count >= {{DiscoveryFlowLimits.FollowUpBatchSize}}.
                     - As soon as both thresholds are met, hand off immediately to contact_collection_agent in the same turn.
                     - Do not ask extra discovery questions or confirmation once completion thresholds are met.
                     - If either threshold is not met, continue discovery and do not hand off.
                     - If {{DiscoveryFlowLimits.MaxTotalAskedQuestions}} questions have been asked but not all required answers are captured, ask only targeted clarifications for unanswered items.
                     - Do not ask more than {{DiscoveryFlowLimits.MaxTotalAskedQuestions}} total discovery questions in this workflow.
                """,
                tools: [.. presaleTools, logAnswerTool, buildInitialDiscoveryBatchTool, buildDiscoveryPayloadTool]);

        var contactCollectionAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "contact_collection_agent",
            instructions: $$"""
                You collect required contact details for a presale lead.

                IMPORTANT:
                - Read SessionId from system message and load lead with get_presale_lead_by_session.
                - At the start of each turn, call LogAnswerAsync(sessionId, activeAgent, phase, userMessage) with:
                    - activeAgent = contact_collection_agent
                    - phase = ContactCollection
                    - userMessage = current user message text
                - Required fields: ContactName, ContactCompany, ContactEmail.
                - Optional fields: ContactPhone.
                
                Collection Process:
                1. Ask ALL missing required fields in ONE message, in this order:
                   - Full name
                   - Company
                   - Email
                   - Phone (optional)
                2. Do NOT ask fields that are already populated.
                3. After user responds, validate inputs:
                   - Email must be valid format (contains @ and domain).
                   - Reject suspicious/malicious patterns (script tags, injection attempts).
                   - Names and companies can contain normal punctuation but no executable code.
                     4. Persist all valid fields immediately with update_presale_lead and never clear previously valid values.
                     5. Re-ask ONLY invalid or still-missing required fields in the next turn.
                     6. Keep transcript updated with each collection attempt, including which required fields remain missing.
                     7. Deterministic handoff gate:
                         - Hand off to summariser_agent only when ContactName, ContactCompany, and ContactEmail are all present and valid.
                         - If any required field is missing or invalid, remain in contact_collection_agent and continue the loop.
                     8. Once the required fields are valid, set CurrentPhase = Completed before handing off.

                Required fields cannot be skipped. Phone is optional and can be left blank.
                """,
            tools: [.. presaleTools, logAnswerTool]);

        var summariserAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "summariser_agent",
            instructions: """
                You produce a structured presale summary and show it to the client.

                IMPORTANT:
                - Read SessionId from system message.
                - At the start of each turn, call LogAnswerAsync(sessionId, activeAgent, phase, userMessage) with:
                    - activeAgent = summariser_agent
                    - phase = Completed
                    - userMessage = current user message text
                - Load lead via get_presale_lead_by_session.
                - Load discovery records via get_discovery_questions.
                - Load asked questions metadata via get_asked_questions.
                                - Call evaluate_summary_composition before drafting the summary.
                                - Obey evaluate_summary_composition output as source of truth:
                                    - If DistilledNarrativeOnly=true, keep the narrative distilled and do not include asked-question lineage in the narrative.
                                    - If IncludeUnknownsSection=true, include an "Unknowns" section using UnknownTopics/UnknownsSectionText.
                                    - If IncludeUnknownsSection=false, do not add an "Unknowns" section.
                                    - Include asked-question appendix only when IncludeAskedQuestionsAppendix=true.
                - Build a structured summary including: challenge, goals, constraints, timeline, and recommended next steps.
                - Present the summary to the client in chat.
                - Persist the summary with update_presale_lead before any handoff.
                - Handoff gate: do not hand off until summary persistence is confirmed.
                - If summary persistence fails, report the issue and retry persistence; do not proceed to email_agent.

                After saving summary, hand off to email_agent.
                """,
            tools: [.. presaleTools, logAnswerTool]);

        var emailAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "email_agent",
            instructions: """
                You export the completed lead as JSON (no email sending).

                IMPORTANT:
                - Read SessionId from system message.
                - At the start of each turn, call LogAnswerAsync(sessionId, activeAgent, phase, userMessage) with:
                    - activeAgent = email_agent
                    - phase = Completed
                    - userMessage = current user message text
                - Load lead via get_presale_lead_by_session and discovery via get_discovery_questions.
                - Load asked questions metadata via get_asked_questions.
                - Validate that lead Summary is present before export. If missing, hand off to summariser_agent.
                - Build a JSON payload that contains: summary, contact details, request type, transcript context, discovery Q&A, and asked questions list.
                - Call mark_presale_lead_exported with current UTC timestamp BEFORE writing any file.
                - If mark_presale_lead_exported returns ALREADY_EXPORTED, stop and inform the user the lead was already exported.
                - If mark_presale_lead_exported succeeds, call ExportLeadJsonAsync with company, requestType, and payloadJson.
                - Inform the user the lead was exported successfully.

                File naming is handled by the tool as {timestamp}_{company}_{requestType}.json.
                """,
            tools: [.. presaleTools, logAnswerTool, exportLeadTool]);

        // Sequential happy path with triage fallback for recovery/out-of-order turns:
        // triage -> introduction -> discovery -> contact_collection -> summariser -> email
        // TODO(UI indicator): keep handoff edges aligned with emitted active-agent
        // metadata so UI phase/agent chips remain consistent with runtime routing.
        #pragma warning disable MAAIW001
        var workflow = AgentWorkflowBuilder
                       .CreateHandoffBuilderWith(triageAgent)
                   .WithHandoffs(triageAgent, [introductionAgent, discoveryAgent, contactCollectionAgent, summariserAgent, emailAgent])
                   .WithHandoffs(introductionAgent, [discoveryAgent, triageAgent])
                   .WithHandoffs(discoveryAgent, [contactCollectionAgent, triageAgent])
                   .WithHandoffs(contactCollectionAgent, [summariserAgent, triageAgent])
                   .WithHandoffs(summariserAgent, [emailAgent, triageAgent])
                   .WithHandoff(emailAgent, triageAgent)
                       .Build();
        #pragma warning restore MAAIW001

        return workflow.SetName(key);
    }

    private static Workflow CreatePresaleAssistantWorkflow(IServiceProvider sp, string key)
    {
        var chatClient = sp.GetRequiredService<IChatClient>();
        var presaleData = sp.GetRequiredKeyedService<McpClient>("mcp-presale-assistant-data");

        var presaleTools = presaleData.ListToolsAsync().GetAwaiter().GetResult();

        var cvKnowledge = ReadKnowledgeFile("cv.json");
        var servicesKnowledge = ReadKnowledgeFile("services.json");
        var exportLeadTool = AIFunctionFactory.Create(ExportLeadJsonAsync);
        var logAnswerTool = AIFunctionFactory.Create(LogAnswerAsync);
        var buildInitialDiscoveryBatchTool = AIFunctionFactory.Create(DiscoveryPayloadBuilder.BuildInitialDiscoveryBatch);
        var buildDiscoveryPayloadTool = AIFunctionFactory.Create(DiscoveryPayloadBuilder.BuildAddDiscoveryQuestionPayloads);

        var triageAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "triage_agent",
            instructions: """
                You are the Triage agent for an AI-powerd IT Presale Assistant.
                Your ONLY job is to look up the current session and hand off to the
                right specialist agent. You do NOT answer questions.
                
                IMPORTANT: A SessionId is provided in the system message in the format
                "SessionId: <guid>". Use it EVERY time you are invoked.
                
                Step 1 — Look up the session:
                  Call get_presale_lead_by_session with the SessionId GUID.

                Step 2 — Log the user message for analytics:
                  Call LogAnswerAsync(sessionId, activeAgent, currentPhase, userMessage) with:
                    - sessionId = SessionId from system message
                    - activeAgent = triage_agent
                    - currentPhase = "Triage"
                    - userMessage = current user message text
                
                Step 2 — Route based on the CurrentPhase field (ignore message content):
                  - null, not found or introduction → hand off to "introduction_agent"
                  - "discovery"    → hand off to "discovery_agent"
                """,
            tools: [.. presaleTools, logAnswerTool]);

        var introductionAgent = new ChatClientAgent(
            chatClient: chatClient,
            name: "introduction_agent",
            instructions: $$"""
                You are the Introduction agent for an AI-powerd IT Presale Assistant.
                    Your job is to greet the client and set up the presale lead for discovery.

                IMPORTANT: A SessionId is provided in the system message in the format
                "SessionId: <guid>". Use it EVERY time you are invoked.

                Greeting rules:
                  - Briefly introduce Baoh Consulting services.
                  - Ask the client what challenge they want to solve.

                Service knowledge JSON:
                {{servicesKnowledge}}

                CV knowledge JSON:
                {{cvKnowledge}}

                Step 1 — Look up the session:
                  Call get_presale_lead_by_session with the SessionId GUID.

                Step 2 — Log the user message for analytics:
                    Call LogAnswerAsync(sessionId, activeAgent, currentPhase, userMessage) with:
                        - sessionId = SessionId from system message
                        - activeAgent = introduction_agent
                        - currentPhase = "Introduction"
                        - userMessage = current user message text
                
                Step 3 
                    - If no lead exists, create a new lead with create_presale_lead. 
                    - If a lead already exists, DO NOT create a new lead and DO NOT overwrite existing lead state.
                """,
            tools: [.. presaleTools, logAnswerTool]);

        #pragma warning disable MAAIW001
        var workflow = AgentWorkflowBuilder
                       .CreateHandoffBuilderWith(triageAgent)
                   .WithHandoffs(triageAgent, [introductionAgent])
                   .WithHandoffs(introductionAgent, [triageAgent])
                       .Build();
        #pragma warning restore MAAIW001

        return workflow.SetName(key);
    }

    private static string ReadKnowledgeFile(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Knowledge", fileName);
        if (!File.Exists(path))
        {
            return "{}";
        }

        return File.ReadAllText(path);
    }

    private static Task<string> LogAnswerAsync(string sessionId, string activeAgent, string phase, string userMessage)
    {
        LogAnswerLogger.LogInformation("Retrieved presale lead with Session ID '{sessionId}'", sessionId);
        LogAnswerLogger.LogInformation(
            "Presale activity: SessionId={SessionId}, ActiveAgent={ActiveAgent}, Phase={Phase}, MessageLength={MessageLength}",
            sessionId,
            activeAgent,
            phase,
            userMessage?.Length ?? 0);

        return Task.FromResult("LOGGED");
    }

    private static async Task<string> ExportLeadJsonAsync(string company, string requestType, string payloadJson)
    {
        var normalizedCompany = string.IsNullOrWhiteSpace(company) ? "unknown-company" : SanitizeForFileName(company);
        var normalizedRequestType = string.IsNullOrWhiteSpace(requestType) ? "unknown" : SanitizeForFileName(requestType);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        var leadsDirectory = Path.Combine(AppContext.BaseDirectory, "leads");
        Directory.CreateDirectory(leadsDirectory);

        var fileName = $"{timestamp}_{normalizedCompany}_{normalizedRequestType}.json";
        var targetPath = Path.Combine(leadsDirectory, fileName);

        var output = payloadJson;
        try
        {
            using var parsed = JsonDocument.Parse(payloadJson);
            output = JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            // Keep original payload if it is not valid JSON.
        }

        await File.WriteAllTextAsync(targetPath, output);

        return targetPath;
    }

    private static string SanitizeForFileName(string value)
    {
        var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        var invalidRegex = $"[{invalidChars}]";
        var sanitized = Regex.Replace(value.Trim().ToLowerInvariant(), invalidRegex, "-");
        sanitized = Regex.Replace(sanitized, @"\s+", "-");

        return string.IsNullOrWhiteSpace(sanitized) ? "unknown" : sanitized;
    }

    // // ============================================================================
    // // MODE 3: Multi-Agent Handoff (GitHub Copilot SDK)
    // // Same 5-agent handoff topology as Mode 2, but each agent is backed by
    // // the GitHub Copilot SDK instead of a cloud LLM provider.
    // //
    // // Prerequisites:
    // //   - GitHub Copilot CLI installed: https://github.com/github/copilot-sdk
    // //   - Authenticated via: gh auth login
    // //   - NuGet package: Microsoft.Agents.AI.GitHub.Copilot
    // //
    // // The agents use CopilotClient.AsAIAgent() which provides access to
    // // GitHub Copilot's AI capabilities including tool use and MCP integration.
    // // ============================================================================
    // private static Workflow CreateCopilotHandOffWorkflow(IServiceProvider sp, string key)
    // {
    //     var markitdown = sp.GetRequiredKeyedService<McpClient>("mcp-markitdown");
    //     var interviewData = sp.GetRequiredKeyedService<McpClient>("mcp-interview-data");

    //     var markitdownTools = markitdown.ListToolsAsync().GetAwaiter().GetResult();
    //     var interviewDataTools = interviewData.ListToolsAsync().GetAwaiter().GetResult();

    //     var copilotClient = new CopilotClient();
    //     copilotClient.StartAsync().GetAwaiter().GetResult();

    //     // --- Triage Agent ---
    //     // FIX: Made state-aware to prevent re-routing loops (see Mode 2 comments).
    //     var triageAgent = copilotClient.AsAIAgent(
    //         name: "triage",
    //         instructions: """
    //             You are the Triage agent for an AI Interview Coach system.
    //             Your ONLY job is to analyze the conversation and hand off to the right specialist agent.
    //             You do NOT answer questions or conduct interviews yourself.

    //             IMPORTANT: Before routing, review the FULL conversation history to determine
    //             which phases have already been completed. Do NOT re-route to an agent that
    //             has already finished its work. The interview follows this sequence:
    //               1. Receptionist (session setup, document intake)
    //               2. Behavioural Interviewer
    //               3. Technical Interviewer
    //               4. Summariser

    //             Routing rules (apply in order, skipping completed phases):
    //             - If the receptionist has NOT yet collected the resume and job description
    //               → hand off to "receptionist"
    //             - If document intake is complete and behavioural interview has NOT started
    //               → hand off to "behavioural_interviewer"
    //             - If behavioural interview is complete and technical interview has NOT started
    //               → hand off to "technical_interviewer"
    //             - If technical interview is complete or the user wants to end
    //               → hand off to "summariser"
    //             - If the user explicitly requests a specific phase, honour that request.
    //             - If unclear, ask the user to clarify what they'd like to do.

    //             When a specialist hands back to you, they have COMPLETED their phase.
    //             Advance to the next phase in the sequence.

    //             Always be brief and supportive. Let the specialists do the detailed work.
    //             """);

    //     // --- Receptionist Agent ---
    //     // FIX: Now hands off directly to behavioural_interviewer (see Mode 2 comments).
    //     var receptionistAgent = copilotClient.AsAIAgent(
    //         name: "receptionist",
    //         instructions: """
    //             You are the Receptionist for an AI Interview Coach system.
    //             Your job is to set up the interview session and collect documents.

    //             Process:
    //             1. Fetch an existing interview session or create a new one. Let the user know their session ID.
    //             2. Ask the user to provide their resume (link or text). Use MarkItDown to parse document links into markdown.
    //             3. Ask the user to provide the job description (link or text). Use MarkItDown to parse document links into markdown.
    //             4. Store the resume and job description in the session record.
    //             5. Once document intake is complete, let the user know and hand off directly to "behavioural_interviewer"
    //                to begin the interview. Only hand off to "triage" if the user wants to do something unexpected.

    //             The user may choose to proceed without a resume or job description — that's fine.
    //             Always maintain a supportive and encouraging tone.
    //             """,
    //         tools: [.. markitdownTools, .. interviewDataTools]);

    //     // --- Behavioural Interviewer Agent ---
    //     // FIX: Now hands off directly to technical_interviewer (see Mode 2 comments).
    //     var behaviouralAgent = copilotClient.AsAIAgent(
    //         name: "behavioural_interviewer",
    //         instructions: """
    //             You are the Behavioural Interviewer for an AI Interview Coach system.
    //             Your job is to conduct the behavioural part of the interview.

    //             Process:
    //             1. Fetch the interview session record to get the resume and job description context.
    //             2. Ask behavioural questions one at a time, tailored to the job description and resume.
    //             3. After each answer, provide constructive feedback and analysis.
    //             4. Append all questions, answers, and analysis to the transcript by updating the session record.
    //             5. After a few questions (typically 3-5), ask if the user wants to continue or move on.
    //             6. When done, hand off directly to "technical_interviewer" to continue the interview.
    //                Only hand off to "triage" if the user wants to do something unexpected.

    //             Use the STAR method (Situation, Task, Action, Result) to guide your questions.
    //             Always maintain a supportive and encouraging tone.
    //             """,
    //         tools: [.. interviewDataTools]);

    //     // --- Technical Interviewer Agent ---
    //     // FIX: Now hands off directly to summariser (see Mode 2 comments).
    //     var technicalAgent = copilotClient.AsAIAgent(
    //         name: "technical_interviewer",
    //         instructions: """
    //             You are the Technical Interviewer for an AI Interview Coach system.
    //             Your job is to conduct the technical part of the interview.

    //             Process:
    //             1. Fetch the interview session record to get the resume and job description context.
    //             2. Ask technical questions one at a time, tailored to the skills in the job description and resume.
    //             3. After each answer, provide constructive feedback, correct any misconceptions, and suggest improvements.
    //             4. Append all questions, answers, and analysis to the transcript by updating the session record.
    //             5. After a few questions (typically 3-5), ask if the user wants to continue or wrap up.
    //             6. When done, hand off directly to "summariser" to generate the interview summary.
    //                Only hand off to "triage" if the user wants to do something unexpected.

    //             Focus on practical, real-world scenarios relevant to the job.
    //             Always maintain a supportive and encouraging tone.
    //             """,
    //         tools: [.. interviewDataTools]);

    //     // --- Summariser Agent ---
    //     // Hands back to Triage only after summary — end of the sequence.
    //     var summariserAgent = copilotClient.AsAIAgent(
    //         name: "summariser",
    //         instructions: """
    //             You are the Summariser for an AI Interview Coach system.
    //             Your job is to generate a comprehensive interview summary.

    //             Process:
    //             1. Fetch the interview session record to get the full transcript.
    //             2. Generate a summary that includes:
    //             - Overview of the interview session
    //             - Key highlights and strong answers
    //             - Areas for improvement
    //             - Specific recommendations for the user
    //             - Overall readiness assessment
    //             3. Update the session record with the summary in the transcript.
    //             4. Mark the interview session as complete.
    //             5. Present the summary to the user.
    //             6. Hand off back to triage in case the user wants to do anything else.

    //             Always maintain a supportive and encouraging tone.
    //             """,
    //         tools: [.. interviewDataTools]);

    //     // Build the handoff workflow — sequential chain with Triage as fallback.
    //     // FIX: Same topology change as Mode 2 (see comments above).
    //     var workflow = AgentWorkflowBuilder
    //                    .CreateHandoffBuilderWith(triageAgent)
    //                    .WithHandoffs(triageAgent, [receptionistAgent, behaviouralAgent, technicalAgent, summariserAgent])
    //                    .WithHandoffs(receptionistAgent, [behaviouralAgent, triageAgent])
    //                    .WithHandoffs(behaviouralAgent, [technicalAgent, triageAgent])
    //                    .WithHandoffs(technicalAgent, [summariserAgent, triageAgent])
    //                    .WithHandoff(summariserAgent, triageAgent)
    //                    .Build();

    //     return workflow.SetName(key);
    // }
}