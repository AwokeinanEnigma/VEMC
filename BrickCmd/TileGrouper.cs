using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using TiledSharp;

namespace VEMC
{
    internal class TileGrouper
    {
        public TileGrouper(TmxMap map)
        {
            this.map = map;
            tileset = map.Tilesets[0];
            offsets = new Dictionary<int, Point>();
            animations = new List<TileGrouper.TileAnimation>();
        }
        public List<TileGroup> FindGroups()
        {
            // Create a list to store the tile groups
            List<TileGroup> tileGroups = new List<TileGroup>();

            // Get the group offsets
            offsets = GetGroupOffsets();

            // Iterate through each layer in the map
            foreach (TmxLayer layer in map.Layers)
            {
                // Find the groups in the current layer
                List<TileGroup> layerTileGroups = FindGroupsInLayer(layer);

                // Add the groups to the list of tile groups
                tileGroups.AddRange(layerTileGroups);
            }

            // Return the list of tile groups
            return tileGroups;

        }
        private Dictionary<int, Point> GetGroupOffsets()
        {
            // Create a dictionary to store the group origins
            Dictionary<int, Point> groupOrigins = new Dictionary<int, Point>();

            // Get the "grouporigins" property from the map's properties
            string groupOriginsString;
            if (map.Properties.TryGetValue("grouporigins", out groupOriginsString))
            {
                // Split the string by ';' to get a list of group origin strings
                string[] groupOriginStrings = groupOriginsString.Split(';');

                // Iterate through each group origin string
                foreach (string groupOriginString in groupOriginStrings)
                {
                    // Split the group origin string by ',' to get the group ID, X, and Y
                    string[] groupOriginFields = groupOriginString.Split(',');
                    if (groupOriginFields.Length == 3)
                    {
                        // Parse the group ID, X, and Y from the group origin fields
                        int groupId, x, y;
                        if (int.TryParse(groupOriginFields[0], out groupId) &&
                            int.TryParse(groupOriginFields[1], out x) &&
                            int.TryParse(groupOriginFields[2], out y))
                        {
                            // Add the group origin to the dictionary if it doesn't already exist
                            if (!groupOrigins.ContainsKey(groupId))
                            {
                                groupOrigins.Add(groupId, new Point(x, y));
                            }
                        }
                    }
                }
            }

            // Return the dictionary of group origins
            return groupOrigins;
        }
        private uint GetTileModifier(TmxLayerTile tile)
        {
            uint modifier = 0;

            // Check if the tile is horizontally flipped
            if (tile.HorizontalFlip)
            {
                // Set the first bit to 1
                modifier |= 1;
            }

            // Check if the tile is vertically flipped
            if (tile.VerticalFlip)
            {
                // Set the second bit to 1
                modifier |= 2;
            }

            // Check if the tile is diagonally flipped
            if (tile.DiagonalFlip)
            {
                // Set the third bit to 1
                modifier |= 4;
            }

            // Get the tile by its GID
            TmxTilesetTile tileById = tileset.GetTileById(tile.Gid - 1);

            // Check if the tile exists and has a property called "actualid"
            if (tileById != null && tileById.Properties.ContainsKey("actualid"))
            {
                // Get the value of "actualid" and add 1 to it
                uint actualId = (uint)tileById.Properties.TryGetDecimal("actualid") + 1;

                // Set the fourth to seventh bits to the value of "actualid"
                modifier |= actualId << 3;
            }

            // Return the modifier
            return modifier;
        }
        public List<TileGroup> FindGroupsInLayer(TmxLayer layer)
        {
            // Initialize variables
            List<TileGroup> tileGroupList = new List<TileGroup>();
            TileGrouper.TileData[,] tileDataArray = new TileGrouper.TileData[map.Width, map.Height];

            // Iterate through all tiles in the layer
            foreach (TmxLayerTile tmxLayerTile in layer.Tiles)
            {
                // Calculate the id of the tile relative to the tileset
                int tileId = tmxLayerTile.Gid - tileset.FirstGid;

                // Get the TmxTilesetTile object for the current tile
                TmxTilesetTile tileById = tileset.GetTileById(tileId);

                // Get the modifier for the current tile
                uint tileModifier = GetTileModifier(tmxLayerTile);

                // If the tile exists in the tileset
                if (tileById != null)
                {
                    // If the tile has a "group" property
                    if (tileById.Properties.ContainsKey("group"))
                    {
                        // Get the "groupmeta" property
                        string groupMeta = tileById.Properties["groupmeta"];

                        // Split the "groupmeta" string into an array of strings
                        string[] metaArray = groupMeta.Split(new char[]
                        {
                            ','
                        });

                        // Add the current tile to the tileDataArray with the specified group id and metadata
                        tileDataArray[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                        {
                            tile = tmxLayerTile,
                            groupid = int.Parse(tileById.Properties["group"]),
                            left = (metaArray[0] == "1"),
                            top = (metaArray[1] == "1"),
                            right = (metaArray[2] == "1"),
                            bottom = (metaArray[3] == "1"),
                            modifier = tileModifier
                        };
                    }
                    else
                    {
                        // Add the current tile to the tileDataArray with a group id of -1
                        tileDataArray[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                        {
                            tile = tmxLayerTile,
                            groupid = -1,
                            modifier = tileModifier
                        };
                    }
                }
                else if (tileId == -1)
                {
                    // Add the current tile to the tileDataArray with a group id of -3 and ignore set to true
                    tileDataArray[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                    {
                        groupid = -3,
                        ignore = true,
                        modifier = tileModifier
                    };
                }
                else
                {
                    // Add the current tile to the tileDataArray with a group id of -1
                    tileDataArray[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                    {
                        tile = tmxLayerTile,
                        groupid = -1,
                        modifier = tileModifier
                    };
                }

            }
            Parallel.For(0, map.Height, i =>
            {
                for (int j = 0; j < map.Width; j++)
                {
                    TileGrouper.TileData tileData = tileDataArray[j, i];
                    if (!tileData.ignore && tileData.groupid > 0)
                    {
                        TileGroup item = FloodFillGroupGet(ref tileDataArray, j, i, tileData.groupid);
                        tileGroupList.Add(item);
                    }
                }
            });
            Utility.ConsoleWrite("Slicing tile layers...", new object[0]);
            List<TileGroup> collection = SliceTileLayer(ref tileDataArray, layer.Properties.ContainsKey("depth") ? int.Parse(layer.Properties["depth"]) : 0);
            tileGroupList.AddRange(collection);

            return tileGroupList;
        }
        private List<TileGroup> SliceTileLayer(ref TileGrouper.TileData[,] tiles, int layerDepth)
        {
            // Initialize variables
            List<TileGroup> tileGroups = new List<TileGroup>();
            List<uint> layerModifiers = new List<uint>();
            int groupWidth = 40;
            int groupHeight = 22;

            // Iterate through the map and create TileGroups in a grid pattern with a width and height of 40 and 22 tiles, respectively
            for (int i = 0; i < map.Height; i += groupHeight)
            {
                for (int j = 0; j < map.Width; j += groupWidth)
                {
                    // Initialize a list to store TileData objects for this TileGroup
                    List<TileGrouper.TileData> tilesInGroup = new List<TileGrouper.TileData>();

                    // Iterate through the tiles within the current grid cell
                    for (int y = i; y < i + groupHeight && y < map.Height; y++)
                    {
                        for (int x = j; x < j + groupWidth && x < map.Width; x++)
                        {
                            // If the tile is not marked as ignored, add it to the list of tiles for this TileGroup
                            if (!tiles[x, y].ignore)
                            {
                                tilesInGroup.Add(tiles[x, y]);

                                // If the tile has a modifier, add it to the list of modifiers for this layer
                                if (tiles[x, y].modifier > 0)
                                {
                                    layerModifiers.Add(tiles[x, y].modifier);
                                }
                            }
                        }
                    }

                    // If the list of tiles for this TileGroup is not empty, create a new TileGroup object and add it to the list
                    if (tilesInGroup.Count > 0)
                    {
                        TileGroup group = new TileGroup
                        {
                            tiles = tilesInGroup,
                            x = j * 8,
                            y = i * 8,
                            originX = 0,
                            originY = 0,
                            depth = layerDepth,
                            width = 320,
                            height = 180
                        };
                        tileGroups.Add(group);
                    }
                }
            }

            // Return the list of TileGroups
            return tileGroups;
        }
        private TileGroup FloodFillGroupGet(ref TileGrouper.TileData[,] tiles, int x, int y, int groupid)
        {
            List<TileGrouper.TileData> list = new List<TileGrouper.TileData>();
            FloodFillStep(ref tiles, x, y, groupid, ref list);

            // Initialize variables for storing the min and max x and y values of the tiles in the group
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            // Iterate through each tile in the group
            foreach (TileGrouper.TileData tileData in list)
            {
                // Update the min and max x and y values if necessary
                minX = Math.Min(minX, tileData.tile.X);
                maxX = Math.Max(maxX, tileData.tile.X);
                minY = Math.Min(minY, tileData.tile.Y);
                maxY = Math.Max(maxY, tileData.tile.Y);
            }

            // Calculate the x and y position of the tile group in tiles
            int groupTileX = minX;
            int groupTileY = minY;

            // Calculate the width and height of the tile group in tiles
            int widthInTiles = maxX - minX + 1;
            int heightInTiles = maxY - minY + 1;

            // Calculate the x and y position of the tile group in pixels
            int groupTileXInPixels = groupTileX * 8;
            int groupTileYInPixels = groupTileY * 8;

            // Calculate the width and height of the tile group in pixels
            int widthInPixels = widthInTiles * 8;
            int heightInPixels = heightInTiles * 8;

            offsets.TryGetValue(groupid, out Point point);

            // Calculate the origin x and y position of the tile group in pixels
            int originX = (widthInPixels / 2) + point.X;
            int originY = (heightInPixels - 4) + point.Y;

            // Return a new TileGroup object with the calculated values
            return new TileGroup
            {
                tiles = list,
                x = groupTileXInPixels,
                y = groupTileYInPixels,
                originX = originX,
                originY = originY,
                depth = groupTileYInPixels + heightInPixels,
                width = widthInPixels,
                height = heightInPixels
            };
        }
        private void FloodFillStep(ref TileGrouper.TileData[,] tiles, int x, int y, int groupid, ref List<TileGrouper.TileData> group)
        {
            if (PointInBounds(x, y) && tiles[x, y].groupid == groupid && !tiles[x, y].ignore)
            {
                // Store the tile data in a local variable for easier access
                TileGrouper.TileData tileData = tiles[x, y];

                // Add the tile data to the group
                group.Add(tileData);

                // Mark the tile as "ignore" to prevent it from being processed again
                tiles[x, y].ignore = true;

                // Recursively process the tiles in the four cardinal directions if the corresponding flags are set
                if (!tileData.left) FloodFillStep(ref tiles, x - 1, y, groupid, ref group);
                if (!tileData.top) FloodFillStep(ref tiles, x, y - 1, groupid, ref group);
                if (!tileData.right) FloodFillStep(ref tiles, x + 1, y, groupid, ref group);
                if (!tileData.bottom) FloodFillStep(ref tiles, x, y + 1, groupid, ref group);

                // Return early to avoid unnecessary processing
                return;
            }
        }
        private bool PointInBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < map.Width && y < map.Height;
        }
        private const int CHUNK_WIDTH = 320;
        private const int CHUNK_HEIGHT = 180;
        private const int TILE_WIDTH = 8;
        private const int TILE_HEIGHT = 8;
        private readonly TmxMap map;
        private readonly TmxTileset tileset;
        private Dictionary<int, Point> offsets;
        private readonly List<TileGrouper.TileAnimation> animations;
        public struct TileAnimation
        {
            public ushort id;
            public float speed;
            public int[] indexes;
        }
        public struct TileData
        {
            public TmxLayerTile tile;
            public int groupid;
            public bool left;
            public bool top;
            public bool right;
            public bool bottom;
            public bool ignore;
            public uint modifier;
        }
    }
}
