using System;
using System.Linq;
using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Factory;
using TextRPG.Core.Models;
using TextRPG.Core.Services.Generation;
using TextRPG.Core.Services.UI;
using TextRPG.Core.Utils;

namespace TextRPG.Core.Services.Game
{
    public class RoomService : IRoomService
    {
        private readonly GameConfig _config;
        private readonly IEnemyFactory _enemyFactory;
        private readonly ICombatService _combatService;
        private readonly MerchantService _merchantService;
        private readonly GameScreenService _gameScreenService;
        private readonly Random _random;

        public RoomService(GameConfig config, IEnemyFactory enemyFactory, ICombatService combatService, MerchantService merchantService, GameScreenService gameScreenService)
        {
            _config = config;
            _enemyFactory = enemyFactory;
            _combatService = combatService;
            _merchantService = merchantService;
            _gameScreenService = gameScreenService;
            _random = new Random();
        }

        public void ProcessRoom(Room room, Player player, Dungeon dungeon, ref bool bossDefeated, Action<int> onDepthIncrease)
        {
            if (room.Type != RoomType.Rest)
            {
                player.IncrementTurnCounter();
            }

            DisplayRoomHeader(room);

            switch (room.Type)
            {
                case RoomType.Enemy:
                    Combat(player, room);
                    break;
                case RoomType.Boss:
                    BossFight(player, room, ref bossDefeated, onDepthIncrease);
                    break;
                case RoomType.Treasure:
                    LootRoom(player, room);
                    break;
                case RoomType.Merchant:
                    VisitMerchant(player);
                    break;
                case RoomType.Rest:
                    Rest(player, room);
                    break;
                case RoomType.Exit:
                    HandleExit(player, onDepthIncrease);
                    break;
            }
        }

        private void DisplayRoomHeader(Room room)
        {
            var color = room.Type switch
            {
                RoomType.Enemy => ConsoleColor.Red,
                RoomType.Boss => ConsoleColor.DarkRed,
                RoomType.Treasure => ConsoleColor.Yellow,
                RoomType.Merchant => ConsoleColor.DarkYellow,
                RoomType.Rest => ConsoleColor.Green,
                RoomType.Exit => ConsoleColor.Blue,
                _ => ConsoleColor.Gray
            };

            ConsoleHelper.WriteColor($"\n{GetRoomDescription(room)}", color);
        }

        private string GetRoomDescription(Room room)
        {
            return room.Type switch
            {
                RoomType.Enemy => "Комната с противником",
                RoomType.Boss => "Логово Босса",
                RoomType.Treasure => "Сокровищница",
                RoomType.Merchant => "Лагерь торговца",
                RoomType.Rest => "Комната отдыха",
                RoomType.Exit => "Выход из подземелья",
                RoomType.Empty => "Пустая комната",
                _ => "Неизвестная комната"
            };
        }

        private void Combat(Player player, Room room)
        {
            var enemy = _enemyFactory.CreateRegularEnemy(player.X, player.Y, player.Level, 1);

            var combatResult = _combatService.StartCombat(player, enemy);

            if (combatResult.PlayerWon)
            {
                player.AddExperience(combatResult.ExperienceGained);
                player.AddGold(combatResult.GoldGained);

                room.Type = RoomType.Empty;

                if (room.Items.Any())
                {
                    LootItems(player, room);
                }

                string[] defeatPhrases =
                {
                    $"{enemy.Name} повержен!",
                    $"{enemy.Name} побежден!",
                    $"Вы одолели {enemy.Name}!",
                    $"{enemy.Name} был побежден в бою!"
                };

                ConsoleHelper.WriteColor($" {defeatPhrases[_random.Next(defeatPhrases.Length)]}", ConsoleColor.Green);
            }
            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
        }

        private void BossFight(Player player, Room room, ref bool bossDefeated, Action<int> onDepthIncrease)
        {
            var boss = _enemyFactory.CreateBoss(player.X, player.Y, player.Level, 1);

            var combatResult = _combatService.StartBossFight(player, boss);

            if (combatResult.PlayerWon)
            {
                Console.WriteLine($"Босс {boss.Name} побеждён!");
                bossDefeated = true;

                if (room.Items.Any())
                {
                    LootItems(player, room);
                }

                room.Type = RoomType.Exit;
                _gameScreenService.ShowVictoryScreen(player);
                ConsoleHelper.WriteColor("Портал выхода активирован! Теперь вы можете покинуть подземелье.", ConsoleColor.Yellow);
                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
            }
            else
            {
                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
            }
        }

        private void LootRoom(Player player, Room room)
        {
            if (room.Items.Any())
            {
                LootItems(player, room);
                room.Type = RoomType.Empty;
            }
            else
            {
                Console.WriteLine("Сокровищница пуста.");
                room.Type = RoomType.Empty;
            }

            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
        }

        private void LootItems(Player player, Room room)
        {
            foreach (var item in room.Items.ToList())
            {
                var inventoryItem = item.CreateCopy(-1, -1);
                player.AddItem(inventoryItem);
                room.Items.Remove(item);
                Console.WriteLine($"Вы нашли {item.Name}!");
            }
        }

        private void VisitMerchant(Player player)
        {
            ConsoleHelper.WriteColor("\nВстреча с торговцем", ConsoleColor.DarkYellow);
            ConsoleHelper.WriteColor("'Приветствую, путник! Хочешь взглянуть на мой товар?'", ConsoleColor.Yellow);

            var merchantItems = _merchantService.CreateMerchantItems();

            bool trading = true;
            while (trading && player.Health > 0)
            {
                _merchantService.DisplayMerchantUI(player, merchantItems);
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    ConsoleHelper.WriteColor("Неверный ввод!", ConsoleColor.Red);
                    continue;
                }

                if (input == "0")
                {
                    trading = false;
                }
                else

                {
                    trading = _merchantService.ProcessMerchantInput(input, player, merchantItems);

                    if (trading)
                    {
                        Console.WriteLine("\nНажмите любую клавишу чтобы продолжить...");
                        Console.ReadKey(true);
                    }
                }
            }

            if (player.Health > 0)
            {
                ConsoleHelper.WriteColor("'Возвращайся, если понадобятся припасы!'", ConsoleColor.Yellow);
            }
        }

        private void Rest(Player player, Room room)
        {
            ConsoleHelper.WriteColor("\n Комната отдыха", ConsoleColor.DarkGreen);

            if (player.TurnsSinceLastRest >= 0 && player.TurnsSinceLastRest < _config.RestRoomCooldown)
            {
                int turnsRemaining = _config.RestRoomCooldown - player.TurnsSinceLastRest;
                ConsoleHelper.WriteColor($"Комната наполняется исцеляющей магией... Вернитесь через {turnsRemaining} ходов.", ConsoleColor.Yellow);
                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
                return;
            }

            ConsoleHelper.WriteColor("Здесь царит спокойная атмосфера. Вы чувствуете, как силы возвращаются к вам.", ConsoleColor.Green);

            int healAmount = player.MaxHealth - player.Health;
            if (healAmount > 0)
            {
                player.Heal(healAmount);
                ConsoleHelper.WriteColor($"Вы полностью восстановили здоровье! +{healAmount} HP", ConsoleColor.Green);
            }
            else
            {
                ConsoleHelper.WriteColor("Ваше здоровье уже на максимуме.", ConsoleColor.Gray);
            }

            player.ActivateRestCooldown();
            room.Type = RoomType.Empty;
            room.Description = "Пустая комната отдыха. Исцеляющая магия иссякла.";

            Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
            Console.ReadKey(true);
        }

        private void HandleExit(Player player, Action<int> onDepthIncrease)
        {
            if (_gameScreenService.ShowVictoryScreen(player))
            {
                onDepthIncrease?.Invoke(1);
            }
            else
            {
                ConsoleHelper.WriteColor("Спасибо за игру! До новых встреч!", ConsoleColor.Green);
            }
        }

    }
}