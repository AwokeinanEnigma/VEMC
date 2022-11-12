using System;
using System.Collections.Generic;
using System.Drawing;


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
            TiledTilesetTile tileById = tileset.GetTileById(tile.Gid - 1);
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
            List<TileGroup> list = new List<TileGroup>();
            TileGrouper.TileData[,] array = new TileGrouper.TileData[map.Width, map.Height];
            int num = 0;
            int count = tiles.Count;
            foreach (TmxLayerTile tmxLayerTile in tiles)
            {
                int num2 = tmxLayerTile.Gid - tileset.FirstGid;
                TiledTilesetTile tileById = tileset.GetTileById(num2);
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
                        array[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                        {
                            tile = tmxLayerTile,
                            groupid = int.Parse(tileById.Properties["group"]),
                            left = (array2[0] == "1"),
                            top = (array2[1] == "1"),
                            right = (array2[2] == "1"),
                            bottom = (array2[3] == "1"),
                            modifier = tileModifier
                        };
                    }
                    else
                    {
                        array[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                        {
                            tile = tmxLayerTile,
                            groupid = -1,
                            modifier = tileModifier
                        };
                    }
                }
                else if (num2 == -1)
                {
                    array[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                    {
                        groupid = -3,
                        ignore = true,
                        modifier = tileModifier
                    };
                }
                else
                {
                    array[tmxLayerTile.X, tmxLayerTile.Y] = new TileGrouper.TileData
                    {
                        tile = tmxLayerTile,
                        groupid = -1,
                        modifier = tileModifier
                    };
                }
                if (num % 100 == 0)
                {
                    Utility.ConsoleWrite("Finding tile groups in \"{0}\"({1}%)", new object[]
                    {
                        layer.Name,
                        Math.Round(num / (double)count * 100.0)
                    });
                }
                num++;
            }
            for (int i = 0; i < map.Height; i++)
            {
                for (int j = 0; j < map.Width; j++)
                {
                    TileGrouper.TileData tileData = array[j, i];
                    if (!tileData.ignore && tileData.groupid > 0)
                    {
                        TileGroup item = FloodFillGroupGet(ref array, j, i, tileData.groupid);
                        list.Add(item);
                    }
                }
            }
            Utility.ConsoleWrite("Slicing tile layers...", new object[0]);
            List<TileGroup> collection = SliceTileLayer(ref array, layer.Properties.ContainsKey("depth") ? int.Parse(layer.Properties["depth"]) : 0);
            list.AddRange(collection);
            return list;
        }
        private List<TileGroup> SliceTileLayer(ref TileGrouper.TileData[,] tiles, int layerDepth)
        {
            List<TileGroup> list = new List<TileGroup>();
            List<uint> list2 = new List<uint>();
            int num = 40;
            int num2 = 22;
            for (int i = 0; i < map.Height; i += num2)
            {
                for (int j = 0; j < map.Width; j += num)
                {
                    List<TileGrouper.TileData> list3 = new List<TileGrouper.TileData>();
                    int num3 = i;
                    while (num3 < i + num2 && num3 < map.Height)
                    {
                        int num4 = j;
                        while (num4 < j + num && num4 < map.Width)
                        {
                            if (!tiles[num4, num3].ignore)
                            {
                                list3.Add(tiles[num4, num3]);
                                if (tiles[num4, num3].modifier > 0U)
                                {
                                    list2.Add(tiles[num4, num3].modifier);
                                }
                            }
                            num4++;
                        }
                        num3++;
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
                        list.Add(item);
                    }
                }
            }
            return list;
        }
        private TileGroup FloodFillGroupGet(ref TileGrouper.TileData[,] tiles, int x, int y, int groupid)
        {
            List<TileGrouper.TileData> list = new List<TileGrouper.TileData>();
            FloodFillStep(ref tiles, x, y, groupid, ref list);
            int num = int.MinValue;
            int num2 = int.MaxValue;
            int num3 = int.MinValue;
            int num4 = int.MaxValue;
            foreach (TileGrouper.TileData tileData in list)
            {
                if (tileData.tile.X + 1 > num)
                {
                    num = tileData.tile.X + 1;
                }
                if (tileData.tile.Y + 1 > num3)
                {
                    num3 = tileData.tile.Y + 1;
                }
                if (tileData.tile.X < num2)
                {
                    num2 = tileData.tile.X;
                }
                if (tileData.tile.Y < num4)
                {
                    num4 = tileData.tile.Y;
                }
            }
            int num5 = num2 * 8;
            int num6 = num4 * 8;
            int num7 = num * 8;
            int num8 = num3 * 8;
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
        private readonly TiledTileset tileset;
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
