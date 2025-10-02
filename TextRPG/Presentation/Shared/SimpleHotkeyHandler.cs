public static class SimpleHotkeyHandler
    {
        public static bool CheckForHotkeys()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                
                if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.D:
                            HandleCtrlD();
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
    }
