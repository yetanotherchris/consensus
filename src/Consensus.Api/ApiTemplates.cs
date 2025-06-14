namespace Consensus.Api;

using Consensus.Core;

internal sealed class ApiTemplates : ITemplate
{
    public string QueryingTemplate => ResourceHelper.GetString("Consensus.Api.Resources.QueryingTemplate.md");
    public string ModelSummaryTemplate => ResourceHelper.GetString("Consensus.Api.Resources.ModelSummaryTemplate.md");
    public string InitialAnswerTemplate => ResourceHelper.GetString("Consensus.Api.Resources.InitialAnswerTemplate.md");
    public string AnswerTemplate => ResourceHelper.GetString("Consensus.Api.Resources.AnswerTemplate.md");
    public string ResponseTemplate => ResourceHelper.GetString("Consensus.Api.Resources.ResponseTemplate.md");
}
