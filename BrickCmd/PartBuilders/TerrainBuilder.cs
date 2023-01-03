using ClipperLib;
using fNbt;
using System;
using System.Collections.Generic;
using TiledSharp;
using VEMC.Parts;

namespace VEMC.PartBuilders
{
    public static partial class MapPartBuilder
    {
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
