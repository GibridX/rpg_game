using System;
using System.Linq;
using TextRPG.Config;
using TextRPG.Core.Services;
using TextRPG.Core.Factory;
using TextRPG.Core.Utils;
using TextRPG.Core.Enums;
using TextRPG.Core.Services.Game;
using TextRPG.Core.Services.UI;
using TextRPG.Core.Services.Generation;

namespace TextRPG
{
    public class Game
    {
        private Player player = null!;
        private Dungeon currentDungeon = null!;
        private GameConfig _config;
        private LevelGenerator _levelGenerator;
        private Random random;
        private ICombatService _combatService;
        private IEnemyFactory _enemyFactory;
        private GameStateService _gameStateService;
        private MerchantService _merchantService;
        private GameUIService _gameUIService;
        private RoomService _roomService;
        private PlayerMovementService _playerMovementService;
        private GameScreenService _gameScreenService;
        private bool needsClear = true;
        private int currentDepth = 1;
        private bool bossDefeated = false;

        public Game()
        {
            _config = new GameConfig();
            _levelGenerator = new LevelGenerator(12, 12);
            random = new Random();
            _combatService = new CombatService(_config);
            _enemyFactory = new EnemyFactory();
            _gameStateService = new GameStateService(_config);
            _merchantService = new MerchantService(_config);
            _gameUIService = new GameUIService();
            _playerMovementService = new PlayerMovementService();
            _gameScreenService = new GameScreenService();

            _roomService = new RoomService(_config, _enemyFactory, _combatService, _merchantService);
            InitializeGame();
        }

        public bool IsExitActive()
        {
            return bossDefeated;
        }

        private void InitializeGame(bool isNewGame = true)
        {
            if (isNewGame)
            {
                player = new Player("Герой", 0, 0);
                currentDepth = 1;
                bossDefeated = false;
            }
            else
            {
                ConsoleHelper.WriteColor($"\nВы спускаетесь на новые грубины подземелья {currentDepth}...", ConsoleColor.Cyan);
                bossDefeated = false;
            }

            currentDungeon = _levelGenerator.GenerateDungeon($"Древний склеп - Уровень {currentDepth}", 20, currentDepth);

            var startRoom = currentDungeon.Rooms.FirstOrDefault(r => r.Type == RoomType.Start);
            if (startRoom == null)
            {
                startRoom = currentDungeon.Rooms.FirstOrDefault();
                if (startRoom == null)
                {
                    startRoom = new Room(0, 0, RoomType.Start);
                    currentDungeon.AddRoom(startRoom);
                }
                else
                {
                    startRoom.Type = RoomType.Start;
                }
            }
            player.X = startRoom.X;
            player.Y = startRoom.Y;
            startRoom.IsExplored = true;

            if (!isNewGame)
            {
                ConsoleHelper.WriteColor($"Вы вошли в подземелье уровня {currentDepth}. Будьте осторожны!", ConsoleColor.Yellow);
            }
        }

        private void ClearIfNeeded()
        {
            if (needsClear)
            {
                Console.Clear();
                _gameStateService.DisplayGameHeader(currentDepth);
                needsClear = false;
            }
        }

        private void MarkForClear()
        {
            needsClear = true;
        }

        public void Start()
        {
            try
            {
                _gameStateService.DisplayGameHeader(currentDepth);
                ConsoleHelper.WriteColor("Нажмите любую клавишу чтобы начать...", ConsoleColor.Green);
                Console.ReadKey(true);

                InitializeGame(isNewGame: true);

                while (player.Health > 0)
                {
                    ClearIfNeeded();
                    _gameStateService.DisplayCompactStatusBar(player, currentDepth, currentDungeon);
                    currentDungeon.DisplayMiniMap(player, bossDefeated);

                    if (!ProcessPlayerInput())
                        break;

                    if (player.Health <= 0)
                    {
                        ShowGameOverScreen();
                        return;
                    }

                    MarkForClear();
                }

                if (player.Health <= 0)
                {
                    ShowGameOverScreen();
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteColor($"Критическая ошибка: {ex.Message}", ConsoleColor.Red);
                Console.ReadKey();
            }
        }

        private bool ProcessPlayerInput()
        {
            if (HotkeyHandler.CheckForHotkeys())
            {
                MarkForClear();
                return true;
            }

            _gameUIService.ShowMainMenu();
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    MovePlayer();
                    break;
                case "2":
                    _gameUIService.ShowInventory(player);
                    MarkForClear();
                    break;
                case "3":
                    _gameUIService.ShowCharacterInfo(player, currentDepth);
                    MarkForClear();
                    break;
                case "4":
                    return false;
                default:
                    ConsoleHelper.WriteColor("Неверный выбор! Нажмите любую клавишу...", ConsoleColor.Red);
                    Console.ReadKey(true);
                    MarkForClear();
                    break;
            }

            return true;
        }
    
        private void MovePlayer()
        {
            ConsoleHelper.WriteColor("\n Куда двигаемся?", ConsoleColor.Cyan);
            ConsoleHelper.WriteColor("W - вверх, S - вниз, A - влево, D - вправо, 0 - отмена", ConsoleColor.White);

            var direction = Console.ReadLine();

            if (string.IsNullOrEmpty(direction))
            {
                ConsoleHelper.WriteColor("Неверное направление!", ConsoleColor.Red);
                return;
            }

            if (_playerMovementService.TryMovePlayer(player, currentDungeon, direction, out string message))
            {
                ConsoleHelper.WriteColor(message, 
                    message.Contains("обнаружили") ? ConsoleColor.Green : ConsoleColor.Gray);
                ProcessCurrentRoom();
            }
            else
            {
                if (!string.IsNullOrEmpty(message))
                {
                    ConsoleHelper.WriteColor(message, ConsoleColor.Red);
                }
                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
            }
        }

        private void ProcessCurrentRoom()
        {
            var currentRoom = currentDungeon.GetRoom(player.X, player.Y);
            if (currentRoom == null)
            {
                _playerMovementService.HandleInvalidPosition(player, currentDungeon);
                return;
            }

            _roomService.ProcessRoom(currentRoom, player, currentDungeon, ref bossDefeated, OnDepthIncrease);
        }

        private void OnDepthIncrease(int depthIncrease)
        {
            currentDepth += depthIncrease;
            InitializeGame(isNewGame: false);
            MarkForClear();
        }

        private void ShowGameOverScreen()
        {
            _gameScreenService.ShowGameOverScreen(player, currentDepth);
            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
        }

        private bool ShowVictoryScreen()
        {
            _gameScreenService.ShowVictoryScreen(player, currentDepth);
            return _gameScreenService.GetVictoryChoice();
        }
    }
}