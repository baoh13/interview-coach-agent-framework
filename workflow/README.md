# portfolio
A repository for showing past projects IT Consultancy

## Copilot Directory Documentation
Each directory contains a bundle of Agents, Prompts, Skills and Instructions. A `README.md` should be included if more explanation required. 

## Control Hierarchy
For complex workflows, consider using an Agent as the top level controller of the workflow, with prompts and skills as components that the Agent can invoke as needed. This allows for a more modular and flexible design, where the Agent can orchestrate the flow of the workflow and make decisions about which prompts and skills to use based on the context of the task at hand.

The Agent has purpose to acheive a goal, over a skill that can be scoped to just a specific task. The Agent can invoke multiple prompts and skills as needed to achieve its goal, and can also make decisions about which prompts and skills to use based on the context of the task at hand.

```
┌─────────────────────────────────────────────────────────────┐
│                      Workflow + Purpose                     │
│                      (agents)                               │
└────────────────────┬────────────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
        ▼            ▼            ▼
    /whats-next  /fetch-ticket  /claim-ticket .... (prompts)
        │            │            │
        └────────────┼────────────┘
                     │
        ┌────────────┼──────────────────────────────┐
        │            │            │       │         │
        ▼            ▼            ▼       ▼         ▼
    jira-cli-   jira-ticket-   next   solution-  ticket-
    setup       fetching             proposal   researching ... (skills)
    
```

# Other copilot resources

* https://github.com/github/awesome-copilot [agents, prompts, skills]
* https://github.com/anthropics/skills [skills]
