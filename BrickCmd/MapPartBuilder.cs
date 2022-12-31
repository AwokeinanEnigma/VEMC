using ClipperLib;
using fNbt;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;
using VEMC.Parts;

namespace VEMC
{
    public static class MapPartBuilder
    {
        public enum BuilderMode
        {
            BuildingBackgroundAudio,
            BuildingSfx,
            BuildingDoor,
            BuildingTriggers,
            BuildingNPCS,
            BuildingNPCPaths,
            BuildingNPCAreas,
            BuildingCrowds,
            BuildingEnemySpawns,
            BuildingParallax,
            BuildingMesh,
            BuildingTiles,
        }

        private static Dictionary<int, Point[]> collisionMasks = new Dictionary<int, Point[]>
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


        public static BuilderMode mode;
        public static List<MapPart> allMapParts = new List<MapPart>();

        public static void BuildSfx(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {
            mode = BuilderMode.BuildingSfx;

            MapPart audioSfx = new MapPart("audsfx", true);
            objectsByType.TryGetValue("sfx", out List<TmxObjectGroup.TmxObject> audioSfxObjects);

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
                }
            }

            if (audioSfx.Tags.Count > 0)
            {
                Debug.Log($"Total SFX: {audioSfx.Tags.Count}");

                mapCompound.Add(audioSfx.Tag);
                allMapParts.Add(audioSfx);
            }
        }

        public static void BuildBackgroundAudio(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {
            mode = BuilderMode.BuildingBackgroundAudio;

            MapPart bgm = new MapPart("audbgm", true);
            objectsByType.TryGetValue("bgm", out List<TmxObjectGroup.TmxObject> bgmObjectList);

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
                }
            }
            if (bgm.Tags.Count > 0)
            {
                Debug.Log($"Total audio: {bgm.Tags.Count}. This probably shouldn't be more than one (1).");

                mapCompound.Add(bgm.Tag);
                allMapParts.Add(bgm);
            }
        }

        public static void BuildDoor(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {
            mode = BuilderMode.BuildingDoor;

            MapPart doors = new MapPart("doors", true);
            objectsByType.TryGetValue("door", out List<TmxObjectGroup.TmxObject> doorObjects);

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
                }
            }
            if (doors.Tags.Count > 0)
            {
                Debug.Log($"Total doors: {doors.Tags.Count}");

                mapCompound.Add(doors.Tag);
                allMapParts.Add(doors);
            }
        }

        public static void BuildTriggers(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {

            mode = BuilderMode.BuildingTriggers;

            MapPart triggers = new MapPart("triggers", true);
            objectsByType.TryGetValue("trigger area", out List<TmxObjectGroup.TmxObject> triggerAreas);

            if (triggerAreas != null)
            {
                // Iterate through each object in the list
                foreach (TmxObjectGroup.TmxObject tmxObject4 in triggerAreas)
                {
                    MapPart triggerPart = new MapPart(false);
                    NbtList coordsList = new NbtList("coords", NbtTagType.Int);

                    // If the object is a polygon
                    if (tmxObject4.ObjectType == TmxObjectGroup.TmxObjectType.Polygon)
                    {
                        // Iterate through each point in the polygon
                        using (List<Tuple<int, int>>.Enumerator points = tmxObject4.Points.GetEnumerator())
                        {
                            while (points.MoveNext())
                            {
                                Tuple<int, int> tuple = points.Current;
                                coordsList.Add(new NbtInt(tuple.Item1));
                                coordsList.Add(new NbtInt(tuple.Item2));
                            }
                            goto AddTriggerPart;
                        }
                        goto Continue;
                    }
                    goto Continue;

                AddTriggerPart:
                    triggerPart.Add(coordsList);
                    triggerPart.AddFromDictionary<string>("scr", tmxObject4.Properties, "script");
                    triggerPart.AddFromDictionary<short>("flag", tmxObject4.Properties, "flag", 0);
                    triggerPart.Add("x", tmxObject4.X);
                    triggerPart.Add("y", tmxObject4.Y);
                    triggers.Add(triggerPart);
                    continue;

                Continue:
                    // If the object is a basic shape
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
                        goto AddTriggerPart;
                    }
                    goto AddTriggerPart;
                }
            }
            if (triggers.Tags.Count > 0)
            {
                Debug.Log($"Total triggers: {triggers.Tags.Count}");
                mapCompound.Add(triggers.Tag);

                allMapParts.Add(triggers);
            }
        }

        public static void BuildNPCS(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {
            mode = BuilderMode.BuildingNPCS;

            MapPart npcs = new MapPart("npcs", true);
            objectsByType.TryGetValue("npc", out List<TmxObjectGroup.TmxObject> npcsFromMap);

            if (npcsFromMap != null)
            {
                foreach (TmxObjectGroup.TmxObject npcObject in npcsFromMap)
                {
                    MapPart npcPart = new MapPart(false);
                    npcObject.Properties.TryGetValue("sprite", out string spriteName);

                    if (spriteName != null && spriteName.Trim().Length > 0)
                    {
                        npcPart.Add("x", npcObject.X + npcObject.Width / 2);
                        npcPart.Add("y", npcObject.Y + npcObject.Height / 2);
                        npcPart.AddFromDictionary<string>("spr", npcObject.Properties, "sprite", string.Empty);
                    }
                    else
                    {
                        npcPart.Add("x", npcObject.X);
                        npcPart.Add("y", npcObject.Y);
                        npcPart.Add("w", npcObject.Width);
                        npcPart.Add("h", npcObject.Height);
                    }

                    npcPart.Add("name", npcObject.Name);
                    npcPart.AddFromDictionary<byte>("dir", npcObject.Properties, "direction", 6);
                    npcPart.AddFromDictionary<byte>("mov", npcObject.Properties, "movement", 0);
                    npcPart.AddFromDictionary<float>("spd", npcObject.Properties, "speed", 1f);
                    npcPart.AddFromDictionary<short>("dst", npcObject.Properties, "distance", 20);
                    npcPart.AddFromDictionary<short>("dly", npcObject.Properties, "delay", 0);
                    npcPart.AddFromDictionary<string>("cnstr", npcObject.Properties, "constraint", string.Empty);
                    npcPart.AddFromDictionary<bool>("cls", npcObject.Properties, "collisions", true);
                    npcPart.AddFromDictionary<bool>("en", npcObject.Properties, "enabled", true);
                    npcPart.AddFromDictionary<short>("flag", npcObject.Properties, "flag", 0);
                    npcPart.AddFromDictionary<bool>("shdw", npcObject.Properties, "shadow", true);
                    npcPart.AddFromDictionary<bool>("stky", npcObject.Properties, "sticky", false);
                    npcPart.AddFromDictionary<int>("dpth", npcObject.Properties, "depth", int.MinValue);

                    List<NbtTag> textTags = new List<NbtTag>();

                    int textPropertyIndex = 0;
                    string npcText;

                    while (npcObject.Properties.TryGetValue("text" + textPropertyIndex, out npcText))
                    {
                        string[] nameAndIndex = npcText.Split(new char[]
                        {
                            ','
                        });
                        if (nameAndIndex.Length >= 2)
                        {
                            textTags.Add(new NbtString(string.Format("t{0}", textPropertyIndex), nameAndIndex[0]));
                            textTags.Add(new NbtShort(string.Format("f{0}", textPropertyIndex), short.Parse(nameAndIndex[1])));
                        }
                        textPropertyIndex++;
                    }

                    NbtCompound textEntries = new NbtCompound("entries");
                    textEntries.AddRange(textTags);
                    npcPart.Add(textEntries);

                    List<NbtTag> telepathyTags = new List<NbtTag>();
                    int telepathyPropertyIndex = 0;

                    while (npcObject.Properties.TryGetValue("tele" + telepathyPropertyIndex, out npcText))
                    {
                        string[] nameAndIndexTelepathy = npcText.Split(new char[]
                        {
                            ','
                        });
                        if (nameAndIndexTelepathy.Length >= 2)
                        {
                            telepathyTags.Add(new NbtString(string.Format("t{0}", telepathyPropertyIndex), nameAndIndexTelepathy[0]));
                            telepathyTags.Add(new NbtShort(string.Format("f{0}", telepathyPropertyIndex), short.Parse(nameAndIndexTelepathy[1])));
                        }
                        telepathyPropertyIndex++;
                    }

                    NbtCompound nbtCompound3 = new NbtCompound("tele");
                    nbtCompound3.AddRange(telepathyTags);
                    npcPart.Add(nbtCompound3);
                    npcs.Add(npcPart);
                }
            }
            if (npcs.Tags.Count > 0)
            {
                Debug.Log($"Total NPCs: {npcs.Tags.Count}");
                mapCompound.Add(npcs.Tag);

                allMapParts.Add(npcs);
            }

        }

        public static void BuildNPCPaths(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {
            mode = BuilderMode.BuildingNPCPaths;

            // Create a MapPart object for the paths
            MapPart paths = new MapPart("paths", true);

            // Get the list of TmxObjects with the type "npc path"
            objectsByType.TryGetValue("npc path", out List<TmxObjectGroup.TmxObject> pathObjects);

            // Initialize a counter for the number of path objects
            int pathCount = 0;

            // If there are any path objects
            if (pathObjects != null)
            {
                // Loop through each path object
                foreach (TmxObjectGroup.TmxObject pathObject in pathObjects)
                {
                    // Create a new MapPart object for this path
                    MapPart pathPart = new MapPart(false);

                    // Add the name of the path object as a tag
                    pathPart.Add("name", pathObject.Name);

                    // Initialize a list to store the coordinates of the path object's points
                    List<NbtInt> coords = new List<NbtInt>();

                    // Initialize a counter for the number of points in the path object
                    int pointCount = 0;

                    // If the path object has any points
                    if (pathObject.Points != null)
                    {
                        // Loop through each point in the path object
                        foreach (Tuple<int, int> point in pathObject.Points)
                        {
                            // Add the point's coordinates (offset by the path object's position) to the coords list
                            coords.Add(new NbtInt(pathObject.X + point.Item1));
                            coords.Add(new NbtInt(pathObject.Y + point.Item2));

                            // Increment the point counter
                            pointCount++;
                        }

                        // Create an NbtList to hold the coordinates
                        NbtList coordTag = new NbtList("coords", coords, NbtTagType.Int);

                        // Add the coordTag to the pathPart
                        pathPart.Add(coordTag);

                        // Add the pathPart to the pathsPart
                        paths.Add(pathPart);

                        // Increment the path counter
                        pathCount++;

                        // Move on to the next path object
                        continue;
                    }
                    else
                    {
                        // Throw an exception if the path object has no points
                        throw new Exception(string.Format("NPC Path does not contain points. Make sure you set \"{0}\" to the right type.", pathObject.Name));
                    }
                }
            }

            // If the pathsPart has any tags, add it to the mapCompound
            if (paths.Tags.Count > 0)
            {
                Debug.Log($"Total NPC paths: {paths.Tags.Count}");
                mapCompound.Add(paths.Tag);

                allMapParts.Add(paths);
            }

        }

        public static void BuildNPCAreas(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {
            mode = BuilderMode.BuildingNPCAreas;

            // Create a MapPart object for the areas
            MapPart areas = new MapPart("areas", true);

            // Get the list of TmxObjects with the type "npc area"
            objectsByType.TryGetValue("npc area", out List<TmxObjectGroup.TmxObject> areaObjects);

            // Initialize a counter for the number of area objects
            int areaCount = 0;

            // If there are any area objects
            if (areaObjects != null)
            {
                // Loop through each area object
                foreach (TmxObjectGroup.TmxObject areaObject in areaObjects)
                {
                    // Create a new MapPart object for this area
                    MapPart areaPart = new MapPart(false);

                    // Add the name, position, and size of the area object as tags
                    areaPart.Add("name", areaObject.Name);
                    areaPart.Add("x", areaObject.X);
                    areaPart.Add("y", areaObject.Y);
                    areaPart.Add("w", areaObject.Width);
                    areaPart.Add("h", areaObject.Height);

                    // Add the areaPart to the areasPart
                    areas.Add(areaPart);

                    // Increment the area counter
                    areaCount++;
                }
            }

            // If the areasPart has any tags, add it to the mapCompound
            if (areas.Tags.Count > 0)
            {
                Debug.Log($"Total NPC areas: {areas.Tags.Count}");
                mapCompound.Add(areas.Tag);

                allMapParts.Add(areas);
            }
        }

        public static void BuildCrowds(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {
            mode = BuilderMode.BuildingCrowds;

            // Create a MapPart object for the crowds
            MapPart crowds = new MapPart("crowds", true);

            // Get the list of TmxObjects with the type "crowd path"
            objectsByType.TryGetValue("crowd path", out List<TmxObjectGroup.TmxObject> crowdObjects);

            // Initialize a counter for the number of crowd objects
            int crowdCount = 0;

            // If there are any crowd objects
            if (crowdObjects != null)
            {
                // Loop through each crowd object
                foreach (TmxObjectGroup.TmxObject crowdObject in crowdObjects)
                {
                    // Create a new MapPart object for this crowd
                    MapPart crowdPart = new MapPart(false);

                    // Add the crowdPart to the crowdsPart
                    crowds.Add(crowdPart);

                    // Add the "mode" property of the crowd object as a tag, if it exists
                    crowdPart.AddFromDictionary<int>("mode", crowdObject.Properties, "mode");

                    // Initialize a list to store the sprites for this crowd
                    List<NbtShort> sprites = new List<NbtShort>();

                    // Try to get the "sprites" property of the crowd object
                    crowdObject.Properties.TryGetValue("sprites", out string spriteString);

                    // If the "sprites" property exists
                    if (spriteString != null)
                    {
                        // Split the sprite string into an array of sprite IDs
                        string[] spriteIds = spriteString.Split(new char[] { ',' });

                        // Initialize a counter for the number of sprites
                        int spriteCount = 0;

                        // Loop through each sprite ID
                        foreach (string spriteId in spriteIds)
                        {
                            // Convert the sprite ID to a short and add it to the sprites list
                            sprites.Add(new NbtShort(short.Parse(spriteId)));

                            // Increment the sprite counter
                            spriteCount++;
                        }

                        // Create an NbtList to hold the sprites
                        NbtList spriteTag = new NbtList("sprs", sprites, NbtTagType.Short);

                        // Add the spriteTag to the crowdPart
                        crowdPart.Add(spriteTag);

                        // Initialize a list to store the coordinates of the crowd object's points
                        List<NbtInt> coords = new List<NbtInt>();

                        // Initialize a counter for the number of points in the crowd object
                        int pointCount = 0;

                        // If the crowd object has any points
                        if (crowdObject.Points != null)
                        {
                            // Loop through each point in the crowd object
                            foreach (Tuple<int, int> point in crowdObject.Points)
                            {
                                // Add the point's coordinates to the coords list
                                coords.Add(new NbtInt(point.Item1));
                                coords.Add(new NbtInt(point.Item2));

                                // Increment the point counter
                                pointCount++;
                            }

                            // Create an NbtList to hold the coordinates
                            NbtList coordTag = new NbtList("coords", coords, NbtTagType.Int);

                            // Add the coordTag to the crowdPart
                            crowdPart.Add(coordTag);

                            // Increment the crowd counter
                            crowdCount++;

                            // Move on to the next crowd object
                            continue;
                        }
                        else
                        {
                            // Throw an exception if the crowd object has no points
                            throw new Exception(string.Format("Crowd Path does not contain points. Make sure you set \"{0}\" to the right type.", crowdObject.Name));
                        }
                    }
                    else
                    {
                        // Throw an exception if the crowd object has no sprites
                        throw new MapPartRequirementException(crowdObject.Name, "sprites");
                    }
                }
            }

            // If the crowdsPart has any tags, add it to the mapCompound
            if (crowds.Tags.Count > 0)
            {
                Debug.Log($"Total crowds: {crowds.Tags.Count}");
                mapCompound.Add(crowds.Tag);

                allMapParts.Add(crowds);
            }
        }

        public static void BuildEnemySpawns(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {
            mode = BuilderMode.BuildingEnemySpawns;

            // Create a MapPart object for the enemy spawns
            MapPart enemySpawns = new MapPart("spawns", true);

            // Get the list of TmxObjects with the type "enemy spawn"
            objectsByType.TryGetValue("enemy spawn", out List<TmxObjectGroup.TmxObject> enemySpawnObjects);

            // Initialize a counter for the number of enemy spawn objects
            int enemySpawnCount = 0;

            // If there are any enemy spawn objects
            if (enemySpawnObjects != null)
            {
                // Loop through each enemy spawn object
                foreach (TmxObjectGroup.TmxObject enemySpawnObject in enemySpawnObjects)
                {
                    // Create a new NbtCompound to hold the enemy spawn data
                    NbtCompound enemySpawnData = new NbtCompound();

                    // Add the enemySpawnData to the enemySpawnsPart
                    enemySpawns.Add(enemySpawnData);

                    // Add the position and size of the enemy spawn object as tags
                    enemySpawnData.Add(new NbtInt("x", enemySpawnObject.X));
                    enemySpawnData.Add(new NbtInt("y", enemySpawnObject.Y));
                    enemySpawnData.Add(new NbtInt("w", enemySpawnObject.Width));
                    enemySpawnData.Add(new NbtInt("h", enemySpawnObject.Height));

                    // Try to get the "flag" property of the enemy spawn object
                    enemySpawnObject.Properties.TryGetValue("flag", out string flagString);

                    // If the "flag" property exists, convert it to a short and add it as a tag
                    if (flagString != null)
                    {
                        short flagValue = short.Parse(flagString);
                        enemySpawnData.Add(new NbtShort("flag", flagValue));
                    }

                    // Initialize lists to hold the enemy IDs and frequencies for this enemy spawn object
                    List<NbtString> enemyIds = new List<NbtString>();
                    List<NbtByte> enemyFrequencies = new List<NbtByte>();

                    // Get a list of the properties of the enemy spawn object that contain the word "enemy"
                    IEnumerable<KeyValuePair<string, string>> enemyProperties = enemySpawnObject.Properties.Where(x => x.Key.Contains("enemy"));

                    // Loop through each enemy property of the enemy spawn object
                    foreach (KeyValuePair<string, string> enemyProperty in enemyProperties)
                    {
                        // Split the enemy property value into an array of enemy ID and frequency
                        string[] enemyData = enemyProperty.Value.Split(new char[] { ',' });

                        // Add the enemy ID and frequency to their respective lists
                        enemyIds.Add(new NbtString(enemyData[0]));
                        enemyFrequencies.Add(new NbtByte(byte.Parse(enemyData[1])));
                    }

                    // Create NbtLists to hold the enemy IDs and frequencies
                    NbtList enemyIdTag = new NbtList("enids", enemyIds, NbtTagType.String);
                    NbtList enemyFrequencyTag = new NbtList("enfreqs", enemyFrequencies, NbtTagType.Byte);

                    // Add the enemy ID and frequency tags to the enemy spawn data
                    enemySpawnData.Add(enemyIdTag);
                    enemySpawnData.Add(enemyFrequencyTag);

                    // Increment the enemy spawn count
                    enemySpawnCount++;
                }
            }

            // If the enemySpawnsPart has any tags, add it to the mapCompound
            if (enemySpawns.Tags.Count > 0)
            {
                Debug.Log($"Total enemy spawns: {enemySpawns.Tags.Count}");
                mapCompound.Add(enemySpawns.Tag);

                allMapParts.Add(enemySpawns);
            }
        }

        public static void BuildParallax(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType)
        {
            mode = BuilderMode.BuildingParallax;

            // Create a MapPart object for the parallax objects
            MapPart parallax = new MapPart("parallax", true);

            // Get the list of TmxObjects with the type "parallax"
            objectsByType.TryGetValue("parallax", out List<TmxObjectGroup.TmxObject> parallaxObjects);


            // If there are any parallax objects
            if (parallaxObjects != null)
            {
                // Loop through each parallax object
                foreach (TmxObjectGroup.TmxObject parallaxObject in parallaxObjects)
                {
                    // Create a new MapPart object to hold the parallax data
                    MapPart parallaxData = new MapPart(false);

                    // Add the parallaxData to the parallaxPart
                    parallax.Add(parallaxData);

                    // Add the sprite, x and y velocity, and position and size of the parallax object as tags
                    parallaxData.AddFromDictionary<string>("spr", parallaxObject.Properties, "sprite");
                    parallaxData.AddFromDictionary<int>("shdrmde", parallaxObject.Properties, "shaderMode");

                    parallaxData.AddFromDictionary<float>("vx", parallaxObject.Properties, "vx", 1f);
                    parallaxData.AddFromDictionary<float>("vy", parallaxObject.Properties, "vy", 1f);
                    parallaxData.Add(new NbtFloat("x", parallaxObject.X));
                    parallaxData.Add(new NbtFloat("y", parallaxObject.Y));
                    parallaxData.Add(new NbtFloat("w", parallaxObject.Width));
                    parallaxData.Add(new NbtFloat("h", parallaxObject.Height));
                }
            }

            // If the parallaxPart has any tags, add it to the mapCompound
            if (parallax.Tags.Count > 0)
            {
                Debug.Log($"Total parallax: {parallax.Tags.Count}");
                mapCompound.Add(parallax.Tag);

                allMapParts.Add(parallax);
            }
        }

        public static void BuildMesh(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType, TmxMap map)
        {
            mode = BuilderMode.BuildingMesh;

            // Create a new NbtList with the name "mesh" and NbtTagType of List
            NbtList meshList = new NbtList("mesh", NbtTagType.List);

            // Build the mesh and store it in a list of lists of IntPoints
            List<List<IntPoint>> mesh = BuildMesh(map);

            // Initialize a counter variable
            int counter = 0;

            // Iterate through each list of IntPoints in the mesh
            foreach (List<IntPoint> intPointList in mesh)
            {
                // Create a new list to store the NbtInts
                List<NbtInt> nbtIntList = new List<NbtInt>();

                // Iterate through each IntPoint in the list
                foreach (IntPoint intPoint in intPointList)
                {
                    // Add the X and Y values of the IntPoint as NbtInts to the list
                    nbtIntList.Add(new NbtInt((int)intPoint.X));
                    nbtIntList.Add(new NbtInt((int)intPoint.Y));
                }

                // Create a new NbtList with the list of NbtInts and NbtTagType of Int
                NbtList innerList = new NbtList(nbtIntList, NbtTagType.Int);

                // Add the inner list to the meshList
                meshList.Add(innerList);

                // Increment the counter variable
                counter++;
            }

            // If the meshList has at least one element, add it to the mapCompound
            if (meshList.Count > 0)
            {
                Debug.Log($"Total mesh tags: {meshList.Count}");
                mapCompound.Add(meshList);
            }
        }

        public static void BuildTiles(NbtCompound mapCompound, Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType, TmxMap map, Dictionary<string, OptimizedTileset> optimizedTilesets)
        {
            mode = BuilderMode.BuildingTiles;

            // Create a new NbtList with the name "tiles" and NbtTagType of Compound
            NbtList tilesList = new NbtList("tiles", NbtTagType.Compound);

            // Create a new TileGrouper for the map
            TileGrouper tileGrouper = new TileGrouper(map);

            // Initialize a counter variable
            int counter = 0;

            // Find the tile groups
            List<TileGroup> tileGroups = tileGrouper.FindGroups();

            // Iterate through each tile group
            foreach (TileGroup tileGroup in tileGroups)
            {
                // Create a new MapPart with false as the parameter
                MapPart mapPart = new MapPart(false);

                // Add the depth, x, y, originX, and rainaway values as NbtInts to the mapPart
                mapPart.Add(new NbtInt("depth", tileGroup.depth));
                mapPart.Add(new NbtInt("x", tileGroup.x));
                mapPart.Add(new NbtInt("y", tileGroup.y));
                mapPart.Add(new NbtInt("ox", tileGroup.originX));
                mapPart.Add(new NbtInt("rainaway", tileGroup.depth));

                // Calculate the width and height of the tile group in tiles
                int widthInTiles = tileGroup.width / map.TileWidth;
                int heightInTiles = tileGroup.height / map.TileHeight;

                // Calculate the x and y position of the tile group in tiles
                int groupTileX = tileGroup.x / map.TileWidth;
                int groupTileY = tileGroup.y / map.TileHeight;

                // Add the width as an NbtInt to the mapPart
                mapPart.Add(new NbtInt("w", widthInTiles));

                // Initialize an array to store the tiles in the tile group
                uint[] tiles = new uint[widthInTiles * heightInTiles];

                // Initialize a flag to keep track if all the tiles in the group are empty
                bool allTilesEmpty = true;

                // Get the first optimized tileset for the map
                OptimizedTileset optimizedTileset = optimizedTilesets[map.Tilesets[0].Name];

                // Iterate through each tile in the group
                for (int y = 0; y < heightInTiles; y++)
                {
                    for (int x = 0; x < widthInTiles; x++)
                    {
                        // Find the TileData for the current tile
                        TileGrouper.TileData tileData = tileGroup.tiles.Find((TileGrouper.TileData t) => t.tile.X == x + groupTileX && t.tile.Y == y + groupTileY);

                        // Calculate the index of the tile in the tiles array
                        int index = y * widthInTiles + x;

                        if (tileData.tile != null)
                        {
                            // Check if the tile has a "rainaway" property
                            TmxTilesetTile tile = map.Tilesets[0].GetTileById(tileData.tile.Gid - 1);
                            if (tile != null && tile.Properties != null && tile.Properties.TryGetValue("rainaway", out string _false))
                            {
                                Console.WriteLine("rainaway");
                            }

                            // Check if the GID is valid
                            if (tileData.tile.Gid > optimizedTileset.TranslationTable.Count)
                            {
                                Console.WriteLine($"GID '{tileData.tile.Gid}' at tile '{tileData.tile.X},{tileData.tile.Y}' when the maximum is {optimizedTileset.TranslationTable.Count}");
                                continue;
                            }

                            // Translate the GID and store it in a variable
                            uint translatedGid = (uint)(optimizedTileset.Translate(tileData.tile.Gid));

                            // Set the value in the tiles array to the modifier and translated GID
                            tiles[index] = (tileData.modifier << 16 | translatedGid);
                        }
                        else
                        {
                            // Set the value in the tiles array to 0 if there is no tile data
                            tiles[index] = 0U;
                        }

                        // Set the allTilesEmpty flag to false if the current tile is not empty
                        allTilesEmpty = (allTilesEmpty && tiles[index] == 0U);
                    }
                }
                // If there are any non-empty tiles in the group, add the tiles array to the mapPart as a byte array
                if (!allTilesEmpty)
                {
                    byte[] tilesByteArray = new byte[tiles.Length * 4];
                    Buffer.BlockCopy(tiles, 0, tilesByteArray, 0, tilesByteArray.Length);
                    mapPart.Add("tiles", tilesByteArray);

                    // Add the mapPart to the tilesList
                    tilesList.Add(mapPart.Tag);

                    // Increment the counter variable
                    counter++;
                }
            }
            if (tilesList.Count > 0)
            {
                Debug.Log($"Total tiles: {tilesList.Count}");
                mapCompound.Add(tilesList);
            }
        }

        private static List<List<IntPoint>> BuildMesh(TmxMap map)
        {
            // Get the height and width of the map in tiles
            int mapHeight = map.Height;
            int mapWidth = map.Width;

            // Get the first tileset for the map
            TmxTileset tileset = map.Tilesets[0];

            // Calculate the total number of tiles in the map
            int numTiles = mapWidth * mapHeight * map.Layers.Count;

            // Initialize a counter variable
            int counter = 0;

            // Create a new MeshBuilder
            MeshBuilder meshBuilder = new MeshBuilder();

            // Initialize a string to store the "solidity" property value
            string solidity = null;

            // Iterate through each layer in the map, starting from the last layer
            for (int i = map.Layers.Count - 1; i >= 0; i--)
            {
                // Get the name of the current layer
                string layerName = map.Layers[i].Name;

                // Get the number of tiles in the current layer
                int tileCount = map.Layers[i].Tiles.Count;

                // Iterate through each tile in the current layer
                for (int j = 0; j < tileCount; j++)
                {
                    // Get the current tile
                    TmxLayerTile tmxLayerTile = map.Layers[i].Tiles[j];

                    // Get the GID of the current tile and subtract 1
                    int gid = tmxLayerTile.Gid - 1;

                    // Get the x and y position of the current tile in tiles
                    int x = tmxLayerTile.X;
                    int y = tmxLayerTile.Y;

                    // Get the TmxTilesetTile for the current tile
                    TmxTilesetTile tileById = tileset.GetTileById(gid);
                    // If the TmxTilesetTile for the current tile exists
                    if (tileById != null)
                    {
                        // Initialize a variable to store the "solidity" property value as an integer
                        int solidityInt = 0;

                        // Try to get the "solidity" property value from the TmxTilesetTile and parse it as an integer
                        if (tileById.Properties.TryGetValue("solidity", out solidity) && int.TryParse(solidity, out solidityInt))
                        {
                            // If the "solidity" value is greater than 0, add a path to the MeshBuilder using the x and y position and the corresponding collision mask
                            if (solidityInt > 0)
                            {
                                meshBuilder.AddPath(x * map.TileWidth, y * map.TileHeight, collisionMasks[solidityInt]);
                            }
                        }
                    }

                    // Increment the counter variable
                    counter++;

                    // Print the progress to the console every 100 tiles
                    if (counter % 100 == 0)
                    {
                        Utility.ConsoleWrite("Building collision mesh from \"{0}\" ({1}%)", new object[]
                        {
              layerName,
             Math.Round(counter / (double)numTiles * 100.0)
                        });
                    }
                }

            }
            Utility.ConsoleWrite("Simplifying collision mesh...                                     ", new object[0]);
            meshBuilder.Simplify();
            return meshBuilder.Solution;
        }
    }

}

