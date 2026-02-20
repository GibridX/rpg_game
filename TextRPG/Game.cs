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
using TextRPG.Core.Models.Inventory.Equipment;

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
        private EquipmentService _equipmentService;
        private bool needsClear = true;
        private int currentDepth = 1;
        private bool bossDefeated = false;

        private string lastMovementMessage = "";

        public Game()
        {
            player = new Player("Герой", 0, 0);
            _equipmentService = new EquipmentService(player);
            _config = new GameConfig();
            _levelGenerator = new LevelGenerator(12, 12);
            random = new Random();
            _combatService = new CombatService(_config);
            _enemyFactory = new EnemyFactory();
            _gameStateService = new GameStateService(_config);
            _merchantService = new MerchantService(_config);
            _gameUIService = new GameUIService(_equipmentService);
            _playerMovementService = new PlayerMovementService();
            _gameScreenService = new GameScreenService();

            _roomService = new RoomService(_config, _enemyFactory, _combatService, _merchantService, _gameScreenService);
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

                    if (!string.IsNullOrEmpty(lastMovementMessage))
                    {
                        if (lastMovementMessage.Contains("стена") || lastMovementMessage.Contains("границ"))
                            ConsoleHelper.WriteColor(lastMovementMessage, ConsoleColor.Red);
                        else if (lastMovementMessage.Contains("обнаружили"))
                            ConsoleHelper.WriteColor(lastMovementMessage, ConsoleColor.Green);
                        else
                            ConsoleHelper.WriteColor(lastMovementMessage, ConsoleColor.Yellow);

                        lastMovementMessage = "";
                        Console.WriteLine();
                    }

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
            _gameUIService.ShowMainMenu();

            var key = Console.ReadKey(true);
            Console.WriteLine();

            if (IsMovementKey(key.Key))
            {
                HandleMovement(key.Key);
                return true;
            }

            switch (key.Key)
            {
                case ConsoleKey.I:
                    _gameUIService.ShowActionFeedback("inventory");
                    _gameUIService.ShowInventoryManagement(player);
                    MarkForClear();
                    break;
                case ConsoleKey.H:
                    _gameUIService.ShowActionFeedback("potion");
                    _gameUIService.ShowQuickPotionMenu(player);
                    break;
                case ConsoleKey.C:
                    _gameUIService.ShowActionFeedback("character");
                    _gameUIService.ShowCharacterInfo(player, currentDepth);
                    MarkForClear();
                    break;
                case ConsoleKey.E:
                    _gameUIService.ShowActionFeedback("equipment");
                    _gameUIService.ShowEquipmentStats(player);
                    _gameUIService.ShowContinuePrompt();
                    Console.ReadKey(true);
                    MarkForClear();
                    break;
                case ConsoleKey.Escape:
                    _gameUIService.ShowActionFeedback("exit");
                    return false;
                default:
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        switch (key.Key)
                        {
                            case ConsoleKey.D:
                                _gameUIService.ShowActionFeedback("debug");
                                _gameUIService.ShowWarningMessage("[Ctrl+D] Функция в разработке");
                                _gameUIService.ShowContinuePrompt();
                                Console.ReadKey(true);
                                MarkForClear();
                                break;
                            default:
                                _gameUIService.ShowErrorMessage("Неизвестная комбинация клавиш!");
                                _gameUIService.ShowContinuePrompt();
                                Console.ReadKey(true);
                                MarkForClear();
                                break;
                        }
                    }
                    else
                    {
                        _gameUIService.ShowErrorMessage("Неизвестная команда!");
                        _gameUIService.ShowWarningMessage("Используйте клавиши I, H, C, E, M или ESC");
                        _gameUIService.ShowContinuePrompt();
                        Console.ReadKey(true);
                        MarkForClear();
                    }
                    break;
            }

            return true;
        }

        private bool IsMovementKey(ConsoleKey key)
        {
            return key == ConsoleKey.W || key == ConsoleKey.A ||
                key == ConsoleKey.S || key == ConsoleKey.D;
        }

        private void HandleMovement(ConsoleKey key)
        {
            string direction = key.ToString().ToLower();

            if (_playerMovementService.TryMovePlayer(player, currentDungeon, direction, out string message))
            {
                lastMovementMessage = message;
                ProcessCurrentRoom();
            }
            else
            {
                if (!string.IsNullOrEmpty(message))
                {
                    lastMovementMessage = message;
                }
            }
        }

        // Неиспользуется, оставлен для обратной совместимости
        private void MovePlayer()
        {
            _gameUIService.ShowMovementMenu();

            var direction = Console.ReadLine()?.ToLower();

            if (string.IsNullOrEmpty(direction))
            {
                _gameUIService.ShowErrorMessage("Неверное направление!");
                _gameUIService.ShowContinuePrompt();
                Console.ReadKey(true);
                return;
            }

            // Обработка быстрого использования зелья
            if (direction == "h")
            {
                _gameUIService.ShowActionFeedback("potion");
                _gameUIService.ShowQuickPotionMenu(player);
                MarkForClear();
                return;
            }

            if (direction == "escape" || direction == "esc")
            {
                _gameUIService.ShowInfoMessage("Возврат в главное меню...");
                return;
            }

            if (_playerMovementService.TryMovePlayer(player, currentDungeon, direction, out string message))
            {
                _gameUIService.ShowSuccessMessage(message);
                ProcessCurrentRoom();
            }
            else
            {
                if (!string.IsNullOrEmpty(message))
                {
                    _gameUIService.ShowErrorMessage(message);
                }
                _gameUIService.ShowContinuePrompt();
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

            _gameUIService.ShowInventorySummary(player);

            _gameUIService.ShowContinuePrompt();
            Console.ReadKey(true);
        }
    }
}