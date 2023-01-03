using fNbt;
using System;
using System.Collections.Generic;
using TiledSharp;
using VEMC.Parts;

namespace VEMC.PartBuilders
{
    public static partial class MapPartBuilder
    {
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
#pragma warning disable CS0162 // Unreachable code detected - Reason for suppression - This code is reached.
                        goto Continue;
#pragma warning restore CS0162 // Unreachable code detected
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
    }
}
