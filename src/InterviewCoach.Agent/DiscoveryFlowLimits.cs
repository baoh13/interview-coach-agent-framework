namespace InterviewCoach.Agent;

public static class DiscoveryFlowLimits
{
    public const int InitialBatchSize = 5;
    public const int FollowUpBatchSize = 5;
    public const int MaxFollowUpRounds = 1;
    public const int MaxTotalAskedQuestions = InitialBatchSize + FollowUpBatchSize;
}