using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VEMC
{
    internal class Program
    {
        public static bool CopyTo;
        public static string CopyToPath;
        private static void Main(string[] args)
        {
            Debug.Initialize();

            IniFile file = new IniFile();
            file.Load(Utility.AppDirectory + $"\\config.ini");
            string redirectPath = file["basicData"]["makeAtPath"].ToString();

            // making note of this
            // if CopyTo is false, then we have no place to copy files to
            // if CopyTo is true, then we have a place to copy files to
            CopyTo = !redirectPath.Contains("<empty>");
            Debug.Log(CopyTo ? $"Copying map files to {redirectPath}" : "The path to copy map files to is empty. You should try setting it!" );
          
            CopyToPath = redirectPath;


            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Version version = Assembly.GetEntryAssembly().GetName().Version;

            Console.WriteLine("VEMC Version {0}.{1}.{2} CMD - ", version.Major, version.Minor, version.Build);
            Console.WriteLine("Using NUGET since the dawn of time");
            Console.ResetColor();
            Console.WriteLine("Builds Violet Engine map files from Tiled TMX files");
            Console.WriteLine();

            // if we have no path to copy to
            if (!CopyTo)
            {
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
            }
            // if we have a path to copy to
            else
            {
                if (!Directory.Exists(CopyToPath + "\\Data"))
                {
                    Directory.CreateDirectory(CopyToPath + "\\Data\\Maps");
                    Directory.CreateDirectory(CopyToPath + "\\Data\\Graphics\\MapTilesets");
                }
                else
                {
                    if (!Directory.Exists(CopyToPath + "\\Data\\Maps"))
                    {
                        Directory.CreateDirectory(CopyToPath + "\\Data\\Maps");
                    }
                    if (!Directory.Exists(CopyToPath + "\\Data\\Graphics\\MapTilesets"))
                    {
                        Directory.CreateDirectory(CopyToPath + "\\Data\\Graphics\\MapTilesets");
                    }
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
            for (int i = 0; i < files.Length; i++)
            {
                string text = files[i];
                string fileName = Path.GetFileName(text);

                MapJob mapJob = new MapJob();
                try
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    
                    Console.WriteLine("Processing \"" + fileName + "\"");
                   
                    Console.ResetColor();
                    
                    mapJob.OpenAndProcess(text);
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
