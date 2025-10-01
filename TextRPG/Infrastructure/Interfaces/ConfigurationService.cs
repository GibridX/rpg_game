using System.Text.Json;

public class ConfigurationService : IConfigurationService
{
    private GameSettings _currentSettings;

    public ConfigurationService()
    {
        _currentSettings = GameSettings.CreateDefault();
    }

    public ConfigurationService(GameSettings settings)
    {
        _currentSettings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public GameSettings GetCurrentSettings() => _currentSettings;

    public void UpdateSettings(GameSettings newSettings)
    {
        _currentSettings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));
    }

    public void ResetToDefault()
    {
        _currentSettings = GameSettings.CreateDefault();
    }

    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Configuration file not found", filePath);

        var json = File.ReadAllLines(filePath);
        _currentSettings = JsonSerializer.Deserialize<GameSettings>(json)
                            ?? GameSettings.CreateDefault();
    }

    public void SaveToFile(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(filePath, json);
    }
}