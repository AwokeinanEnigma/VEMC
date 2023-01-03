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

            Debug.Log("Map passed first few checks.");

            if (map.Tilesets.Count > 1)
            {
                Debug.LogWarning("HEAR YE HEAR YE");
                Debug.LogWarning("THIS MAP HAS MORE THAN ONE TILESET");
                Debug.LogWarning("THIS IS NOT ALLOWED");
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
                        // Get the file name of the tileset image, without the extension
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tmxTileset.Image.Source);

                        // Get the first global ID of the tileset
                        int firstGid = tmxTileset.FirstGid;

                        // Optimize the tileset
                        OptimizedTileset optimizedTileset = TilesetOptimizer.Optimize(tmxTileset);

                        // Add the optimized tileset to the dictionary using its name as the key
                        tilesetDict.Add(tmxTileset.Name, optimizedTileset);

                        // Add a new compound tag to the main compound, with the file name and first global ID as sub-tags
                        compound.Add(new NbtCompound
{
    new NbtString("ts", fileNameWithoutExtension),
    new NbtInt("tid", firstGid)
});

                        // Get the transparency color of the tileset image
                        TmxColor trans = tmxTileset.Image.Trans;

                        // Convert the transparency color to a System.Drawing.Color object
                        Color transparencyColor = Color.FromArgb(trans.R, trans.G, trans.B);

                        // Create a .mtdat file for the tileset
                        NbtFile tileSetDat = TilesetDatBuilder.Build(tmxTileset, optimizedTileset);

                        // Save the .mtdat file to the specified directory
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
            
            MapPartBuilder.BuildMesh(mapCompound, objectsByType, map);
            MapPartBuilder.BuildTiles(mapCompound, objectsByType, map, optimizedTilesets);
            
            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.Log($"Elapsed seconds is {elapsedMs}");

            string fileName2 = string.Format("{0}\\Data\\Maps\\{1}.mdat", Utility.AppDirectory, map.Properties["name"]);
            nbtFile.SaveToFile(fileName2, NbtCompression.GZip);
            Utility.ConsoleWrite("Saved. Press any key to exit.");

            Console.ReadKey();
        }

        private Dictionary<string, List<TmxObjectGroup.TmxObject>> GetObjectsByType(TmxList<TmxObjectGroup> groupList)
        {
            // Create a dictionary to store the objects, grouped by type
            Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType = new Dictionary<string, List<TmxObjectGroup.TmxObject>>();

            // Iterate through the object groups
            foreach (TmxObjectGroup tmxObjectGroup in groupList)
            {
                // Iterate through the objects in the group
                foreach (TmxObjectGroup.TmxObject tmxObject in tmxObjectGroup.Objects)
                {
                    // Get the lowercase type of the object
                    string type = tmxObject.Type.ToLower();

                    // Get the list of objects for this type, or create a new one if it doesn't exist
                    List<TmxObjectGroup.TmxObject> objectList;
                    if (objectsByType.ContainsKey(type))
                    {
                        objectList = objectsByType[type];
                    }
                    else
                    {
                        objectList = new List<TmxObjectGroup.TmxObject>();
                        objectsByType.Add(type, objectList);
                    }

                    // Add the object to the list
                    objectList.Add(tmxObject);
                }
            }

            // Return the dictionary of objects by type
            return objectsByType;
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
