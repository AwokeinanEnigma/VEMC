using System;
using System.Collections.Generic;
using System.Drawing;
using TiledSharp;

namespace VEMC
{
	// Token: 0x02000039 RID: 57
	internal class TileGrouper
	{
		// Token: 0x0600022B RID: 555 RVA: 0x00009282 File Offset: 0x00007482
		public TileGrouper(TmxMap map)
		{
			this.map = map;
			this.tileset = map.Tilesets[0];
			this.offsets = new Dictionary<int, Point>();
			this.animations = new List<TileGrouper.TileAnimation>();
		}

		// Token: 0x0600022C RID: 556 RVA: 0x000092BC File Offset: 0x000074BC
		public List<TileGroup> FindGroups()
		{
			List<TileGroup> list = new List<TileGroup>();
			this.offsets = this.GetGroupOffsets();
			foreach (TmxLayer layer in this.map.Layers)
			{
				List<TileGroup> collection = this.FindGroupsInLayer(layer);
				list.AddRange(collection);
			}
			return list;
		}

		// Token: 0x0600022D RID: 557 RVA: 0x0000932C File Offset: 0x0000752C
		private Dictionary<int, Point> GetGroupOffsets()
		{
			Dictionary<int, Point> dictionary = new Dictionary<int, Point>();
			string text = null;
			this.map.Properties.TryGetValue("grouporigins", out text);
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
						int key = 0;
						int x = 0;
						int y = 0;
						int.TryParse(array2[0], out key);
						int.TryParse(array2[1], out x);
						int.TryParse(array2[2], out y);
						if (!dictionary.ContainsKey(key))
						{
							dictionary.Add(key, new Point(x, y));
						}
					}
				}
			}
			return dictionary;
		}

		// Token: 0x0600022E RID: 558 RVA: 0x000093F4 File Offset: 0x000075F4
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
			TmxTilesetTile tileById = this.tileset.GetTileById(tile.Gid - 1);
			if (tileById != null && tileById.Properties.ContainsKey("actualid"))
			{
				uint num2 = (uint)tileById.Properties.TryGetDecimal("actualid") + 1U;
				num |= num2 << 3;
			}
			return num;
		}

		// Token: 0x0600022F RID: 559 RVA: 0x00009470 File Offset: 0x00007670
		public List<TileGroup> FindGroupsInLayer(TmxLayer layer)
		{
			List<TmxLayerTile> tiles = layer.Tiles;
			List<TileGroup> list = new List<TileGroup>();
			TileGrouper.TileData[,] array = new TileGrouper.TileData[this.map.Width, this.map.Height];
			int num = 0;
			int count = tiles.Count;
			foreach (TmxLayerTile tmxLayerTile in tiles)
			{
				int num2 = tmxLayerTile.Gid - this.tileset.FirstGid;
				TmxTilesetTile tileById = this.tileset.GetTileById(num2);
				uint tileModifier = this.GetTileModifier(tmxLayerTile);
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
						Math.Round((double)num / (double)count * 100.0)
					});
				}
				num++;
			}
			for (int i = 0; i < this.map.Height; i++)
			{
				for (int j = 0; j < this.map.Width; j++)
				{
					TileGrouper.TileData tileData = array[j, i];
					if (!tileData.ignore && tileData.groupid > 0)
					{
						TileGroup item = this.FloodFillGroupGet(ref array, j, i, tileData.groupid);
						list.Add(item);
					}
				}
			}
			Utility.ConsoleWrite("Slicing tile layers...", new object[0]);
			List<TileGroup> collection = this.SliceTileLayer(ref array, layer.Properties.ContainsKey("depth") ? int.Parse(layer.Properties["depth"]) : 0);
			list.AddRange(collection);
			return list;
		}

		// Token: 0x06000230 RID: 560 RVA: 0x000097EC File Offset: 0x000079EC
		private List<TileGroup> SliceTileLayer(ref TileGrouper.TileData[,] tiles, int layerDepth)
		{
			List<TileGroup> list = new List<TileGroup>();
			List<uint> list2 = new List<uint>();
			int num = 40;
			int num2 = 22;
			for (int i = 0; i < this.map.Height; i += num2)
			{
				for (int j = 0; j < this.map.Width; j += num)
				{
					List<TileGrouper.TileData> list3 = new List<TileGrouper.TileData>();
					int num3 = i;
					while (num3 < i + num2 && num3 < this.map.Height)
					{
						int num4 = j;
						while (num4 < j + num && num4 < this.map.Width)
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

		// Token: 0x06000231 RID: 561 RVA: 0x00009960 File Offset: 0x00007B60
		private TileGroup FloodFillGroupGet(ref TileGrouper.TileData[,] tiles, int x, int y, int groupid)
		{
			List<TileGrouper.TileData> list = new List<TileGrouper.TileData>();
			this.FloodFillStep(ref tiles, x, y, groupid, ref list);
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
			Point point = default(Point);
			this.offsets.TryGetValue(groupid, out point);
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

		// Token: 0x06000232 RID: 562 RVA: 0x00009AFC File Offset: 0x00007CFC
		private void FloodFillStep(ref TileGrouper.TileData[,] tiles, int x, int y, int groupid, ref List<TileGrouper.TileData> group)
		{
			if (this.PointInBounds(x, y) && tiles[x, y].groupid == groupid && !tiles[x, y].ignore)
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
					this.FloodFillStep(ref tiles, x - 1, y, groupid, ref group);
				}
				if (flag2)
				{
					this.FloodFillStep(ref tiles, x, y - 1, groupid, ref group);
				}
				if (flag3)
				{
					this.FloodFillStep(ref tiles, x + 1, y, groupid, ref group);
				}
				if (flag4)
				{
					this.FloodFillStep(ref tiles, x, y + 1, groupid, ref group);
				}
				return;
			}
		}

		// Token: 0x06000233 RID: 563 RVA: 0x00009BD5 File Offset: 0x00007DD5
		private bool PointInBounds(int x, int y)
		{
			return x >= 0 && y >= 0 && x < this.map.Width && y < this.map.Height;
		}

		// Token: 0x040000B3 RID: 179
		private const int CHUNK_WIDTH = 320;

		// Token: 0x040000B4 RID: 180
		private const int CHUNK_HEIGHT = 180;

		// Token: 0x040000B5 RID: 181
		private const int TILE_WIDTH = 8;

		// Token: 0x040000B6 RID: 182
		private const int TILE_HEIGHT = 8;

		// Token: 0x040000B7 RID: 183
		private TmxMap map;

		// Token: 0x040000B8 RID: 184
		private TmxTileset tileset;

		// Token: 0x040000B9 RID: 185
		private Dictionary<int, Point> offsets;

		// Token: 0x040000BA RID: 186
		private List<TileGrouper.TileAnimation> animations;

		// Token: 0x0200003A RID: 58
		public struct TileAnimation
		{
			// Token: 0x040000BB RID: 187
			public ushort id;

			// Token: 0x040000BC RID: 188
			public float speed;

			// Token: 0x040000BD RID: 189
			public int[] indexes;
		}

		// Token: 0x0200003B RID: 59
		public struct TileData
		{
			// Token: 0x040000BE RID: 190
			public TmxLayerTile tile;

			// Token: 0x040000BF RID: 191
			public int groupid;

			// Token: 0x040000C0 RID: 192
			public bool left;

			// Token: 0x040000C1 RID: 193
			public bool top;

			// Token: 0x040000C2 RID: 194
			public bool right;

			// Token: 0x040000C3 RID: 195
			public bool bottom;

			// Token: 0x040000C4 RID: 196
			public bool ignore;

			// Token: 0x040000C5 RID: 197
			public uint modifier;
		}
	}
}
