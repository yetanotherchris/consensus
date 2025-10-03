using System.Text;
using ConsensusAgent.Logging;

namespace ConsensusAgent.Services;

/// <summary>
/// Service for handling output operations (saving files)
/// </summary>
public class OutputService : IOutputService
{
    private readonly SimpleLogger _logger;

    public OutputService(SimpleLogger logger)
    {
        _logger = logger;
    }

    public async Task SaveConsensusAsync(string consensus, string filePath)
    {
        _logger.LogInformation("Saving consensus to file: {0}", filePath);
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await File.WriteAllTextAsync(filePath, consensus);
        
        _logger.LogInformation("Consensus saved successfully");
    }
}
