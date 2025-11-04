using System;

namespace TextRPG.Core.Utils
{
    public static class HotkeyHandler
    {
        private static bool _hotkeyProcessed = false;

        public static bool CheckForHotkeys()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);

                // Обработка Ctrl+ комбинаций
                if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.D:
                            HandleCtrlD();
                            return true;
                    }
                }
                // Обработка обычных горячих клавиш (без Ctrl)
                else
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.I:
                            HandleInventoryKey();
                            return true;
                        case ConsoleKey.H:
                            HandleHealthPotionKey();
                            return true;
                        case ConsoleKey.C:
                            HandleCharacterInfoKey();
                            return true;
                        case ConsoleKey.E:
                            HandleEquipmentKey();
                            return true;
                        case ConsoleKey.Escape:
                            HandleEscapeKey();
                            return true;
                    }
                }
            }
            return false;
        }

        private static void HandleCtrlD()
        {
            Console.WriteLine();
            ConsoleHelper.WriteColor("[Ctrl+D] Отладочная информация - функция в разработке", ConsoleColor.Yellow);
            ConsoleHelper.WriteColor("Нажмите любую клавишу чтобы продолжить...", ConsoleColor.Gray);
            Console.ReadKey(true);
        }

        // Горячие клавиши без Ctrl для быстрого доступа
        private static void HandleInventoryKey()
        {
            Console.WriteLine();
            ConsoleHelper.WriteColor("[I] Быстрый доступ к инвентарю", ConsoleColor.Green);
            ConsoleHelper.WriteColor("Нажмите любую клавишу чтобы продолжить...", ConsoleColor.Gray);
            _hotkeyProcessed = true;
        }

        private static void HandleHealthPotionKey()
        {
            Console.WriteLine();
            ConsoleHelper.WriteColor("[H] Быстрое использование зелья здоровья", ConsoleColor.Green);
            ConsoleHelper.WriteColor("Нажмите любую клавишу чтобы продолжить...", ConsoleColor.Gray);
            _hotkeyProcessed = true;
        }

        private static void HandleCharacterInfoKey()
        {
            Console.WriteLine();
            ConsoleHelper.WriteColor("[C] Информация о персонаже", ConsoleColor.Green);
            ConsoleHelper.WriteColor("Нажмите любую клавишу чтобы продолжить...", ConsoleColor.Gray);
            _hotkeyProcessed = true;
        }

        private static void HandleEquipmentKey()
        {
            Console.WriteLine();
            ConsoleHelper.WriteColor("[E] Статистика экипировки", ConsoleColor.Green);
            ConsoleHelper.WriteColor("Нажмите любую клавишу чтобы продолжить...", ConsoleColor.Gray);
            _hotkeyProcessed = true;
        }

        private static void HandleEscapeKey()
        {
            Console.WriteLine();
            ConsoleHelper.WriteColor("[ESC] Возврат в главное меню", ConsoleColor.Green);
            ConsoleHelper.WriteColor("Нажмите любую клавишу чтобы продолжить...", ConsoleColor.Gray);
            _hotkeyProcessed = true;
        }

        public static string GetHotkeyHelp()
        {
            return @"
Горячие клавиши:
  I - Инвентарь
  H - Использовать зелье здоровья
  C - Информация о персонаже
  E - Статистика экипировки";
        }

        public static bool WasHotkeyProcessed()
        {
            var processed = _hotkeyProcessed;
            _hotkeyProcessed = false;
            return processed;
        }
    }
}