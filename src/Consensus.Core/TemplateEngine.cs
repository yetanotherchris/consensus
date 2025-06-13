using Scriban;

namespace Consensus.Core;

internal static class TemplateEngine
{
    public static string Render(string template, object values)
    {
        var scribanTemplate = Template.Parse(template);
        return scribanTemplate.Render(values, member => member.Name);
    }
}
