using System;
using System.Collections.Generic;
using System.Drawing;
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
            List<TileGroup> list = new List<TileGroup>();
            offsets = GetGroupOffsets();
            foreach (TmxLayer layer in map.Layers)
            {
                List<TileGroup> collection = FindGroupsInLayer(layer);
                list.AddRange(collection);
            }
            return list;
        }
        private Dictionary<int, Point> GetGroupOffsets()
        {
            Dictionary<int, Point> dictionary = new Dictionary<int, Point>();
            map.Properties.TryGetValue("grouporigins", out string text);
            if (text != null)
            {
                string[] array = text.Split(new char[]
                {
                    ';'
                });
                int num = array.Length;
                for (int i = 0; i < num; i++)
                {
                    string[] array2 = array[i].Split(new char[]
                    {
                        ','
                    });
                    if (array2.Length == 3)
                    {
                        int.TryParse(array2[0], out int key);
                        int.TryParse(array2[1], out int x);
                        int.TryParse(array2[2], out int y);
                        if (!dictionary.ContainsKey(key))
                        {
                            dictionary.Add(key, new Point(x, y));
                        }
                    }
                }
            }
            return dictionary;
        }
        private uint GetTileModifier(TmxLayerTile tile)
        {
            uint num = 0U;
            if (tile.HorizontalFlip)
            {
                num |= 1U;
            }
            if (tile.VerticalFlip)
            {
                num |= 2U;
            }
            if (tile.DiagonalFlip)
            {
                num |= 4U;
            }
            TmxTilesetTile tileById = tileset.GetTileById(tile.Gid - 1);
            if (tileById != null && tileById.Properties.ContainsKey("actualid"))
            {
                uint num2 = (uint)tileById.Properties.TryGetDecimal("actualid") + 1U;
                num |= num2 << 3;
            }
            return num;
        }
        public List<TileGroup> FindGroupsInLayer(TmxLayer layer)
        {
            List<TmxLayerTile> tiles = layer.Tiles;
            List<TileGroup> tileGroupList = new List<TileGroup>();
            TileGrouper.TileData[,] tileDataArray = new TileGrouper.TileData[map.Width, map.Height];

            int num = 0;
            int tileCount = tiles.Count;
            foreach (TmxLayerTile tmxLayerTile in tiles)
            {
                int num2 = tmxLayerTile.Gid - tileset.FirstGid;
                TmxTilesetTile tileById = tileset.GetTileById(num2);
                uint tileModifier = GetTileModifier(tmxLayerTile);
                if (tileById != null)
                {
                    if (tileById.Properties.ContainsKey("group"))
                    {
                        string text = tileById.Properties["groupmeta"];
                        string[] array2 = text.Split(new char[]
                        {
                            ','
                        });
                        tileDataArray[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                        {
                            tile = tmxLayerTile,
                            groupid = int.Parse(tileById.Properties["group"]),
                            left = (array2[0] == "1"),
                            top = (array2[1] == "1"),
                            right = (array2[2] == "1"),
                            bottom = (array2[3] == "1"),
                            modifier = tileModifier,
                                                  tree = tileById.Properties.ContainsKey("tree") ? true : false,
                        };
                    }
                    else
                    {
                        tileDataArray[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                        {
                            tile = tmxLayerTile,
                            groupid = -1,
                            modifier = tileModifier,
                    //        tree = tileById.Properties.ContainsKey("tree") ? true : false,

                        };
                    }
                }
                else if (num2 == -1)
                {
                    tileDataArray[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                    {
                        groupid = -3,
                        ignore = true,
                        modifier = tileModifier,
            //                                 tree = tileById.Properties.ContainsKey("tree") ? true : false,
                    };
                }
                else
                {
                    tileDataArray[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                    {
                        tile = tmxLayerTile,
                        groupid = -1,
                        modifier = tileModifier,
                                               //tree = tileById.Properties.ContainsKey("tree") ? true : false,
                    };
                }
                if (num % 100 == 0)
                {
                    Utility.ConsoleWrite("Finding tile groups in \"{0}\"({1}%)", new object[]
                    {
                        layer.Name,
                        Math.Round(num / (double)tileCount * 100.0)
                    });
                }
                num++;
            }
            for (int i = 0; i < map.Height; i++)
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
            }
            Utility.ConsoleWrite("Slicing tile layers...", new object[0]);
            List<TileGroup> collection = SliceTileLayer(ref tileDataArray, layer.Properties.ContainsKey("depth") ? int.Parse(layer.Properties["depth"]) : 0);
            tileGroupList.AddRange(collection);

            return tileGroupList;
        }
        private List<TileGroup> SliceTileLayer(ref TileGrouper.TileData[,] tiles, int layerDepth)
        {
            List<TileGroup> tileGroup = new List<TileGroup>();
            List<uint> layerUint = new List<uint>();
            int forty = 40;
            int twentytwo = 22;
            for (int i = 0; i < map.Height; i += twentytwo)
            {
                for (int j = 0; j < map.Width; j += forty)
                {
                    List<TileGrouper.TileData> list3 = new List<TileGrouper.TileData>();
                    int tileLoop = i;
                    while (tileLoop < i + twentytwo && tileLoop < map.Height)
                    {
                        int num4 = j;
                        while (num4 < j + forty && num4 < map.Width)
                        {
                            if (!tiles[num4, tileLoop].ignore)
                            {
                                list3.Add(tiles[num4, tileLoop]);
                                if (tiles[num4, tileLoop].modifier > 0U)
                                {
                                    layerUint.Add(tiles[num4, tileLoop].modifier);
                                }
                            }
                            num4++;
                        }
                        tileLoop++;
                    }
                    if (list3.Count > 0)
                    {
                        TileGroup item = new TileGroup
                        {
                            tiles = list3,
                            x = j * 8,
                            y = i * 8,
                            originX = 0,
                            originY = 0,
                            depth = layerDepth,
                            width = 320,
                            height = 180
                        };
                        tileGroup.Add(item);
                    }
                }
            }
            return tileGroup;
        }
        private TileGroup FloodFillGroupGet(ref TileGrouper.TileData[,] tiles, int x, int y, int groupid)
        {
            List<TileGrouper.TileData> list = new List<TileGrouper.TileData>();
            FloodFillStep(ref tiles, x, y, groupid, ref list);
            int xMax = int.MinValue;
            int xMaximum = int.MaxValue;
            int yMax = int.MinValue;
            int yMaximum = int.MaxValue;
            foreach (TileGrouper.TileData tileData in list)
            {
                if (tileData.tile.X + 1 > xMax)
                {
                    xMax = tileData.tile.X + 1;
                }
                if (tileData.tile.Y + 1 > yMax)
                {
                    yMax = tileData.tile.Y + 1;
                }
                if (tileData.tile.X < xMaximum)
                {
                    xMaximum = tileData.tile.X;
                }
                if (tileData.tile.Y < yMaximum)
                {
                    yMaximum = tileData.tile.Y;
                }
            }
            int num5 = xMaximum * 8;
            int num6 = yMaximum * 8;
            int num7 = xMax * 8;
            int num8 = yMax * 8;
            int num9 = num7 - num5;
            int num10 = num8 - num6;
            offsets.TryGetValue(groupid, out Point point);
            return new TileGroup
            {
                tiles = list,
                x = num5,
                y = num6,
                originX = num9 / 2 + point.X,
                originY = num10 - 4 + point.Y,
                depth = num8,
                width = num9,
                height = num10
            };
        }
        private void FloodFillStep(ref TileGrouper.TileData[,] tiles, int x, int y, int groupid, ref List<TileGrouper.TileData> group)
        {
            if (PointInBounds(x, y) && tiles[x, y].groupid == groupid && !tiles[x, y].ignore)
            {
                TileGrouper.TileData item = tiles[x, y];
                bool flag = !item.left;
                bool flag2 = !item.top;
                bool flag3 = !item.right;
                bool flag4 = !item.bottom;
                group.Add(item);
                tiles[x, y].ignore = true;
                if (flag)
                {
                    FloodFillStep(ref tiles, x - 1, y, groupid, ref group);
                }
                if (flag2)
                {
                    FloodFillStep(ref tiles, x, y - 1, groupid, ref group);
                }
                if (flag3)
                {
                    FloodFillStep(ref tiles, x + 1, y, groupid, ref group);
                }
                if (flag4)
                {
                    FloodFillStep(ref tiles, x, y + 1, groupid, ref group);
                }
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
            public bool tree;
        }
    }
}
