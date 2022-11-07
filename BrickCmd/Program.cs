using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VEMC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Console.WriteLine("VEMC Version {0}.{1}.{2} CMD - ", version.Major, version.Minor, version.Build);
            Console.WriteLine("Using NUGET since the dawn of time");
            Console.ResetColor();
            Console.WriteLine("Builds Violet Engine map files from Tiled TMX files");
            Console.WriteLine();
            if (!Directory.Exists(Utility.AppDirectory + "\\Data"))
            {
                Directory.CreateDirectory(Utility.AppDirectory + "\\Data\\Maps");
                Directory.CreateDirectory(Utility.AppDirectory + "\\Data\\Graphics\\MapTilesets");
            }
            else
            {
                if (!Directory.Exists(Utility.AppDirectory + "\\Data\\Maps"))
                {
                    Directory.CreateDirectory(Utility.AppDirectory + "\\Data\\Maps");
                }
                if (!Directory.Exists(Utility.AppDirectory + "\\Data\\Graphics\\MapTilesets"))
                {
                    Directory.CreateDirectory(Utility.AppDirectory + "\\Data\\Graphics\\MapTilesets");
                }
            }
            if (args.Length == 0)
            {
                Console.WriteLine("For easier use, drag and drop TMX files onto the exe!");
                string text;
                do
                {
                    Console.Write("Please specify a TMX file: ");
                    text = Console.ReadLine().Replace("\"", "");
                }
                while (!File.Exists(text));
                Program.Process(new string[]
                {
                    text
                });
                return;
            }
            Program.Process(args);
        }
        private static void Process(string[] files)
        {
            float num = files.Length;
            float num2 = 0f;
            int i = 0;
            while (i < files.Length)
            {
                string text = files[i];
                string fileName = Path.GetFileName(text);

                MapJob mapJob = new MapJob();
                try
                {



                    mapJob.Open(text);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Processing \"" + fileName + "\"");
                    Console.ResetColor();
                    mapJob.Process();
                }

                catch (KeyNotFoundException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error while processing " + Program.DetermineMode(mapJob.Mode) + ":");
                    Console.ResetColor();
                    Console.WriteLine(
                        "Couldn't find a property. (Not sure which one.)\nMake sure you've specified the required properties.");
                    Console.WriteLine(ex);
                    Console.WriteLine("Press any key...");
                    Console.ReadKey();
                    goto IL_20A;
                }
                catch (MapBuildException ex2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error while processing " + Program.DetermineMode(mapJob.Mode) + ":");
                    Console.ResetColor();
                    Console.WriteLine(ex2.Message);
                    Console.WriteLine("Press any key...");
                    Console.ReadKey();
                }
                catch (MapPropertyException ex3)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error while processing " + Program.DetermineMode(mapJob.Mode) + ":");
                    Console.ResetColor();
                    Console.WriteLine(ex3.Message);
                    Console.WriteLine("Press any key...");
                    Console.ReadKey();
                }
                catch (Exception ex4)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("An unhandled exception occurred while processing " +
                                      Program.DetermineMode(mapJob.Mode) + ":");
                    Console.ResetColor();
                    Console.WriteLine(ex4.ToString());
                    Console.WriteLine("Press any key...");
                    Console.ReadKey();
                    goto IL_20A;
                }

                goto IL_1A7;
            IL_20A:
                i++;
                continue;
            IL_1A7:
                num2 += 1f;
                float num3 = num2 / num;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Concat(new object[]
                {
                    "Finished processing ",
                    fileName,
                    " [",
                    Math.Ceiling(num3 * 100f),
                    "%]"
                }));
                Console.ResetColor();
                goto IL_20A;


            }
        }
        private static string DetermineMode(int mode)
        {
            switch (mode)
            {
                case 0:
                    return "initialization";
                case 1:
                    return "header";
                case 2:
                    return "effects";
                case 3:
                    return "bgm";
                case 4:
                    return "sfx";
                case 5:
                    return "doors";
                case 6:
                    return "triggers";
                case 7:
                    return "npcs";
                case 8:
                    return "npc paths";
                case 9:
                    return "npc areas";
                case 10:
                    return "crowds";
                case 11:
                    return "enemies";
                case 12:
                    return "parallax backgrounds";
                case 13:
                    return "collision mesh";
                case 14:
                    return "tile data";
                case 15:
                    return "saving";
                default:
                    return "something??";
            }
        }
    }
}
