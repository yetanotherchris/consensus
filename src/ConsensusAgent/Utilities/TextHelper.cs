namespace ConsensusAgent.Utilities;

/// <summary>
/// Helper methods for text manipulation
/// </summary>
public static class TextHelper
{
    public static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + "...";
    }
}
