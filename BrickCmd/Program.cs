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
            Debug.Initialize();

            IniFile file = new IniFile();
            file.Load(Utility.AppDirectory + $"\\config.ini");
            string redirectPath = file["basicData"]["makeAtPath"].ToString();



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
            for (int i = 0; i < files.Length; i++) {
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
                catch (Exception e)
                {
                    Debug.LogError($"Error occurred while processing map." + Environment.NewLine +
                        $"Part Builder Mode: {PartBuilders.MapPartBuilder.mode}" + Environment.NewLine +
                        $"{e}", false);
                    // Skip the current map
                    continue;
                }
            }
            Debug.Log($"Press any key to exit");
            Console.ReadKey();

        }
    }
}
