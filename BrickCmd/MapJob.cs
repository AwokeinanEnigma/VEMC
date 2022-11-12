using ClipperLib;
using fNbt;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
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
            Mode++;

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

            Mode++;
            Mode++;

            MapPart bgm = new MapPart("audbgm", true);
            objectsByType.TryGetValue("bgm", out List<TmxObjectGroup.TmxObject> bgmObjectList);

            int numberOfBgmParts = 0;

            if (bgmObjectList != null)
            {
                foreach (TmxObjectGroup.TmxObject bgmObj in bgmObjectList)
                {
                    MapPart bgmPart = new MapPart(false);
                    bgmPart.Add("x", bgmObj.X);
                    bgmPart.Add("y", bgmObj.Y);
                    bgmPart.Add("w", bgmObj.Width);
                    bgmPart.Add("h", bgmObj.Height);
                    bgmPart.AddFromDictionary<short>("flag", bgmObj.Properties, "playflag", 0);
                    bgmPart.AddFromDictionary<bool>("loop", bgmObj.Properties, "loop", true);
                    bgmPart.AddFromDictionary<string>("bgm", bgmObj.Properties, "bgm");
                    bgm.Add(bgmPart);
                    numberOfBgmParts++;
                }
            }

            if (bgm.Tags.Count > 0)
            {
                mapCompound.Add(bgm.Tag);
            }
            Mode++;


            MapPart audioSfx = new MapPart("audsfx", true);
            objectsByType.TryGetValue("sfx", out List<TmxObjectGroup.TmxObject> audioSfxObjects);
            int num2 = 0;

            if (audioSfxObjects != null)
            {
                foreach (TmxObjectGroup.TmxObject sfxObj in audioSfxObjects)
                {
                    MapPart sfxPart = new MapPart(false);
                    sfxPart.Add("x", sfxObj.X);
                    sfxPart.Add("y", sfxObj.Y);
                    sfxPart.Add("w", sfxObj.Width);
                    sfxPart.Add("h", sfxObj.Height);
                    sfxPart.AddFromDictionary<short>("flag", sfxObj.Properties, "playFlag", 0);
                    sfxPart.AddFromDictionary<short>("interval", sfxObj.Properties, "interval", 0);
                    sfxPart.AddFromDictionary<bool>("loop", sfxObj.Properties, "loop", true);
                    sfxPart.AddFromDictionary<string>("sfx", sfxObj.Properties, "sfx");
                    audioSfx.Add(sfxPart);
                    num2++;
                }
            }

            if (audioSfx.Tags.Count > 0)
            {
                mapCompound.Add(audioSfx.Tag);
            }
            Mode++;

            MapPart doors = new MapPart("doors", true);
            objectsByType.TryGetValue("door", out List<TmxObjectGroup.TmxObject> doorObjects);
            int num3 = 0;
            if (doorObjects != null)
            {
                foreach (TmxObjectGroup.TmxObject doorObjs in doorObjects)
                {
                    MapPart doorPart = new MapPart(false);
                    doorPart.Add("x", doorObjs.X);
                    doorPart.Add("y", doorObjs.Y);
                    doorPart.Add("w", doorObjs.Width);
                    doorPart.Add("h", doorObjs.Height);
                    doorPart.AddFromDictionary<int>("xto", doorObjs.Properties, "xto");
                    doorPart.AddFromDictionary<int>("yto", doorObjs.Properties, "yto");
                    doorPart.AddFromDictionary<string>("map", doorObjs.Properties, "map");
                    doorPart.AddFromDictionary<int>("sfx", doorObjs.Properties, "sfx", 0);
                    doorPart.AddFromDictionary<short>("flag", doorObjs.Properties, "flag", 0);
                    doors.Add(doorPart);
                    num3++;
                }
            }
            if (doors.Tags.Count > 0)
            {
                mapCompound.Add(doors.Tag);
            }
            Mode++;
            MapPart triggers = new MapPart("triggers", true);
            objectsByType.TryGetValue("trigger area", out List<TmxObjectGroup.TmxObject> triggerList);
            int num4 = 0;
            if (triggerList != null)
            {
                foreach (TmxObjectGroup.TmxObject tmxObject4 in triggerList)
                {
                    MapPart triggerPart = new MapPart(false);
                    NbtList coordsList = new NbtList("coords", NbtTagType.Int);
                    if (tmxObject4.ObjectType == TmxObjectGroup.TmxObjectType.Polygon)
                    {
                        using (List<Tuple<int, int>>.Enumerator enumerator3 = tmxObject4.Points.GetEnumerator())
                        {
                            while (enumerator3.MoveNext())
                            {
                                Tuple<int, int> tuple = enumerator3.Current;
                                coordsList.Add(new NbtInt(tuple.Item1));
                                coordsList.Add(new NbtInt(tuple.Item2));
                            }
                            goto IL_88B;
                        }
                        goto IL_7FF;
                    }
                    goto IL_7FF;
                IL_88B:
                    triggerPart.Add(coordsList);
                    triggerPart.AddFromDictionary<string>("scr", tmxObject4.Properties, "script");
                    triggerPart.AddFromDictionary<short>("flag", tmxObject4.Properties, "flag", 0);
                    triggerPart.Add("x", tmxObject4.X);
                    triggerPart.Add("y", tmxObject4.Y);
                    triggers.Add(triggerPart);
                    num4++;
                    continue;
                IL_7FF:
                    if (tmxObject4.ObjectType == TmxObjectGroup.TmxObjectType.Basic)
                    {
                        coordsList.Add(new NbtInt(0));
                        coordsList.Add(new NbtInt(0));
                        coordsList.Add(new NbtInt(tmxObject4.Width));
                        coordsList.Add(new NbtInt(0));
                        coordsList.Add(new NbtInt(tmxObject4.Width));
                        coordsList.Add(new NbtInt(tmxObject4.Height));
                        coordsList.Add(new NbtInt(0));
                        coordsList.Add(new NbtInt(tmxObject4.Height));
                        goto IL_88B;
                    }
                    goto IL_88B;
                }
            }
            if (triggers.Tags.Count > 0)
            {
                mapCompound.Add(triggers.Tag);
            }
            Mode++;
            MapPart npcPart = new MapPart("npcs", true);
            objectsByType.TryGetValue("npc", out List<TmxObjectGroup.TmxObject> npcList);
            if (npcList != null)
            {
                foreach (TmxObjectGroup.TmxObject npcObj in npcList)
                {
                    MapPart npc = new MapPart(false);
                    npcObj.Properties.TryGetValue("sprite", out string text2);
                    if (text2 != null && text2.Trim().Length > 0)
                    {
                        npc.Add("x", npcObj.X + npcObj.Width / 2);
                        npc.Add("y", npcObj.Y + npcObj.Height / 2);
                        npc.AddFromDictionary<string>("spr", npcObj.Properties, "sprite", string.Empty);
                    }
                    else
                    {
                        npc.Add("x", npcObj.X);
                        npc.Add("y", npcObj.Y);
                        npc.Add("w", npcObj.Width);
                        npc.Add("h", npcObj.Height);
                    }
                    npc.Add("name", npcObj.Name);
                    npc.AddFromDictionary<byte>("dir", npcObj.Properties, "direction", 6);
                    npc.AddFromDictionary<byte>("mov", npcObj.Properties, "movement", 0);
                    npc.AddFromDictionary<float>("spd", npcObj.Properties, "speed", 1f);
                    npc.AddFromDictionary<short>("dst", npcObj.Properties, "distance", 20);
                    npc.AddFromDictionary<short>("dly", npcObj.Properties, "delay", 0);
                    npc.AddFromDictionary<string>("cnstr", npcObj.Properties, "constraint", string.Empty);
                    npc.AddFromDictionary<bool>("cls", npcObj.Properties, "collisions", true);
                    npc.AddFromDictionary<bool>("en", npcObj.Properties, "enabled", true);
                    npc.AddFromDictionary<short>("flag", npcObj.Properties, "flag", 0);
                    npc.AddFromDictionary<bool>("shdw", npcObj.Properties, "shadow", true);
                    npc.AddFromDictionary<bool>("stky", npcObj.Properties, "sticky", false);
                    npc.AddFromDictionary<int>("dpth", npcObj.Properties, "depth", int.MinValue);
                 
                    List<NbtTag> textTags = new List<NbtTag>();
                    
                    int rufiniIndex = 0;
                   
                    string rufiniText;

                    while (npcObj.Properties.TryGetValue("text" + rufiniIndex, out rufiniText))
                    {
                        string[] array = rufiniText.Split(new char[]
                        {
                            ','
                        });
                        if (array.Length >= 2)
                        {
                            textTags.Add(new NbtString(string.Format("t{0}", rufiniIndex), array[0]));
                            textTags.Add(new NbtShort(string.Format("f{0}", rufiniIndex), short.Parse(array[1])));
                        }
                        rufiniIndex++;
                    }

                    NbtCompound txtEntries = new NbtCompound("entries");
                    txtEntries.AddRange(textTags);
                    npc.Add(txtEntries);

                    List<NbtTag> rufiniTextTags = new List<NbtTag>();
                    int secondRufiniIndex = 0;
                    while (npcObj.Properties.TryGetValue("tele" + secondRufiniIndex, out rufiniText))
                    {
                        string[] array2 = rufiniText.Split(new char[]
                        {
                            ','
                        });
                        if (array2.Length >= 2)
                        {
                            rufiniTextTags.Add(new NbtString(string.Format("t{0}", secondRufiniIndex), array2[0]));
                            rufiniTextTags.Add(new NbtShort(string.Format("f{0}", secondRufiniIndex), short.Parse(array2[1])));
                        }
                        secondRufiniIndex++;
                    }
                    NbtCompound telepathy = new NbtCompound("tele");
                    telepathy.AddRange(rufiniTextTags);
                    npc.Add(telepathy);
                    npcPart.Add(npc);
                }
            }
            if (npcPart.Tags.Count > 0)
            {
                mapCompound.Add(npcPart.Tag);
            }
            Mode++;

            MapPart pathsPart = new MapPart("paths", true);
            objectsByType.TryGetValue("npc path", out List<TmxObjectGroup.TmxObject> pathList);
            int num8 = 0;
            if (pathList != null)
            {
                foreach (TmxObjectGroup.TmxObject pathObject in pathList)
                {
                    MapPart pathPart = new MapPart(false);
                    pathPart.Add("name", pathObject.Name);

                    List<NbtInt> coordsList = new List<NbtInt>();
                    if (pathObject.Points != null)
                    {
                        using (List<Tuple<int, int>>.Enumerator enumerator3 = pathObject.Points.GetEnumerator())
                        {
                            while (enumerator3.MoveNext())
                            {
                                Tuple<int, int> tuple2 = enumerator3.Current;
                                coordsList.Add(new NbtInt(pathObject.X + tuple2.Item1));
                                coordsList.Add(new NbtInt(pathObject.Y + tuple2.Item2));
                            }
                            goto IL_E84;
                        }
                        goto IL_E6D;
                    IL_E84:
                        NbtList coordsNbtList = new NbtList("coords", coordsList, NbtTagType.Int);
                        pathPart.Add(coordsNbtList);
                        pathsPart.Add(pathPart);
                        num8++;
                        continue;
                    }
                IL_E6D:
                    throw new Exception(string.Format("NPC Path does not contain points. Make sure you set \"{0}\" to the right type.", pathObject.Name));
                }
            }
            if (pathsPart.Tags.Count > 0)
            {
                mapCompound.Add(pathsPart.Tag);
            }
            Mode++;

            MapPart areasPart = new MapPart("areas", true);
            objectsByType.TryGetValue("npc area", out List<TmxObjectGroup.TmxObject> npcAreaList);
            int num10 = 0;
            if (npcAreaList != null)
            {
                foreach (TmxObjectGroup.TmxObject tmxObject7 in npcAreaList)
                {
                    MapPart npcArea = new MapPart(false);
                    npcArea.Add("name", tmxObject7.Name);
                    npcArea.Add("x", tmxObject7.X);
                    npcArea.Add("y", tmxObject7.Y);
                    npcArea.Add("w", tmxObject7.Width);
                    npcArea.Add("h", tmxObject7.Height);
                    areasPart.Add(npcArea);
                    num10++;
                }
            }
            if (areasPart.Tags.Count > 0)
            {
                mapCompound.Add(areasPart.Tag);
            }
            Mode++;

            MapPart crowdsPart = new MapPart("crowds", true);
            objectsByType.TryGetValue("crowd path", out List<TmxObjectGroup.TmxObject> crowdsPath);
            if (crowdsPath != null)
            {
                foreach (TmxObjectGroup.TmxObject crowdObject in crowdsPath)
                {
                    MapPart crowdPath = new MapPart(false);
                    crowdsPart.Add(crowdPath);
                    crowdPath.AddFromDictionary<int>("mode", crowdObject.Properties, "mode");

                    List<NbtShort> spritesInArea = new List<NbtShort>();
                    crowdObject.Properties.TryGetValue("sprites", out string spriteString);
                    if (spriteString != null)
                    {
                        string[] array3 = spriteString.Split(new char[]
                        {
                            ','
                        });
                        foreach (string s in array3)
                        {
                            spritesInArea.Add(new NbtShort(short.Parse(s)));
                        }

                        NbtList sprites = new NbtList("sprs", spritesInArea, NbtTagType.Short);
                        crowdPath.Add(sprites);

                        List<NbtInt> npcNbtInt = new List<NbtInt>();
                        int num13 = 0;
                        if (crowdObject.Points != null)
                        {
                            using (List<Tuple<int, int>>.Enumerator tuplesEnumerator = crowdObject.Points.GetEnumerator())
                            {
                                while (tuplesEnumerator.MoveNext())
                                {
                                    Tuple<int, int> currentTuple = tuplesEnumerator.Current;
                                    npcNbtInt.Add(new NbtInt(currentTuple.Item1));
                                    npcNbtInt.Add(new NbtInt(currentTuple.Item2));
                                    num13++;
                                }
                                goto IL_1191;
                            }
                            goto IL_117A;
                        IL_1191:
                            NbtList coordsNbtList = new NbtList("coords", npcNbtInt, NbtTagType.Int);
                            crowdPath.Add(coordsNbtList);
                            continue;
                        }
                    IL_117A:
                        throw new Exception(string.Format("Crowd Path does not contain points. Make sure you set \"{0}\" to the right type.", crowdObject.Name));
                    }
                    throw new MapPartRequirementException(crowdObject.Name, "sprites");
                }
            }
            if (crowdsPart.Tags.Count > 0)
            {
                mapCompound.Add(crowdsPart.Tag);
            }
            Mode++;
            MapPart spawnsPart = new MapPart("spawns", true);
            objectsByType.TryGetValue("enemy spawn", out List<TmxObjectGroup.TmxObject> enemySpawnObjects);

            if (enemySpawnObjects != null)
            {
                foreach (TmxObjectGroup.TmxObject enemySpawn in enemySpawnObjects)
                {

                    NbtCompound spawnCompound = new NbtCompound();
                    spawnsPart.Add(spawnCompound);
                    Console.WriteLine($"3");
                    spawnCompound.Add(new NbtInt("x", enemySpawn.X));
                    spawnCompound.Add(new NbtInt("y", enemySpawn.Y));
                    spawnCompound.Add(new NbtInt("w", enemySpawn.Width));
                    spawnCompound.Add(new NbtInt("h", enemySpawn.Height));

                    //...?
                    enemySpawn.Properties.TryGetValue("flag", out string s2);
                    short value = short.Parse(s2);
                    spawnCompound.Add(new NbtShort("flag", value));

                    List<NbtString> enemyId = new List<NbtString>();
                    List<NbtByte> enemyByte = new List<NbtByte>();

                    IEnumerable<KeyValuePair<string, string>> enemyPairs = enemySpawn.Properties.Where(x => x.Key.Contains("enemy"));
                    foreach (KeyValuePair<string, string> strung in enemyPairs)
                    {
                        Console.WriteLine($"strung, {strung.Key}, {strung.Value}");
                        string[] enemyDataSplit = strung.Value.Split(new char[]
                        {
                            ','
                        });

                        Console.WriteLine($"Enemy: {enemyDataSplit[0]}");
                        enemyId.Add(new NbtString(enemyDataSplit[0]));
                        enemyByte.Add(new NbtByte(byte.Parse(enemyDataSplit[1])));

                    }
                    NbtList enemyIdList = new NbtList("enids", enemyId, NbtTagType.String);
                    NbtList enemyFrequencies = new NbtList("enfreqs", enemyByte, NbtTagType.Byte);
                    spawnCompound.Add(enemyIdList);
                    spawnCompound.Add(enemyFrequencies);
                }
            }
            if (spawnsPart.Tags.Count > 0)
            {
                mapCompound.Add(spawnsPart.Tag);
            }
            Mode++;
            MapPart parallaxPart = new MapPart("parallax", true);
            objectsByType.TryGetValue("parallax", out List<TmxObjectGroup.TmxObject> parallaxList);
            if (parallaxList != null)
            {
                foreach (TmxObjectGroup.TmxObject currentParallax in parallaxList)
                {
                    MapPart parallax = new MapPart(false);
                    parallaxPart.Add(parallax);

                    parallax.AddFromDictionary<string>("spr", currentParallax.Properties, "sprite");
                    parallax.AddFromDictionary<float>("vx", currentParallax.Properties, "vx", 1f);
                    parallax.AddFromDictionary<float>("vy", currentParallax.Properties, "vy", 1f);
                    parallax.Add(new NbtFloat("x", currentParallax.X));
                    parallax.Add(new NbtFloat("y", currentParallax.Y));
                    parallax.Add(new NbtFloat("w", currentParallax.Width));
                    parallax.Add(new NbtFloat("h", currentParallax.Height));
                }
            }
            if (parallaxPart.Tags.Count > 0)
            {
                mapCompound.Add(parallaxPart.Tag);
            }
            Mode++;

            NbtList meshPart = new NbtList("mesh", NbtTagType.List);
            List<List<IntPoint>> mesh = BuildMesh();
            foreach (List<IntPoint> pointList in mesh)
            {
                List<NbtInt> point = new List<NbtInt>();
                foreach (IntPoint intPoint in pointList)
                {
                    point.Add(new NbtInt((int)intPoint.X));
                    point.Add(new NbtInt((int)intPoint.Y));
                }

                NbtList listofPoints = new NbtList(point, NbtTagType.Int);
                meshPart.Add(listofPoints);
            }
            if (meshPart.Count > 0)
            {
                mapCompound.Add(meshPart);
            }
            Mode++;

            NbtList genericTilesList = new NbtList("tiles", NbtTagType.Compound);
            TileGrouper tileGrouper = new TileGrouper(map);
            List<TileGroup> tileGroups = tileGrouper.FindGroups();
            foreach (TileGroup tileGroup in tileGroups)
            {
                MapPart tileGroupPart = new MapPart(false);
                tileGroupPart.Add(new NbtInt("depth", tileGroup.depth));
                tileGroupPart.Add(new NbtInt("x", tileGroup.x));
                tileGroupPart.Add(new NbtInt("y", tileGroup.y));
                tileGroupPart.Add(new NbtInt("ox", tileGroup.originX));
                tileGroupPart.Add(new NbtInt("w", tileGroup.width / map.TileWidth));
                
                int tileWidth = tileGroup.width / map.TileWidth;
                int tileHeight = tileGroup.height / map.TileHeight;

                int groupTileX = tileGroup.x / map.TileWidth;
                int groupTileY = tileGroup.y / map.TileHeight;

                uint[] tileSize = new uint[tileWidth * tileHeight];
                bool invalidTile = true;

                OptimizedTileset optimizedTileset2 = optimizedTilesets[map.Tilesets[0].Name];
                int y;

                for (y = 0; y < tileHeight; y++)
                {
                    int x;
                    for (x = 0; x < tileWidth; x++)
                    {
                        TileGrouper.TileData tileData = tileGroup.tiles.Find((TileGrouper.TileData t) => t.tile.X == x + groupTileX && t.tile.Y == y + groupTileY);
                        int heightWidthX = y * tileWidth + x;
                        if (tileData.tile != null)
                        {

                            if (tileData.tile.Gid > optimizedTileset2.TranslationTable.Count)
                            {
                                Console.WriteLine($"GID '{tileData.tile.Gid}' at tile '{tileData.tile.X},{tileData.tile.Y}' when the maximum is {optimizedTileset2.TranslationTable.Count}");
                                continue;
                            }


                            uint translatedGID = (uint)(optimizedTileset2.Translate(tileData.tile.Gid));
                            uint val = (tileData.modifier << 16 | translatedGID);
                            
                            tileSize[heightWidthX] = val;
                        }
                        else
                        {
                            tileSize[heightWidthX] = 0U;
                        }
                        invalidTile = (invalidTile && tileSize[heightWidthX] == 0U);
                    }
                }
                if (!invalidTile)

                {
                    
                    byte[] tileBytes = new byte[tileSize.Length * 4];
                    Buffer.BlockCopy(tileSize, 0, tileBytes, 0, tileBytes.Length);
                    tileGroupPart.Add("tiles", tileBytes);

                    genericTilesList.Add(tileGroupPart.Tag);
                }
            }
            if (genericTilesList.Count > 0)
            {
                mapCompound.Add(genericTilesList);
            }
            Mode++;
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
