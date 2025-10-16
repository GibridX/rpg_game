using System;
using System.Collections.Generic;
using System.Linq;
using TextRPG.Config;
using TextRPG.Core.Enums;
using TextRPG.Core.Services.Application;
using TextRPG.Core.Utils;

namespace TextRPG
{
    class Program
    {
        static void Main()
        {
            var appRunner = new ApplicationRunner();
            appRunner.Run();
        }
    }
}