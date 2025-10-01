using TextRPG;

public class PlayerService
{
    private readonly PlayerService _playerSettings;

    public PlayerService(IConfigurationService configService)
    {
        _playerSettings = configService.GetCurrentSettings().Player;
    }

    public Player CreateNewPlayer(string name)
    {
        return new Player(name, 0, 0)
        {
            BaseMaxHealth = _playerSettings.BaseHealth,
            MaxHealth = _playerSettings.BaseHealth,
            Health = _playerSettings.BaseHealth,
            BaseAttack = _playerSettings.BaseAttack,
            Attack = _playerSettings.BaseAttack,
            ExperienceToNextLevel = 50
        };
    }
}