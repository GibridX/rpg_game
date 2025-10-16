using System;
using System.Threading;
using TextRPG;

namespace TextRPG.Core.Services.Application
{
    public class ApplicationRunner
    {
        public void Run()
        {
            Console.Title = "Текстовая RPG - Подземелье";
            Console.CursorVisible = true;

            int attempts = 0;
            const int maxAttempts = 3;
            bool gameCompletedSuccessfully = false;

            while (attempts < maxAttempts && !gameCompletedSuccessfully)
            {
                try
                {
                    var game = new TextRPG.Game();
                    game.Start();
                    gameCompletedSuccessfully = true;
                }
                catch (Exception ex)
                {
                    attempts++;
                    HandleGameError(ex, attempts, maxAttempts);
                }
            }

            if (gameCompletedSuccessfully)
            {
                Console.WriteLine("\nИгра завершена успешно! Спасибо за игру!");
            }
        }

        private void HandleGameError(Exception ex, int attempts, int maxAttempts)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Произошла ошибка в игре (попытка {attempts}/{maxAttempts})");
            Console.WriteLine(ex.Message);

#if DEBUG
            Console.WriteLine("\nStack Trace:");
            Console.WriteLine(ex.StackTrace);
#endif

            Console.ResetColor();

            if (attempts < maxAttempts)
            {
                Console.WriteLine($"\nПерезапуск игры через 3 секунды...");
                CountdownRestart();
            }
            else
            {
                Console.WriteLine("\nИгра завершена из-за критических ошибок.");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

        private void CountdownRestart()
        {
            for (int i = 3; i > 0; i--)
            {
                Console.WriteLine($"{i}...");
                Thread.Sleep(1000);
            }
        }
    }
}