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

namespace VEMC.PartBuilders
{
    public static partial class MapPartBuilder
    {
        public enum BuilderMode
        {
            None,
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


        public static BuilderMode mode = BuilderMode.None;
        public static List<MapPart> allMapParts = new List<MapPart>();




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


    }

}

