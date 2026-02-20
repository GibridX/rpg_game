using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Utils;
using System.Text;

namespace TextRPG.Core.Services.Game
{
    public class GameStateService
    {
        private readonly GameConfig _config;
        private readonly StringBuilder _displayBuilder;
        private readonly char[] _healthBarBuffer;
        private readonly char[] _expBarBuffer;
        private const int BarWidth = 20;
        private const int StatusBarWidth = 66;

        // Кэшированные шаблоны
        private readonly string _headerTemplate;
        private readonly string _statusBarTop;
        private readonly string _statusBarBottom;

        // Структура для данных статус-бара
        private struct StatusBarData
        {
            public PlayerStats Stats;
            public ProgressBars Bars;
            public CooldownInfo Cooldowns;
        }

        private struct PlayerStats
        {
            public int Health;
            public int MaxHealth;
            public int Attack;
            public int Level;
            public int Gold;
            public int Experience;
            public int ExperienceToNextLevel;
        }

        private struct ProgressBars
        {
            public string HealthBar;
            public string ExpBar;
            public double HealthPercent;
            public double ExpPercent;
        }

        private struct CooldownInfo
        {
            public string RestStatus;
            public bool IsRestReady;
            public int TurnsUntilRest;
        }

        public GameStateService(GameConfig config)
        {
            _config = config;
            _displayBuilder = new StringBuilder(512);
            _healthBarBuffer = new char[BarWidth];
            _expBarBuffer = new char[BarWidth];
            
            _headerTemplate = @"
╔══════════════════════════════════════════════════════════════╗
║                   ТЕКСТОВАЯ RPG - ПОДЗЕМЕЛЬЕ                 ║
║                 Древний Склеп - Глубина: {0,2}                  ║
╚══════════════════════════════════════════════════════════════╝";

            _statusBarTop = "╔════════════════════════════════════════════════════════════════╗";
            _statusBarBottom = "╚════════════════════════════════════════════════════════════════╝";
            
            InitializeBarBuffers();
        }

        private void InitializeBarBuffers()
        {
            for (int i = 0; i < BarWidth; i++)
            {
                _healthBarBuffer[i] = '░';
                _expBarBuffer[i] = '░';
            }
        }

        public void DisplayGameHeader(int currentDepth)
        {
            ConsoleHelper.WriteColor(string.Format(_headerTemplate, currentDepth), ConsoleColor.Cyan);
        }

        public void DisplayCompactStatusBar(Player player, int currentDepth, Dungeon currentDungeon)
        {
            // Подготовка данных
            var statusData = PrepareStatusData(player);
            
            // Очистка билдера
            _displayBuilder.Clear();

            // Построение интерфейса
            BuildStatusBarInterface(statusData);
            
            // Вывод
            Console.Write(_displayBuilder.ToString());
            
            // Цветное наложение
            DisplayColoredStatusOverlay(statusData);
            
            // Информация о комнате
            DisplayRoomInfo(player, currentDungeon);
        }

        private StatusBarData PrepareStatusData(Player player)
        {
            var data = new StatusBarData();
            
            // Статистика игрока
            data.Stats = new PlayerStats
            {
                Health = player.Health,
                MaxHealth = player.MaxHealth,
                Attack = player.Attack,
                Level = player.Level,
                Gold = player.Gold,
                Experience = player.Experience,
                ExperienceToNextLevel = player.ExperienceToNextLevel
            };

            // Прогресс-бары
            data.Stats = new PlayerStats
            {
                Health = player.Health,
                MaxHealth = player.MaxHealth,
                Attack = player.Attack,
                Level = player.Level,
                Gold = player.Gold,
                Experience = player.Experience,
                ExperienceToNextLevel = player.ExperienceToNextLevel
            };

            data.Bars = new ProgressBars
            {
                HealthPercent = (double)player.Health / player.MaxHealth,
                ExpPercent = (double)player.Experience / player.ExperienceToNextLevel,
                HealthBar = CreateProgressBar((double)player.Health / player.MaxHealth, _healthBarBuffer),
                ExpBar = CreateProgressBar((double)player.Experience / player.ExperienceToNextLevel, _expBarBuffer)
            };

            // Кулдауны
            data.Cooldowns = PrepareCooldownInfo(player);

            return data;
        }

        private CooldownInfo PrepareCooldownInfo(Player player)
        {
            bool isRestReady = player.TurnsSinceLastRest == -1 || 
                              player.TurnsSinceLastRest >= _config.RestRoomCooldown;
            
            return new CooldownInfo
            {
                IsRestReady = isRestReady,
                TurnsUntilRest = isRestReady ? 0 : _config.RestRoomCooldown - player.TurnsSinceLastRest,
                RestStatus = isRestReady ? " Отдых: готов" : $" Отдых: {_config.RestRoomCooldown - player.TurnsSinceLastRest}"
            };
        }

        private void BuildStatusBarInterface(StatusBarData data)
        {
            // Верхняя граница
            _displayBuilder.AppendLine(_statusBarTop);

            // Первая строка: Основные характеристики
            BuildStatsLine(data);
            
            // Вторая строка: Прогресс и кулдауны
            BuildProgressLine(data);

            // Нижняя граница
            _displayBuilder.AppendLine(_statusBarBottom);
        }

        private void BuildStatsLine(StatusBarData data)
        {
            _displayBuilder.Append("║");
            
            // Группа здоровья
            string healthSection = $"HP: {data.Stats.Health,3}/{data.Stats.MaxHealth,3} [{data.Bars.HealthBar}]";
            
            // Группа боевых характеристик
            string combatSection = $"АТК: {data.Stats.Attack,2}";
            
            // Групка прогресса
            string progressSection = $"УР: {data.Stats.Level,2}";
            
            // Группа ресурсов
            string resourcesSection = $"Золото: {data.Stats.Gold,3}";

            // Собираем строку
            _displayBuilder.Append(healthSection).Append(" ");
            _displayBuilder.Append(combatSection).Append(" ");
            _displayBuilder.Append(progressSection).Append(" ");
            _displayBuilder.Append(resourcesSection);
            
            // Выравнивание
            AlignLine(healthSection.Length + combatSection.Length + progressSection.Length + resourcesSection.Length + 3);
        }

        private void BuildProgressLine(StatusBarData data)
        {
            _displayBuilder.Append("║");
            
            // Группа опыта
            string expSection = $"Опыт: {data.Stats.Experience,3}/{data.Stats.ExperienceToNextLevel,3} [{data.Bars.ExpBar}]";
            
            // Группа кулдаунов
            string cooldownSection = data.Cooldowns.RestStatus;

            // Собираем строку
            _displayBuilder.Append(expSection).Append(cooldownSection);
            
            // Выравнивание
            AlignLine(expSection.Length + cooldownSection.Length);
        }

        private void AlignLine(int contentLength)
        {
            int spacesNeeded = StatusBarWidth - 2 - contentLength;
            if (spacesNeeded > 0)
            {
                _displayBuilder.Append(new string(' ', spacesNeeded));
            }
            _displayBuilder.AppendLine("║");
        }

        private string CreateProgressBar(double percent, char[] buffer)
        {
            int filledWidth = (int)(BarWidth * percent);
            
            for (int i = 0; i < BarWidth; i++)
            {
                buffer[i] = i < filledWidth ? '█' : '░';
            }
            
            return new string(buffer);
        }

        private void DisplayColoredStatusOverlay(StatusBarData data)
        {
            int originalLeft = Console.CursorLeft;
            int originalTop = Console.CursorTop;

            // Перемещаемся к статус-бару
            Console.SetCursorPosition(0, originalTop - 3);

            // Первая строка с цветом
            DisplayColoredStatsLine(data);
            
            // Вторая строка с цветом
            DisplayColoredProgressLine(data);

            // Возвращаем курсор
            Console.SetCursorPosition(originalLeft, originalTop);
        }

        private void DisplayColoredStatsLine(StatusBarData data)
        {
            Console.Write("║");
            
            // Здоровье (красный)
            ConsoleHelper.WriteColorInline($"HP: {data.Stats.Health,3}/{data.Stats.MaxHealth,3} ", ConsoleColor.White);
            ConsoleHelper.WriteColorInline($"[{data.Bars.HealthBar}] ", ConsoleColor.Red);
            
            // Атака (желтый)
            ConsoleHelper.WriteColorInline($"АТК: {data.Stats.Attack,2} ", ConsoleColor.Yellow);
            
            // Уровень (голубой)
            ConsoleHelper.WriteColorInline($"УР: {data.Stats.Level,2} ", ConsoleColor.Cyan);
            
            // Золото (желтый)
            ConsoleHelper.WriteColorInline($"Золото: {data.Stats.Gold,3}", ConsoleColor.Yellow);
            
            // Выравнивание
            int textLength = $"HP: {data.Stats.Health,3}/{data.Stats.MaxHealth,3} [{data.Bars.HealthBar}] АТК: {data.Stats.Attack,2} УР: {data.Stats.Level,2} Золото: {data.Stats.Gold,3}".Length;
            AlignColoredLine(textLength);
        }

        private void DisplayColoredProgressLine(StatusBarData data)
        {
            Console.Write("║");
            
            // Опыт (синий)
            ConsoleHelper.WriteColorInline($"Опыт: {data.Stats.Experience,3}/{data.Stats.ExperienceToNextLevel,3} ", ConsoleColor.White);
            ConsoleHelper.WriteColorInline($"[{data.Bars.ExpBar}]", ConsoleColor.Blue);
            
            // Отдых (зеленый/желтый)
            var restColor = data.Cooldowns.IsRestReady ? ConsoleColor.DarkGreen : ConsoleColor.DarkYellow;
            ConsoleHelper.WriteColorInline(data.Cooldowns.RestStatus, restColor);
            
            // Выравнивание
            int textLength = $"Опыт: {data.Stats.Experience,3}/{data.Stats.ExperienceToNextLevel,3} [{data.Bars.ExpBar}]{data.Cooldowns.RestStatus}".Length;
            AlignColoredLine(textLength);
        }

        private void AlignColoredLine(int contentLength)
        {
            int spacesNeeded = StatusBarWidth - 2 - contentLength;
            if (spacesNeeded > 0)
            {
                Console.Write(new string(' ', spacesNeeded));
            }
            Console.WriteLine("║");
        }

        private void DisplayRoomInfo(Player player, Dungeon currentDungeon)
        {
            var currentRoom = currentDungeon.GetRoom(player.X, player.Y);
            if (currentRoom != null)
            {
                Console.WriteLine();
                ConsoleHelper.WriteColorInline($"{currentRoom.Description} ", GetRoomTitleColor(currentRoom.Type));
                ConsoleHelper.WriteColorInline($"({player.X}, {player.Y})", ConsoleColor.Gray);
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        private ConsoleColor GetRoomTitleColor(RoomType type)
        {
            return type switch
            {
                RoomType.Start => ConsoleColor.Blue,
                RoomType.Enemy => ConsoleColor.Red,
                RoomType.Treasure => ConsoleColor.Magenta,
                RoomType.Boss => ConsoleColor.DarkRed,
                RoomType.Empty => ConsoleColor.Gray,
                _ => ConsoleColor.White
            };
        }
    }
}