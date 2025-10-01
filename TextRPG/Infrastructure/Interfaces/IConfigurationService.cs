public interface IConfigurationService
{
    GameSettings GetCurrentSettings();
    void UpdateSettings(GameSettings newSettings);
    void ResetToDefault();
    void LoadFromFile(string filePath);
    void SaveToFile(string filePath);
}