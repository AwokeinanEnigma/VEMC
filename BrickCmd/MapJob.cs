using ClipperLib;
using fNbt;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TiledSharp;
using VEMC.Parts;

namespace VEMC
{
    public class MapJob
    {
        public int Mode { get; private set; }
        public MapJob()
        {
            Mode = 0;
        }
        public void Open(string filename)
        {
            map = new TmxMap(filename);
        }
        public void CheckProperty(string property)
        {
            if (!map.Properties.ContainsKey(property))
            {
                throw new MapPropertyException(property);
            }
        }
        public void ValidateMap()
        {
            CheckProperty("name");
            CheckProperty("title");
            CheckProperty("subtitle");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Map passed first few checks.");
            Console.ResetColor();

            if (map.Tilesets.Count > 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("HEAR YE HEAR YE");
                Console.WriteLine("THIS MAP HAS MORE THAN ONE TILESET");
                Console.WriteLine("THIS IS NOT ALLOWED");
                throw new Exception("Map had more than two tilesets.");

            }

        }
        public void Process()
        {
            ValidateMap();
            void AddToHeader(MapPart header)
            {
                header.AddFromDictionary<string>("title", map.Properties, "title");
                header.AddFromDictionary<string>("subtitle", map.Properties, "subtitle");
                header.AddFromDictionary<string>("script", map.Properties, "script", string.Empty);
                header.AddFromDictionary<string>("bbg", map.Properties, "bbg", string.Empty);
                header.AddFromDictionary<bool>("shdw", map.Properties, "shadows", true);
                header.AddFromDictionary<bool>("ocn", map.Properties, "ocean", false);
                header.Add("color", (int)Utility.TmxColorToInt(map.BackgroundColor));
            }

            void CreateTilesetDatFiles(Dictionary<string, OptimizedTileset> tilesetDict, List<NbtCompound> compound)
            {
                foreach (TmxTileset tmxTileset in map.Tilesets)
                {
                    if (tmxTileset.Image != null)
                    {
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tmxTileset.Image.Source);
                        int firstGid = tmxTileset.FirstGid;
                        OptimizedTileset optimizedTileset = TilesetOptimizer.Optimize(tmxTileset);
                        tilesetDict.Add(tmxTileset.Name, optimizedTileset);
                        compound.Add(new NbtCompound
                        {
                            new NbtString("ts", fileNameWithoutExtension),
                            new NbtInt("tid", firstGid)
                        });
                        TmxColor trans = tmxTileset.Image.Trans;
                        Color.FromArgb(trans.R, trans.G, trans.B);
                        NbtFile tileSetDat = TilesetDatBuilder.Build(tmxTileset, optimizedTileset);
                        string fileName = Utility.AppDirectory + "\\Data\\Graphics\\MapTilesets\\" + fileNameWithoutExtension + ".mtdat";
                        tileSetDat.SaveToFile(fileName, NbtCompression.GZip);
                    }
                }
            }

            Utility.Hash(map.Properties["name"]);

            Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType = GetObjectsByType(map.ObjectGroups);
            NbtFile nbtFile = new NbtFile();

            NbtCompound mapCompound = new NbtCompound("map");
            nbtFile.RootTag = mapCompound;
            

            MapPart mapHeader = new MapPart("head");
            AddToHeader(mapHeader);

            if (map.Properties.TryGetValue("nightColor", out string text))
            {
                mapHeader.Add("nColor", int.Parse("FF" + text.Substring(0, Math.Min(6, text.Length)), NumberStyles.HexNumber));
            }
            mapHeader.Add("width", map.Width * map.TileWidth);
            mapHeader.Add("height", map.Height * map.TileHeight);

            List<NbtCompound> optimizedTilesetCompounds = new List<NbtCompound>();
            Dictionary<string, OptimizedTileset> optimizedTilesets = new Dictionary<string, OptimizedTileset>();

            CreateTilesetDatFiles(optimizedTilesets, optimizedTilesetCompounds);

            NbtList tag = new NbtList("tilesets", optimizedTilesetCompounds, NbtTagType.Compound);
            mapHeader.Add(tag);
            mapCompound.Add(mapHeader.Tag);


            var watch = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here


            // Create a list of tasks to execute the build methods in parallel
            List<Task> tasks = new List<Task>();

            tasks.Add(Task.Run(() => MapPartBuilder.BuildBackgroundAudio(mapCompound, objectsByType)));
            tasks.Add(Task.Run(() => MapPartBuilder.BuildSfx(mapCompound, objectsByType)));
            tasks.Add(Task.Run(() => MapPartBuilder.BuildDoor(mapCompound, objectsByType)));
            tasks.Add(Task.Run(() => MapPartBuilder.BuildTriggers(mapCompound, objectsByType)));
            tasks.Add(Task.Run(() => MapPartBuilder.BuildNPCS(mapCompound, objectsByType)));
            tasks.Add(Task.Run(() => MapPartBuilder.BuildNPCPaths(mapCompound, objectsByType)));
            tasks.Add(Task.Run(() => MapPartBuilder.BuildNPCAreas(mapCompound, objectsByType)));
            tasks.Add(Task.Run(() => MapPartBuilder.BuildCrowds(mapCompound, objectsByType)));
            tasks.Add(Task.Run(() => MapPartBuilder.BuildEnemySpawns(mapCompound, objectsByType)));
            tasks.Add(Task.Run(() => MapPartBuilder.BuildParallax(mapCompound, objectsByType)));
            tasks.Add(Task.Run(() => MapPartBuilder.BuildMesh(mapCompound, objectsByType, map)));
            tasks.Add(Task.Run(() => MapPartBuilder.BuildTiles(mapCompound, objectsByType, map, optimizedTilesets)));

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.Log($"Elapsed seconds is {elapsedMs}");

            Utility.ConsoleWrite("Saving...                             ", new object[0]);
            string fileName2 = string.Format("{0}\\Data\\Maps\\{1}.mdat", Utility.AppDirectory, map.Properties["name"]);
            nbtFile.SaveToFile(fileName2, NbtCompression.GZip);
        }
        public List<List<IntPoint>> BuildMesh()
        {
            int height = map.Height;
            int width = map.Width;
            TmxTileset tileset = map.Tilesets[0];
            int num = width * height * map.Layers.Count;
            int num2 = 0;
            MeshBuilder meshBuilder = new MeshBuilder();
            string text = null;
            for (int i = map.Layers.Count - 1; i >= 0; i--)
            {
                string name = map.Layers[i].Name;
                int count = map.Layers[i].Tiles.Count;
                for (int j = 0; j < count; j++)
                {
                    TmxLayerTile tmxLayerTile = map.Layers[i].Tiles[j];
                    int gid = tmxLayerTile.Gid - 1;
                    int x = tmxLayerTile.X;
                    int y = tmxLayerTile.Y;
                    TmxTilesetTile tileById = tileset.GetTileById(gid);
                    if (tileById != null)
                    {
                        int num3 = 0;
                        tileById.Properties.TryGetValue("solidity", out text);
                        if (text != null)
                        {
                            num3 = int.Parse(text);
                        }
                        if (num3 > 0)
                        {
                            meshBuilder.AddPath(x * map.TileWidth, y * map.TileHeight, collisionMasks[num3]);
                        }
                    }
                    num2++;
                    if (num2 % 100 == 0)
                    {
                        Utility.ConsoleWrite("Building collision mesh from \"{0}\" ({1}%)", new object[]
                        {
                            name,
                            Math.Round(num2 / (double)num * 100.0)
                        });
                    }
                }
            }
            Utility.ConsoleWrite("Simplifying collision mesh...                                     ", new object[0]);
            meshBuilder.Simplify();
            return meshBuilder.Solution;
        }
        private uint CheckGraphicsManifest(string filename)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            string path = Utility.AppDirectory + "\\Resources\\Graphics\\manifest.txt";
            FileStream fileStream = new FileStream(path, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            int num = -1;
            int num2 = 0;
            while (!streamReader.EndOfStream)
            {
                string a = streamReader.ReadLine();
                if (a == fileNameWithoutExtension)
                {
                    num = num2;
                    break;
                }
                num2++;
            }
            fileStream.Close();
            if (num == -1)
            {
                fileStream = new FileStream(path, FileMode.Append);
                StreamWriter streamWriter = new StreamWriter(fileStream);
                streamWriter.WriteLine(fileNameWithoutExtension);
                streamWriter.Close();
                num = File.ReadLines(path).Count<string>() - 1;
                Console.WriteLine(filename + " was added to the graphics manifest.");
            }
            return (uint)num;
        }
        private Dictionary<string, List<TmxObjectGroup.TmxObject>> GetObjectsByType(TmxList<TmxObjectGroup> groupList)
        {
            Dictionary<string, List<TmxObjectGroup.TmxObject>> dictionary = new Dictionary<string, List<TmxObjectGroup.TmxObject>>();
            foreach (TmxObjectGroup tmxObjectGroup in groupList)
            {
                foreach (TmxObjectGroup.TmxObject tmxObject in tmxObjectGroup.Objects)
                {
                    string key = tmxObject.Type.ToLower();
                    List<TmxObjectGroup.TmxObject> list;
                    if (dictionary.ContainsKey(key))
                    {
                        list = dictionary[key];
                    }
                    else
                    {
                        list = new List<TmxObjectGroup.TmxObject>();
                        dictionary.Add(key, list);
                    }
                    list.Add(tmxObject);
                }
            }
            return dictionary;
        }

        private TmxMap map;
        private readonly List<Tuple<string, byte>> effectDict = new List<Tuple<string, byte>>
        {
            new Tuple<string, byte>("none", 0),
            new Tuple<string, byte>("rain", 1),
            new Tuple<string, byte>("storm", 2),
            new Tuple<string, byte>("snow", 3),
            new Tuple<string, byte>("underwater", 4),
            new Tuple<string, byte>("lighting", 5)
        };
        private readonly Dictionary<int, Point[]> collisionMasks = new Dictionary<int, Point[]>
        {
            {
                1,
                new Point[]
                {
                    new Point(0, 0),
                    new Point(8, 0),
                    new Point(8, 8),
                    new Point(0, 8)
                }
            },
            {
                2,
                new Point[]
                {
                    new Point(0, 0),
                    new Point(8, 0),
                    new Point(8, 8)
                }
            },
            {
                3,
                new Point[]
                {
                    new Point(8, 0),
                    new Point(8, 8),
                    new Point(0, 8)
                }
            },
            {
                4,
                new Point[]
                {
                    new Point(0, 0),
                    new Point(8, 8),
                    new Point(0, 8)
                }
            },
            {
                5,
                new Point[]
                {
                    new Point(0, 0),
                    new Point(8, 0),
                    new Point(0, 8)
                }
            },
            {
                6,
                new Point[]
                {
                    new Point(0, 0),
                    new Point(8, 0),
                    new Point(7, 8),
                    new Point(0, 8)
                }
            },
            {
                7,
                new Point[]
                {
                    new Point(0, 0),
                    new Point(7, 0),
                    new Point(6, 8),
                    new Point(0, 8)
                }
            },
            {
                8,
                new Point[]
                {
                    new Point(0, 0),
                    new Point(6, 0),
                    new Point(5, 8),
                    new Point(0, 8)
                }
            },
            {
                9,
                new Point[]
                {
                    new Point(0, 0),
                    new Point(4, 0),
                    new Point(3, 8),
                    new Point(0, 8)
                }
            },
            {
                10,
                new Point[]
                {
                    new Point(0, 0),
                    new Point(3, 0),
                    new Point(2, 8),
                    new Point(0, 8)
                }
            },
            {
                11,
                new Point[]
                {
                    new Point(0, 0),
                    new Point(2, 0),
                    new Point(1, 8),
                    new Point(0, 8)
                }
            },
            {
                12,
                new Point[]
                {
                    new Point(0, 0),
                    new Point(8, 0),
                    new Point(8, 8),
                    new Point(1, 8)
                }
            },
            {
                13,
                new Point[]
                {
                    new Point(1, 0),
                    new Point(8, 0),
                    new Point(8, 8),
                    new Point(2, 8)
                }
            },
            {
                14,
                new Point[]
                {
                    new Point(2, 0),
                    new Point(8, 0),
                    new Point(8, 8),
                    new Point(3, 8)
                }
            },
            {
                15,
                new Point[]
                {
                    new Point(4, 0),
                    new Point(8, 0),
                    new Point(8, 8),
                    new Point(5, 8)
                }
            },
            {
                16,
                new Point[]
                {
                    new Point(5, 0),
                    new Point(8, 0),
                    new Point(8, 8),
                    new Point(6, 8)
                }
            },
            {
                17,
                new Point[]
                {
                    new Point(6, 0),
                    new Point(8, 0),
                    new Point(8, 8),
                    new Point(7, 8)
                }
            }
        };
    }
}
