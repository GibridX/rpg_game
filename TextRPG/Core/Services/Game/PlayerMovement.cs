using System;
using System.Linq;
using TextRPG.Core.Utils;
using TextRPG.Core.Enums;

namespace TextRPG.Core.Services.Game
{
    public class PlayerMovementService
    {
        public bool TryMovePlayer(Player player, Dungeon dungeon, string direction, out string message)
        {
            message = "";
            int newX = player.X, newY = player.Y;

            switch (direction.ToLower())
            {
                case "w": newY--; break;
                case "s": newY++; break;
                case "a": newX--; break;
                case "d": newX++; break;
                case "0":
                    message = "Перемещение отменено.";
                    return false;
                default:
                    message = "Неверное направление!";
                    return false;
            }

            if (newX < 0 || newX >= dungeon.Width || newY < 0 || newY >= dungeon.Height)
            {
                message = "Вы достигли границы подземелья! Дальше пути нет.";
                return false;
            }

            var newRoom = dungeon.GetRoom(newX, newY);
            if (newRoom == null)
            {
                message = "Туда нельзя двигаться! Это стена.";
                return false;
            }

            // Успешное перемещение
            player.X = newX;
            player.Y = newY;

            if (!newRoom.IsExplored)
            {
                newRoom.IsExplored = true;
                message = "Вы обнаружили новую комнату!";
            }
            else
            {
                message = $"Вы переместились в комнату ({newX}, {newY})";
            }

            return true;
        }

        public void HandleInvalidPosition(Player player, Dungeon dungeon)
        {
            ConsoleHelper.WriteColor("Ошибка: комната не найдена!", ConsoleColor.Red);

            try
            {
                var startRoom = dungeon.Rooms.FirstOrDefault(r => r.Type == RoomType.Start);
                if (startRoom != null)
                {
                    player.X = startRoom.X;
                    player.Y = startRoom.Y;
                    ConsoleHelper.WriteColor("Вы возвращены в стартовую комнату.", ConsoleColor.Yellow);
                }
                else
                {
                    var anyRoom = dungeon.Rooms.FirstOrDefault();
                    if (anyRoom != null)
                    {
                        player.X = anyRoom.X;
                        player.Y = anyRoom.Y;
                        ConsoleHelper.WriteColor("Вы перемещены в случайную комнату.", ConsoleColor.Yellow);
                    }
                    else
                    {
                        ConsoleHelper.WriteColor("Критическая ошибка: в подземелье нет комнат!", ConsoleColor.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteColor($"Ошибка при восстановлении позиции: {ex.Message}", ConsoleColor.Red);
            }
        }
    }
}