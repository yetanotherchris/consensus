namespace ConsensusApp;

internal static class Prompts
{
    public static readonly string InitialSystemPrompt =
        ResourceHelper.GetString("ConsensusApp.Resources.InitialSystemPrompt.txt");
    public static readonly string FollowupSystemPrompt =
        ResourceHelper.GetString("ConsensusApp.Resources.FollowupSystemPrompt.txt");
    public static readonly string ChangeSummarySystemPrompt =
        ResourceHelper.GetString("ConsensusApp.Resources.ChangeSummarySystemPrompt.txt");
}
