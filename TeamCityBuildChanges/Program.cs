﻿using System;
using System.Collections.Generic;
using ManyConsole;

namespace TeamCityBuildChanges
{
    class Program
    {
        static int Main(string[] args)
        {
            var commands = GetCommands();
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
        }

        static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }

        
    }
}
