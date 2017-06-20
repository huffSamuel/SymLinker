using SymLinker.Linker;
using System;
using System.IO;

namespace SymLinker
{
    class Program
    {
        static ConsoleColor bgc;
        static ConsoleColor fgc;
        static void Main(string[] args)
        {
            // Clear and setup for test
            Directory.Delete("C:\\Test\\Sub", true);
            Directory.CreateDirectory("C:\\Test\\Sub");
            File.WriteAllText("C:\\Test\\testdoc.txt", "This is a test file", System.Text.Encoding.ASCII);

            // Create and setup the symlinker
            var linker = new SymLinker.Linker.Linker();
            linker.OnError += WriteError;
            linker.OnWarn += WriteWarn;
            linker.OnInfo += WriteInfo;

            // Create a link!
            linker.CreateLink(@"C:\Test\testdoc.txt", @"C:\Test\Sub");
        }

        static void WriteError(string msg)
        {
            SaveConsoleColors();
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(msg);
            RestoreConsoleColors();
        }

        static void WriteInfo(string msg)
        {
            SaveConsoleColors();
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(msg);
            RestoreConsoleColors();
        }

        static void WriteWarn(string msg)
        {
            SaveConsoleColors();
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(msg);
            RestoreConsoleColors();
        }

        static void SaveConsoleColors()
        {
            fgc = Console.ForegroundColor;
            bgc = Console.BackgroundColor;
        }

        static void RestoreConsoleColors()
        {
            Console.ForegroundColor = fgc;
            Console.BackgroundColor = bgc;
        }
    }
}