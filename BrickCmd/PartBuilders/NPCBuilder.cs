using fNbt;
using System;
using System.Collections.Generic;
using TiledSharp;
using VEMC.Parts;

namespace VEMC.PartBuilders
{
    public static partial class MapPartBuilder
    {
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
    }
}
