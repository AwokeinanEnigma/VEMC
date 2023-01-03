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

