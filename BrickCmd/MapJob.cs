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
            objectsByType.TryGetValue("trigger area", out List<TmxObjectGroup.TmxObject> list5);
            int num4 = 0;
            if (list5 != null)
            {
                foreach (TmxObjectGroup.TmxObject tmxObject4 in list5)
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
            MapPart mapPart10 = new MapPart("npcs", true);
            objectsByType.TryGetValue("npc", out List<TmxObjectGroup.TmxObject> list6);
            int num5 = 0;
            if (list6 != null)
            {
                foreach (TmxObjectGroup.TmxObject tmxObject5 in list6)
                {
                    MapPart mapPart11 = new MapPart(false);
                    tmxObject5.Properties.TryGetValue("sprite", out string text2);
                    if (text2 != null && text2.Trim().Length > 0)
                    {
                        mapPart11.Add("x", tmxObject5.X + tmxObject5.Width / 2);
                        mapPart11.Add("y", tmxObject5.Y + tmxObject5.Height / 2);
                        mapPart11.AddFromDictionary<string>("spr", tmxObject5.Properties, "sprite", string.Empty);
                    }
                    else
                    {
                        mapPart11.Add("x", tmxObject5.X);
                        mapPart11.Add("y", tmxObject5.Y);
                        mapPart11.Add("w", tmxObject5.Width);
                        mapPart11.Add("h", tmxObject5.Height);
                    }
                    mapPart11.Add("name", tmxObject5.Name);
                    mapPart11.AddFromDictionary<byte>("dir", tmxObject5.Properties, "direction", 6);
                    mapPart11.AddFromDictionary<byte>("mov", tmxObject5.Properties, "movement", 0);
                    mapPart11.AddFromDictionary<float>("spd", tmxObject5.Properties, "speed", 1f);
                    mapPart11.AddFromDictionary<short>("dst", tmxObject5.Properties, "distance", 20);
                    mapPart11.AddFromDictionary<short>("dly", tmxObject5.Properties, "delay", 0);
                    mapPart11.AddFromDictionary<string>("cnstr", tmxObject5.Properties, "constraint", string.Empty);
                    mapPart11.AddFromDictionary<bool>("cls", tmxObject5.Properties, "collisions", true);
                    mapPart11.AddFromDictionary<bool>("en", tmxObject5.Properties, "enabled", true);
                    mapPart11.AddFromDictionary<short>("flag", tmxObject5.Properties, "flag", 0);
                    mapPart11.AddFromDictionary<bool>("shdw", tmxObject5.Properties, "shadow", true);
                    mapPart11.AddFromDictionary<bool>("stky", tmxObject5.Properties, "sticky", false);
                    mapPart11.AddFromDictionary<int>("dpth", tmxObject5.Properties, "depth", int.MinValue);
                    List<NbtTag> list7 = new List<NbtTag>();
                    int num6 = 0;
                    string text3;
                    while (tmxObject5.Properties.TryGetValue("text" + num6, out text3))
                    {
                        string[] array = text3.Split(new char[]
                        {
                            ','
                        });
                        if (array.Length >= 2)
                        {
                            list7.Add(new NbtString(string.Format("t{0}", num6), array[0]));
                            list7.Add(new NbtShort(string.Format("f{0}", num6), short.Parse(array[1])));
                        }
                        num6++;
                    }
                    NbtCompound nbtCompound2 = new NbtCompound("entries");
                    nbtCompound2.AddRange(list7);
                    mapPart11.Add(nbtCompound2);
                    List<NbtTag> list8 = new List<NbtTag>();
                    int num7 = 0;
                    while (tmxObject5.Properties.TryGetValue("tele" + num7, out text3))
                    {
                        string[] array2 = text3.Split(new char[]
                        {
                            ','
                        });
                        if (array2.Length >= 2)
                        {
                            list8.Add(new NbtString(string.Format("t{0}", num7), array2[0]));
                            list8.Add(new NbtShort(string.Format("f{0}", num7), short.Parse(array2[1])));
                        }
                        num7++;
                    }
                    NbtCompound nbtCompound3 = new NbtCompound("tele");
                    nbtCompound3.AddRange(list8);
                    mapPart11.Add(nbtCompound3);
                    mapPart10.Add(mapPart11);
                    num5++;
                }
            }
            if (mapPart10.Tags.Count > 0)
            {
                mapCompound.Add(mapPart10.Tag);
            }
            Mode++;
            MapPart mapPart12 = new MapPart("paths", true);
            objectsByType.TryGetValue("npc path", out List<TmxObjectGroup.TmxObject> list9);
            int num8 = 0;
            if (list9 != null)
            {
                foreach (TmxObjectGroup.TmxObject tmxObject6 in list9)
                {
                    MapPart mapPart13 = new MapPart(false);
                    mapPart13.Add("name", tmxObject6.Name);
                    List<NbtInt> list10 = new List<NbtInt>();
                    int num9 = 0;
                    if (tmxObject6.Points != null)
                    {
                        using (List<Tuple<int, int>>.Enumerator enumerator3 = tmxObject6.Points.GetEnumerator())
                        {
                            while (enumerator3.MoveNext())
                            {
                                Tuple<int, int> tuple2 = enumerator3.Current;
                                list10.Add(new NbtInt(tmxObject6.X + tuple2.Item1));
                                list10.Add(new NbtInt(tmxObject6.Y + tuple2.Item2));
                                num9++;
                            }
                            goto IL_E84;
                        }
                        goto IL_E6D;
                    IL_E84:
                        NbtList tag2 = new NbtList("coords", list10, NbtTagType.Int);
                        mapPart13.Add(tag2);
                        mapPart12.Add(mapPart13);
                        num8++;
                        continue;
                    }
                IL_E6D:
                    throw new Exception(string.Format("NPC Path does not contain points. Make sure you set \"{0}\" to the right type.", tmxObject6.Name));
                }
            }
            if (mapPart12.Tags.Count > 0)
            {
                mapCompound.Add(mapPart12.Tag);
            }
            Mode++;
            MapPart mapPart14 = new MapPart("areas", true);
            objectsByType.TryGetValue("npc area", out List<TmxObjectGroup.TmxObject> list11);
            int num10 = 0;
            if (list11 != null)
            {
                foreach (TmxObjectGroup.TmxObject tmxObject7 in list11)
                {
                    MapPart mapPart15 = new MapPart(false);
                    mapPart15.Add("name", tmxObject7.Name);
                    mapPart15.Add("x", tmxObject7.X);
                    mapPart15.Add("y", tmxObject7.Y);
                    mapPart15.Add("w", tmxObject7.Width);
                    mapPart15.Add("h", tmxObject7.Height);
                    mapPart14.Add(mapPart15);
                    num10++;
                }
            }
            if (mapPart14.Tags.Count > 0)
            {
                mapCompound.Add(mapPart14.Tag);
            }
            Mode++;
            MapPart mapPart16 = new MapPart("crowds", true);
            objectsByType.TryGetValue("crowd path", out List<TmxObjectGroup.TmxObject> list12);
            int num11 = 0;
            if (list12 != null)
            {
                foreach (TmxObjectGroup.TmxObject tmxObject8 in list12)
                {
                    MapPart mapPart17 = new MapPart(false);
                    mapPart16.Add(mapPart17);
                    mapPart17.AddFromDictionary<int>("mode", tmxObject8.Properties, "mode");
                    List<NbtShort> list13 = new List<NbtShort>();
                    tmxObject8.Properties.TryGetValue("sprites", out string text4);
                    if (text4 != null)
                    {
                        string[] array3 = text4.Split(new char[]
                        {
                            ','
                        });
                        int num12 = 0;
                        foreach (string s in array3)
                        {
                            list13.Add(new NbtShort(short.Parse(s)));
                            num12++;
                        }
                        NbtList tag3 = new NbtList("sprs", list13, NbtTagType.Short);
                        mapPart17.Add(tag3);
                        List<NbtInt> list14 = new List<NbtInt>();
                        int num13 = 0;
                        if (tmxObject8.Points != null)
                        {
                            using (List<Tuple<int, int>>.Enumerator enumerator3 = tmxObject8.Points.GetEnumerator())
                            {
                                while (enumerator3.MoveNext())
                                {
                                    Tuple<int, int> tuple3 = enumerator3.Current;
                                    list14.Add(new NbtInt(tuple3.Item1));
                                    list14.Add(new NbtInt(tuple3.Item2));
                                    num13++;
                                }
                                goto IL_1191;
                            }
                            goto IL_117A;
                        IL_1191:
                            NbtList tag4 = new NbtList("coords", list14, NbtTagType.Int);
                            mapPart17.Add(tag4);
                            num11++;
                            continue;
                        }
                    IL_117A:
                        throw new Exception(string.Format("Crowd Path does not contain points. Make sure you set \"{0}\" to the right type.", tmxObject8.Name));
                    }
                    throw new MapPartRequirementException(tmxObject8.Name, "sprites");
                }
            }
            if (mapPart16.Tags.Count > 0)
            {
                mapCompound.Add(mapPart16.Tag);
            }
            Mode++;
            MapPart mapPart18 = new MapPart("spawns", true);
            objectsByType.TryGetValue("enemy spawn", out List<TmxObjectGroup.TmxObject> list15);
            int num14 = 0;
            if (list15 != null)
            {
                foreach (TmxObjectGroup.TmxObject tmxObject9 in list15)
                {

                    NbtCompound nbtCompound4 = new NbtCompound();
                    mapPart18.Add(nbtCompound4);
                    Console.WriteLine($"3");
                    nbtCompound4.Add(new NbtInt("x", tmxObject9.X));
                    nbtCompound4.Add(new NbtInt("y", tmxObject9.Y));
                    nbtCompound4.Add(new NbtInt("w", tmxObject9.Width));
                    nbtCompound4.Add(new NbtInt("h", tmxObject9.Height));
                    tmxObject9.Properties.TryGetValue("flag", out string s2);
                    short value = short.Parse(s2);
                    nbtCompound4.Add(new NbtShort("flag", value));
                    List<NbtString> list16 = new List<NbtString>();
                    List<NbtByte> list17 = new List<NbtByte>();

                    IEnumerable<KeyValuePair<string, string>> thing = tmxObject9.Properties.Where(x => x.Key.Contains("enemy"));
                    foreach (KeyValuePair<string, string> strung in thing)
                    {
                        Console.WriteLine($"strung, {strung.Key}, {strung.Value}");
                        string[] array5 = strung.Value.Split(new char[]
                        {
                            ','
                        });
                        Console.WriteLine($"Enemy: {array5[0]}");
                        list16.Add(new NbtString(array5[0]));
                        list17.Add(new NbtByte(byte.Parse(array5[1])));
                    }
                    NbtList newTag = new NbtList("enids", list16, NbtTagType.String);
                    NbtList newTag2 = new NbtList("enfreqs", list17, NbtTagType.Byte);
                    nbtCompound4.Add(newTag);
                    nbtCompound4.Add(newTag2);
                    num14++;
                }
            }
            if (mapPart18.Tags.Count > 0)
            {
                mapCompound.Add(mapPart18.Tag);
            }
            Mode++;
            MapPart mapPart19 = new MapPart("parallax", true);
            objectsByType.TryGetValue("parallax", out List<TmxObjectGroup.TmxObject> list18);
            int num16 = 0;
            if (list18 != null)
            {
                foreach (TmxObjectGroup.TmxObject tmxObject10 in list18)
                {
                    MapPart mapPart20 = new MapPart(false);
                    mapPart19.Add(mapPart20);
                    mapPart20.AddFromDictionary<string>("spr", tmxObject10.Properties, "sprite");
                    mapPart20.AddFromDictionary<float>("vx", tmxObject10.Properties, "vx", 1f);
                    mapPart20.AddFromDictionary<float>("vy", tmxObject10.Properties, "vy", 1f);
                    mapPart20.Add(new NbtFloat("x", tmxObject10.X));
                    mapPart20.Add(new NbtFloat("y", tmxObject10.Y));
                    mapPart20.Add(new NbtFloat("w", tmxObject10.Width));
                    mapPart20.Add(new NbtFloat("h", tmxObject10.Height));
                    num16++;
                }
            }
            if (mapPart19.Tags.Count > 0)
            {
                mapCompound.Add(mapPart19.Tag);
            }
            Mode++;

            NbtList nbtList2 = new NbtList("mesh", NbtTagType.List);
            List<List<IntPoint>> list19 = BuildMesh();
            int num17 = 0;
            foreach (List<IntPoint> list20 in list19)
            {
                List<NbtInt> list21 = new List<NbtInt>();
                foreach (IntPoint intPoint in list20)
                {
                    list21.Add(new NbtInt((int)intPoint.X));
                    list21.Add(new NbtInt((int)intPoint.Y));
                }
                NbtList newTag3 = new NbtList(list21, NbtTagType.Int);
                nbtList2.Add(newTag3);
                num17++;
            }
            if (nbtList2.Count > 0)
            {
                mapCompound.Add(nbtList2);
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
                bool flag = true;

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
                            tileSize[heightWidthX] = (tileData.modifier << 16 | translatedGID);
                        }
                        else
                        {
                            tileSize[heightWidthX] = 0U;
                        }
                        flag = (flag && tileSize[heightWidthX] == 0U);
                    }
                }
                if (!flag)
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
        private TmxObjectGroup ObjectGroupByName(TmxList<TmxObjectGroup> list, string name)
        {
            foreach (TmxObjectGroup tmxObjectGroup in list)
            {
                if (tmxObjectGroup.Name.ToLower() == name.ToLower())
                {
                    return tmxObjectGroup;
                }
            }
            throw new Exception("Object group with name \"" + name + "\" does not exist.");
        }
        private bool ObjectGroupExists(TmxList<TmxObjectGroup> list, string name)
        {
            foreach (TmxObjectGroup tmxObjectGroup in list)
            {
                if (tmxObjectGroup.Name.ToLower() == name.ToLower())
                {
                    return true;
                }
            }
            return false;
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
