using AngleSharp.Html.Parser;

namespace ConsensusApp;

internal static class ResponseParser
{
    private static readonly HtmlParser Parser = new();

    public static string GetRevisedAnswer(string response)
    {
        var doc = Parser.ParseDocument(response);
        var element = doc.QuerySelector("RevisedAnswer") ?? doc.QuerySelector("InitialResponse");
        return element?.TextContent.Trim() ?? response;
    }

    public static string GetInitialResponseSummary(string response)
    {
        var doc = Parser.ParseDocument(response);
        return doc.QuerySelector("InitialResponseSummary")?.TextContent.Trim() ?? string.Empty;
    }

    public static string GetInitialResponse(string response)
    {
        var doc = Parser.ParseDocument(response);
        var element = doc.QuerySelector("InitialResponse");
        return element?.TextContent.Trim() ?? response;
    }

    public static string GetChangesSummary(string response)
    {
        var doc = Parser.ParseDocument(response);
        return doc.QuerySelector("ChangesSummary")?.TextContent.Trim() ?? response.Trim();
    }
}
