namespace Consensus.Configuration;

/// <summary>
/// Exception thrown when there are errors in application settings validation or loading.
/// </summary>
public class SettingsException : Exception
{
    public SettingsException(string message) : base(message)
    {
    }

    public SettingsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
