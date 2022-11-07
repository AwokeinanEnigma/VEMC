using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using VEMC.Parts;
using ClipperLib;
using fNbt;
using TiledSharp;

namespace VEMC
{
	// Token: 0x02000041 RID: 65
	public class MapJob
	{
		// Token: 0x1700007A RID: 122
		// (get) Token: 0x0600024D RID: 589 RVA: 0x0000A974 File Offset: 0x00008B74
		// (set) Token: 0x0600024E RID: 590 RVA: 0x0000A97C File Offset: 0x00008B7C
		public int Mode { get; private set; }

		// Token: 0x0600024F RID: 591 RVA: 0x0000A988 File Offset: 0x00008B88
		public MapJob()
		{
			this.Mode = 0;
		}

		// Token: 0x06000250 RID: 592 RVA: 0x0000B041 File Offset: 0x00009241
		public void Open(string filename)
		{
			this.map = new TmxMap(filename);
		}

		// Token: 0x06000251 RID: 593 RVA: 0x0000B04F File Offset: 0x0000924F
		public void CheckProperty(string property)
		{
			if (!this.map.Properties.ContainsKey(property))
			{
				throw new MapPropertyException(property);
			}
		}

		// Token: 0x06000252 RID: 594 RVA: 0x0000B06B File Offset: 0x0000926B
		public void ValidateMap()
		{
			this.CheckProperty("name");
			this.CheckProperty("title");
			this.CheckProperty("subtitle");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Map passed first few checks.");
			Console.ResetColor();

			if (map.Tilesets.Count > 1)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("HEAR YE HEAR YE");
				Console.WriteLine("THIS MAP HAS MORE THAN ONE TILESET");
				Console.WriteLine("THIS IS NOT ALLOWED");
				throw new Exception("Map had more than two tilesets.");

			}

		}

		// Token: 0x06000253 RID: 595 RVA: 0x0000B0FC File Offset: 0x000092FC
		public void Process()
		{
			this.ValidateMap();
			void AddToHeader(MapPart header)
			{
				header.AddFromDictionary<string>("title", this.map.Properties, "title");
				header.AddFromDictionary<string>("subtitle", this.map.Properties, "subtitle");
				header.AddFromDictionary<string>("script", this.map.Properties, "script", string.Empty);
				header.AddFromDictionary<string>("bbg", this.map.Properties, "bbg", string.Empty);
				header.AddFromDictionary<bool>("shdw", this.map.Properties, "shadows", true);
				header.AddFromDictionary<bool>("ocn", this.map.Properties, "ocean", false);
				header.Add("color", (int)Utility.TmxColorToInt(this.map.BackgroundColor));
			}

			void CreateTilesetDatFiles(Dictionary<string, OptimizedTileset> tilesetDict, List<NbtCompound> compound)
			{
				foreach (TmxTileset tmxTileset in this.map.Tilesets)
				{
					if (tmxTileset.Image != null)
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tmxTileset.Image.Source);
						int firstGid = tmxTileset.FirstGid;
						OptimizedTileset optimizedTileset = TilesetOptimizer.Optimize(tmxTileset);
						tilesetDict.Add(tmxTileset.Name, optimizedTileset);
						compound.Add(new NbtCompound
						{
							new NbtString("ts", fileNameWithoutExtension),
							new NbtInt("tid", firstGid)
						});
						TmxColor trans = tmxTileset.Image.Trans;
						Color.FromArgb(trans.R, trans.G, trans.B);
						NbtFile nbtFile2 = TilesetDatBuilder.Build(tmxTileset, optimizedTileset);
						string fileName = Utility.AppDirectory + "\\Data\\Graphics\\MapTilesets\\" + fileNameWithoutExtension + ".mtdat";
						nbtFile2.SaveToFile(fileName, NbtCompression.GZip);
					}
				}
			}

			Utility.Hash(this.map.Properties["name"]);
			Dictionary<string, List<TmxObjectGroup.TmxObject>> objectsByType = this.GetObjectsByType(this.map.ObjectGroups);
			NbtFile nbtFile = new NbtFile();
			NbtCompound nbtCompound = new NbtCompound("map");
			nbtFile.RootTag = nbtCompound;
			this.Mode++;
			// Console.WriteLine("1");
			MapPart mapPart = new MapPart("head");
			AddToHeader(mapPart);
			string text = null;
			if (this.map.Properties.TryGetValue("nightColor", out text))
			{
				mapPart.Add("nColor", int.Parse("FF" + text.Substring(0, Math.Min(6, text.Length)), NumberStyles.HexNumber));
			}
			mapPart.Add("width", this.map.Width * this.map.TileWidth);
			mapPart.Add("height", this.map.Height * this.map.TileHeight);
			List<NbtCompound> list = new List<NbtCompound>();
			Dictionary<string, OptimizedTileset> dictionary = new Dictionary<string, OptimizedTileset>();

			CreateTilesetDatFiles(dictionary, list);

			NbtList tag = new NbtList("tilesets", list, NbtTagType.Compound);
			mapPart.Add(tag);
			nbtCompound.Add(mapPart.Tag);
			this.Mode++;
			this.Mode++;
			MapPart mapPart2 = new MapPart("audbgm", true);
			List<TmxObjectGroup.TmxObject> list2 = null;
			objectsByType.TryGetValue("bgm", out list2);
			int num = 0;
			Console.WriteLine("4");
			if (list2 != null)
			{
				foreach (TmxObjectGroup.TmxObject tmxObject in list2)
				{
					MapPart mapPart3 = new MapPart(false);
					mapPart3.Add("x", tmxObject.X);
					mapPart3.Add("y", tmxObject.Y);
					mapPart3.Add("w", tmxObject.Width);
					mapPart3.Add("h", tmxObject.Height);
					mapPart3.AddFromDictionary<short>("flag", tmxObject.Properties, "playflag", 0);
					mapPart3.AddFromDictionary<bool>("loop", tmxObject.Properties, "loop", true);
					mapPart3.AddFromDictionary<string>("bgm", tmxObject.Properties, "bgm");
					mapPart2.Add(mapPart3);
					num++;
				}
			}
			Console.WriteLine("5");
			if (mapPart2.Tags.Count > 0)
			{
				nbtCompound.Add(mapPart2.Tag);
			}
			this.Mode++;
			Console.WriteLine("6");
			MapPart mapPart4 = new MapPart("audsfx", true);
			List<TmxObjectGroup.TmxObject> list3 = null;
			objectsByType.TryGetValue("sfx", out list3);
			int num2 = 0;
			Console.WriteLine("25");
			if (list3 != null)
			{
				foreach (TmxObjectGroup.TmxObject tmxObject2 in list3)
				{
					MapPart mapPart5 = new MapPart(false);
					mapPart5.Add("x", tmxObject2.X);
					mapPart5.Add("y", tmxObject2.Y);
					mapPart5.Add("w", tmxObject2.Width);
					mapPart5.Add("h", tmxObject2.Height);
					mapPart5.AddFromDictionary<short>("flag", tmxObject2.Properties, "playFlag", 0);
					mapPart5.AddFromDictionary<short>("interval", tmxObject2.Properties, "interval", 0);
					mapPart5.AddFromDictionary<bool>("loop", tmxObject2.Properties, "loop", true);
					mapPart5.AddFromDictionary<string>("sfx", tmxObject2.Properties, "sfx");
					mapPart4.Add(mapPart5);
					num2++;
				}
			}
			Console.WriteLine("62");
			if (mapPart4.Tags.Count > 0)
			{
				nbtCompound.Add(mapPart4.Tag);
			}
			this.Mode++;
			MapPart mapPart6 = new MapPart("doors", true);
			List<TmxObjectGroup.TmxObject> list4 = null;
			objectsByType.TryGetValue("door", out list4);
			int num3 = 0;
			if (list4 != null)
			{
				foreach (TmxObjectGroup.TmxObject tmxObject3 in list4)
				{
					MapPart mapPart7 = new MapPart(false);
					mapPart7.Add("x", tmxObject3.X);
					mapPart7.Add("y", tmxObject3.Y);
					mapPart7.Add("w", tmxObject3.Width);
					mapPart7.Add("h", tmxObject3.Height);
					mapPart7.AddFromDictionary<int>("xto", tmxObject3.Properties, "xto");
					mapPart7.AddFromDictionary<int>("yto", tmxObject3.Properties, "yto");
					mapPart7.AddFromDictionary<string>("map", tmxObject3.Properties, "map");
					mapPart7.AddFromDictionary<int>("sfx", tmxObject3.Properties, "sfx", 0);
					mapPart7.AddFromDictionary<short>("flag", tmxObject3.Properties, "flag", 0);
					mapPart6.Add(mapPart7);
					num3++;
				}
			}
			if (mapPart6.Tags.Count > 0)
			{
				nbtCompound.Add(mapPart6.Tag);
			}
			this.Mode++;
			MapPart mapPart8 = new MapPart("triggers", true);
			List<TmxObjectGroup.TmxObject> list5 = null;
			objectsByType.TryGetValue("trigger area", out list5);
			int num4 = 0;
			if (list5 != null)
			{
				foreach (TmxObjectGroup.TmxObject tmxObject4 in list5)
				{
					MapPart mapPart9 = new MapPart(false);
					NbtList nbtList = new NbtList("coords", NbtTagType.Int);
					if (tmxObject4.ObjectType == TmxObjectGroup.TmxObjectType.Polygon)
					{
						using (List<Tuple<int, int>>.Enumerator enumerator3 = tmxObject4.Points.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								Tuple<int, int> tuple = enumerator3.Current;
								nbtList.Add(new NbtInt(tuple.Item1));
								nbtList.Add(new NbtInt(tuple.Item2));
							}
							goto IL_88B;
						}
						goto IL_7FF;
					}
					goto IL_7FF;
				IL_88B:
					mapPart9.Add(nbtList);
					mapPart9.AddFromDictionary<string>("scr", tmxObject4.Properties, "script");
					mapPart9.AddFromDictionary<short>("flag", tmxObject4.Properties, "flag", 0);
					mapPart9.Add("x", tmxObject4.X);
					mapPart9.Add("y", tmxObject4.Y);
					mapPart8.Add(mapPart9);
					num4++;
					continue;
				IL_7FF:
					if (tmxObject4.ObjectType == TmxObjectGroup.TmxObjectType.Basic)
					{
						nbtList.Add(new NbtInt(0));
						nbtList.Add(new NbtInt(0));
						nbtList.Add(new NbtInt(tmxObject4.Width));
						nbtList.Add(new NbtInt(0));
						nbtList.Add(new NbtInt(tmxObject4.Width));
						nbtList.Add(new NbtInt(tmxObject4.Height));
						nbtList.Add(new NbtInt(0));
						nbtList.Add(new NbtInt(tmxObject4.Height));
						goto IL_88B;
					}
					goto IL_88B;
				}
			}
			if (mapPart8.Tags.Count > 0)
			{
				nbtCompound.Add(mapPart8.Tag);
			}
			this.Mode++;
			MapPart mapPart10 = new MapPart("npcs", true);
			List<TmxObjectGroup.TmxObject> list6 = null;
			objectsByType.TryGetValue("npc", out list6);
			int num5 = 0;
			if (list6 != null)
			{
				foreach (TmxObjectGroup.TmxObject tmxObject5 in list6)
				{
					MapPart mapPart11 = new MapPart(false);
					string text2;
					tmxObject5.Properties.TryGetValue("sprite", out text2);
					if (text2 != null && text2.Trim().Length > 0)
					{
						mapPart11.Add("x", tmxObject5.X + tmxObject5.Width / 2);
						mapPart11.Add("y", tmxObject5.Y + tmxObject5.Height / 2);
						mapPart11.AddFromDictionary<string>("spr", tmxObject5.Properties, "sprite", string.Empty);
					}
					else
					{
						mapPart11.Add("x", tmxObject5.X);
						mapPart11.Add("y", tmxObject5.Y);
						mapPart11.Add("w", tmxObject5.Width);
						mapPart11.Add("h", tmxObject5.Height);
					}
					mapPart11.Add("name", tmxObject5.Name);
					mapPart11.AddFromDictionary<byte>("dir", tmxObject5.Properties, "direction", 6);
					mapPart11.AddFromDictionary<byte>("mov", tmxObject5.Properties, "movement", 0);
					mapPart11.AddFromDictionary<float>("spd", tmxObject5.Properties, "speed", 1f);
					mapPart11.AddFromDictionary<short>("dst", tmxObject5.Properties, "distance", 20);
					mapPart11.AddFromDictionary<short>("dly", tmxObject5.Properties, "delay", 0);
					mapPart11.AddFromDictionary<string>("cnstr", tmxObject5.Properties, "constraint", string.Empty);
					mapPart11.AddFromDictionary<bool>("cls", tmxObject5.Properties, "collisions", true);
					mapPart11.AddFromDictionary<bool>("en", tmxObject5.Properties, "enabled", true);
					mapPart11.AddFromDictionary<short>("flag", tmxObject5.Properties, "flag", 0);
					mapPart11.AddFromDictionary<bool>("shdw", tmxObject5.Properties, "shadow", true);
					mapPart11.AddFromDictionary<bool>("stky", tmxObject5.Properties, "sticky", false);
					mapPart11.AddFromDictionary<int>("dpth", tmxObject5.Properties, "depth", int.MinValue);
					List<NbtTag> list7 = new List<NbtTag>();
					int num6 = 0;
					string text3;
					while (tmxObject5.Properties.TryGetValue("text" + num6, out text3))
					{
						string[] array = text3.Split(new char[]
						{
							','
						});
						if (array.Length >= 2)
						{
							list7.Add(new NbtString(string.Format("t{0}", num6), array[0]));
							list7.Add(new NbtShort(string.Format("f{0}", num6), short.Parse(array[1])));
						}
						num6++;
					}
					NbtCompound nbtCompound2 = new NbtCompound("entries");
					nbtCompound2.AddRange(list7);
					mapPart11.Add(nbtCompound2);
					List<NbtTag> list8 = new List<NbtTag>();
					int num7 = 0;
					while (tmxObject5.Properties.TryGetValue("tele" + num7, out text3))
					{
						string[] array2 = text3.Split(new char[]
						{
							','
						});
						if (array2.Length >= 2)
						{
							list8.Add(new NbtString(string.Format("t{0}", num7), array2[0]));
							list8.Add(new NbtShort(string.Format("f{0}", num7), short.Parse(array2[1])));
						}
						num7++;
					}
					NbtCompound nbtCompound3 = new NbtCompound("tele");
					nbtCompound3.AddRange(list8);
					mapPart11.Add(nbtCompound3);
					mapPart10.Add(mapPart11);
					num5++;
				}
			}
			if (mapPart10.Tags.Count > 0)
			{
				nbtCompound.Add(mapPart10.Tag);
			}
			this.Mode++;
			MapPart mapPart12 = new MapPart("paths", true);
			List<TmxObjectGroup.TmxObject> list9 = null;
			objectsByType.TryGetValue("npc path", out list9);
			int num8 = 0;
			if (list9 != null)
			{
				foreach (TmxObjectGroup.TmxObject tmxObject6 in list9)
				{
					MapPart mapPart13 = new MapPart(false);
					mapPart13.Add("name", tmxObject6.Name);
					List<NbtInt> list10 = new List<NbtInt>();
					int num9 = 0;
					if (tmxObject6.Points != null)
					{
						using (List<Tuple<int, int>>.Enumerator enumerator3 = tmxObject6.Points.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								Tuple<int, int> tuple2 = enumerator3.Current;
								list10.Add(new NbtInt(tmxObject6.X + tuple2.Item1));
								list10.Add(new NbtInt(tmxObject6.Y + tuple2.Item2));
								num9++;
							}
							goto IL_E84;
						}
						goto IL_E6D;
					IL_E84:
						NbtList tag2 = new NbtList("coords", list10, NbtTagType.Int);
						mapPart13.Add(tag2);
						mapPart12.Add(mapPart13);
						num8++;
						continue;
					}
				IL_E6D:
					throw new Exception(string.Format("NPC Path does not contain points. Make sure you set \"{0}\" to the right type.", tmxObject6.Name));
				}
			}
			if (mapPart12.Tags.Count > 0)
			{
				nbtCompound.Add(mapPart12.Tag);
			}
			this.Mode++;
			MapPart mapPart14 = new MapPart("areas", true);
			List<TmxObjectGroup.TmxObject> list11 = null;
			objectsByType.TryGetValue("npc area", out list11);
			int num10 = 0;
			if (list11 != null)
			{
				foreach (TmxObjectGroup.TmxObject tmxObject7 in list11)
				{
					MapPart mapPart15 = new MapPart(false);
					mapPart15.Add("name", tmxObject7.Name);
					mapPart15.Add("x", tmxObject7.X);
					mapPart15.Add("y", tmxObject7.Y);
					mapPart15.Add("w", tmxObject7.Width);
					mapPart15.Add("h", tmxObject7.Height);
					mapPart14.Add(mapPart15);
					num10++;
				}
			}
			if (mapPart14.Tags.Count > 0)
			{
				nbtCompound.Add(mapPart14.Tag);
			}
			this.Mode++;
			MapPart mapPart16 = new MapPart("crowds", true);
			List<TmxObjectGroup.TmxObject> list12 = null;
			objectsByType.TryGetValue("crowd path", out list12);
			int num11 = 0;
			if (list12 != null)
			{
				foreach (TmxObjectGroup.TmxObject tmxObject8 in list12)
				{
					MapPart mapPart17 = new MapPart(false);
					mapPart16.Add(mapPart17);
					mapPart17.AddFromDictionary<int>("mode", tmxObject8.Properties, "mode");
					List<NbtShort> list13 = new List<NbtShort>();
					string text4 = null;
					tmxObject8.Properties.TryGetValue("sprites", out text4);
					if (text4 != null)
					{
						string[] array3 = text4.Split(new char[]
						{
							','
						});
						int num12 = 0;
						foreach (string s in array3)
						{
							list13.Add(new NbtShort(short.Parse(s)));
							num12++;
						}
						NbtList tag3 = new NbtList("sprs", list13, NbtTagType.Short);
						mapPart17.Add(tag3);
						List<NbtInt> list14 = new List<NbtInt>();
						int num13 = 0;
						if (tmxObject8.Points != null)
						{
							using (List<Tuple<int, int>>.Enumerator enumerator3 = tmxObject8.Points.GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									Tuple<int, int> tuple3 = enumerator3.Current;
									list14.Add(new NbtInt(tuple3.Item1));
									list14.Add(new NbtInt(tuple3.Item2));
									num13++;
								}
								goto IL_1191;
							}
							goto IL_117A;
						IL_1191:
							NbtList tag4 = new NbtList("coords", list14, NbtTagType.Int);
							mapPart17.Add(tag4);
							num11++;
							continue;
						}
					IL_117A:
						throw new Exception(string.Format("Crowd Path does not contain points. Make sure you set \"{0}\" to the right type.", tmxObject8.Name));
					}
					throw new MapPartRequirementException(tmxObject8.Name, "sprites");
				}
			}
			if (mapPart16.Tags.Count > 0)
			{
				nbtCompound.Add(mapPart16.Tag);
			}
			this.Mode++;
			MapPart mapPart18 = new MapPart("spawns", true);
			List<TmxObjectGroup.TmxObject> list15 = null;
			objectsByType.TryGetValue("enemy spawn", out list15);
			int num14 = 0;
			if (list15 != null)
			{
				foreach (TmxObjectGroup.TmxObject tmxObject9 in list15)
				{

					NbtCompound nbtCompound4 = new NbtCompound();
					mapPart18.Add(nbtCompound4);
					Console.WriteLine($"3");
					nbtCompound4.Add(new NbtInt("x", tmxObject9.X));
					nbtCompound4.Add(new NbtInt("y", tmxObject9.Y));
					nbtCompound4.Add(new NbtInt("w", tmxObject9.Width));
					nbtCompound4.Add(new NbtInt("h", tmxObject9.Height));
					string s2 = "0";
					tmxObject9.Properties.TryGetValue("flag", out s2);
					short value = short.Parse(s2);
					nbtCompound4.Add(new NbtShort("flag", value));
					List<NbtString> list16 = new List<NbtString>();
					List<NbtByte> list17 = new List<NbtByte>();
					string text5;
					int num15 = 0;




					/*foreach (KeyValuePair<string, string> entry in tmxObject9.Properties)
                    {
                        // do something with entry.Value or entry.Key
                    }*/

					/*while (tmxObject9.Properties.TryGetValue("enemy" + num15, out text5))
					{
						Console.WriteLine($"counter {num15}");

                        num15++;

					}*/

					IEnumerable<KeyValuePair<string, string>> thing = tmxObject9.Properties.Where(x => x.Key.Contains("enemy"));
					foreach (KeyValuePair<string, string> strung in thing)
					{
						Console.WriteLine($"strung, {strung.Key}, {strung.Value}");
						string[] array5 = strung.Value.Split(new char[]
						{
							','
						});
						Console.WriteLine($"Enemy: {array5[0]}");
						list16.Add(new NbtString(array5[0]));
						//Console.WriteLine($"added string");
						list17.Add(new NbtByte(byte.Parse(array5[1])));
					}
					NbtList newTag = new NbtList("enids", list16, NbtTagType.String);
					NbtList newTag2 = new NbtList("enfreqs", list17, NbtTagType.Byte);
					nbtCompound4.Add(newTag);
					nbtCompound4.Add(newTag2);
					num14++;
				}
			}
			if (mapPart18.Tags.Count > 0)
			{
				nbtCompound.Add(mapPart18.Tag);
			}
			this.Mode++;
			MapPart mapPart19 = new MapPart("parallax", true);
			List<TmxObjectGroup.TmxObject> list18 = null;
			objectsByType.TryGetValue("parallax", out list18);
			int num16 = 0;
			if (list18 != null)
			{
				foreach (TmxObjectGroup.TmxObject tmxObject10 in list18)
				{
					MapPart mapPart20 = new MapPart(false);
					mapPart19.Add(mapPart20);
					mapPart20.AddFromDictionary<string>("spr", tmxObject10.Properties, "sprite");
					mapPart20.AddFromDictionary<float>("vx", tmxObject10.Properties, "vx", 1f);
					mapPart20.AddFromDictionary<float>("vy", tmxObject10.Properties, "vy", 1f);
					mapPart20.Add(new NbtFloat("x", (float)tmxObject10.X));
					mapPart20.Add(new NbtFloat("y", (float)tmxObject10.Y));
					mapPart20.Add(new NbtFloat("w", (float)tmxObject10.Width));
					mapPart20.Add(new NbtFloat("h", (float)tmxObject10.Height));
					num16++;
				}
			}
			if (mapPart19.Tags.Count > 0)
			{
				nbtCompound.Add(mapPart19.Tag);
			}
			this.Mode++;
			NbtList nbtList2 = new NbtList("mesh", NbtTagType.List);
			List<List<IntPoint>> list19 = this.BuildMesh();
			int num17 = 0;
			foreach (List<IntPoint> list20 in list19)
			{
				List<NbtInt> list21 = new List<NbtInt>();
				foreach (IntPoint intPoint in list20)
				{
					list21.Add(new NbtInt((int)intPoint.X));
					list21.Add(new NbtInt((int)intPoint.Y));
				}
				NbtList newTag3 = new NbtList(list21, NbtTagType.Int);
				nbtList2.Add(newTag3);
				num17++;
			}
			if (nbtList2.Count > 0)
			{
				nbtCompound.Add(nbtList2);
			}
			this.Mode++;
			NbtList nbtList3 = new NbtList("tiles", NbtTagType.Compound);
			TileGrouper tileGrouper = new TileGrouper(this.map);
			int num18 = 0;
			List<TileGroup> list22 = tileGrouper.FindGroups();
			foreach (TileGroup tileGroup in list22)
			{
				MapPart mapPart21 = new MapPart(false);
				mapPart21.Add(new NbtInt("depth", tileGroup.depth));
				mapPart21.Add(new NbtInt("x", tileGroup.x));
				mapPart21.Add(new NbtInt("y", tileGroup.y));
				mapPart21.Add(new NbtInt("ox", tileGroup.originX));
				mapPart21.Add(new NbtInt("rainaway", tileGroup.depth));
				mapPart21.Add(new NbtInt("w", tileGroup.width / this.map.TileWidth));
				int num19 = tileGroup.width / this.map.TileWidth;
				int num20 = tileGroup.height / this.map.TileHeight;
				int groupTileX = tileGroup.x / this.map.TileWidth;
				int groupTileY = tileGroup.y / this.map.TileHeight;
				uint[] array6 = new uint[num19 * num20];
				bool flag = true;
				OptimizedTileset optimizedTileset2 = dictionary[this.map.Tilesets[0].Name];
				int y;
				for (y = 0; y < num20; y++)
				{
					int x;
					for (x = 0; x < num19; x++)
					{
						TileGrouper.TileData tileData = tileGroup.tiles.Find((TileGrouper.TileData t) => t.tile.X == x + groupTileX && t.tile.Y == y + groupTileY);
						int num21 = y * num19 + x;
						if (tileData.tile != null)
						{
							string _false;
							if (this.map.Tilesets[0].GetTileById(tileData.tile.Gid - 1) != null)
							{
								if (this.map.Tilesets[0].GetTileById(tileData.tile.Gid - 1).Properties != null)
								{
									if (this.map.Tilesets[0].GetTileById(tileData.tile.Gid - 1).Properties.TryGetValue("rainaway", out _false)) {

										Console.WriteLine("rainaway");
									}
								}
							}

							if (tileData.tile.Gid > optimizedTileset2.TranslationTable.Count)
							{
								Console.WriteLine($"GID '{tileData.tile.Gid}' at tile '{tileData.tile.X},{tileData.tile.Y}' when the maximum is {optimizedTileset2.TranslationTable.Count}");
								continue;
							}

							uint num22 = (uint)(optimizedTileset2.Translate(tileData.tile.Gid));
							array6[num21] = (tileData.modifier << 16 | num22);
						}
						else
						{
							array6[num21] = 0U;
						}
						flag = (flag && array6[num21] == 0U);
					}
				}
				if (!flag)
				{
					byte[] array7 = new byte[array6.Length * 4];
					Buffer.BlockCopy(array6, 0, array7, 0, array7.Length);
					mapPart21.Add("tiles", array7);
					nbtList3.Add(mapPart21.Tag);
					num18++;
				}
			}
			if (nbtList3.Count > 0)
			{
				nbtCompound.Add(nbtList3);
			}
			this.Mode++;
			Utility.ConsoleWrite("Saving...                             ", new object[0]);
			string fileName2 = string.Format("{0}\\Data\\Maps\\{1}.mdat", Utility.AppDirectory, this.map.Properties["name"]);
			nbtFile.SaveToFile(fileName2, NbtCompression.GZip);
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0000CBD4 File Offset: 0x0000ADD4
		public List<List<IntPoint>> BuildMesh()
		{
			int height = this.map.Height;
			int width = this.map.Width;
			TmxTileset tileset = this.map.Tilesets[0];
			int num = width * height * this.map.Layers.Count;
			int num2 = 0;
			MeshBuilder meshBuilder = new MeshBuilder();
			string text = null;
			for (int i = this.map.Layers.Count - 1; i >= 0; i--)
			{
				string name = this.map.Layers[i].Name;
				int count = this.map.Layers[i].Tiles.Count;
				for (int j = 0; j < count; j++)
				{
					TmxLayerTile tmxLayerTile = this.map.Layers[i].Tiles[j];
					int gid = tmxLayerTile.Gid - 1;
					int x = tmxLayerTile.X;
					int y = tmxLayerTile.Y;
					TmxTilesetTile tileById = tileset.GetTileById(gid);
					if (tileById != null)
					{
						int num3 = 0;
						tileById.Properties.TryGetValue("solidity", out text);
						if (text != null)
						{
							num3 = int.Parse(text);
						}
						if (num3 > 0)
						{
							meshBuilder.AddPath(x * this.map.TileWidth, y * this.map.TileHeight, this.collisionMasks[num3]);
						}
					}
					num2++;
					if (num2 % 100 == 0)
					{
						Utility.ConsoleWrite("Building collision mesh from \"{0}\" ({1}%)", new object[]
						{
							name,
							Math.Round((double)num2 / (double)num * 100.0)
						});
					}
				}
			}
			Utility.ConsoleWrite("Simplifying collision mesh...                                     ", new object[0]);
			meshBuilder.Simplify();
			return meshBuilder.Solution;
		}

		// Token: 0x06000255 RID: 597 RVA: 0x0000CDAC File Offset: 0x0000AFAC
		private uint CheckGraphicsManifest(string filename)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
			string path = Utility.AppDirectory + "\\Resources\\Graphics\\manifest.txt";
			FileStream fileStream = new FileStream(path, FileMode.Open);
			StreamReader streamReader = new StreamReader(fileStream);
			int num = -1;
			int num2 = 0;
			while (!streamReader.EndOfStream)
			{
				string a = streamReader.ReadLine();
				if (a == fileNameWithoutExtension)
				{
					num = num2;
					break;
				}
				num2++;
			}
			fileStream.Close();
			if (num == -1)
			{
				fileStream = new FileStream(path, FileMode.Append);
				StreamWriter streamWriter = new StreamWriter(fileStream);
				streamWriter.WriteLine(fileNameWithoutExtension);
				streamWriter.Close();
				num = File.ReadLines(path).Count<string>() - 1;
				Console.WriteLine(filename + " was added to the graphics manifest.");
			}
			return (uint)num;
		}

		// Token: 0x06000256 RID: 598 RVA: 0x0000CE58 File Offset: 0x0000B058
		private Dictionary<string, List<TmxObjectGroup.TmxObject>> GetObjectsByType(TmxList<TmxObjectGroup> groupList)
		{
			Dictionary<string, List<TmxObjectGroup.TmxObject>> dictionary = new Dictionary<string, List<TmxObjectGroup.TmxObject>>();
			foreach (TmxObjectGroup tmxObjectGroup in groupList)
			{
				foreach (TmxObjectGroup.TmxObject tmxObject in tmxObjectGroup.Objects)
				{
					string key = tmxObject.Type.ToLower();
					List<TmxObjectGroup.TmxObject> list;
					if (dictionary.ContainsKey(key))
					{
						list = dictionary[key];
					}
					else
					{
						list = new List<TmxObjectGroup.TmxObject>();
						dictionary.Add(key, list);
					}
					list.Add(tmxObject);
				}
			}
			return dictionary;
		}

		// Token: 0x06000257 RID: 599 RVA: 0x0000CF18 File Offset: 0x0000B118
		private TmxObjectGroup ObjectGroupByName(TmxList<TmxObjectGroup> list, string name)
		{
			foreach (TmxObjectGroup tmxObjectGroup in list)
			{
				if (tmxObjectGroup.Name.ToLower() == name.ToLower())
				{
					return tmxObjectGroup;
				}
			}
			throw new Exception("Object group with name \"" + name + "\" does not exist.");
		}

		// Token: 0x06000258 RID: 600 RVA: 0x0000CF8C File Offset: 0x0000B18C
		private bool ObjectGroupExists(TmxList<TmxObjectGroup> list, string name)
		{
			foreach (TmxObjectGroup tmxObjectGroup in list)
			{
				if (tmxObjectGroup.Name.ToLower() == name.ToLower())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x040000CE RID: 206
		private TmxMap map;

		// Token: 0x040000CF RID: 207
		private List<Tuple<string, byte>> effectDict = new List<Tuple<string, byte>>
		{
			new Tuple<string, byte>("none", 0),
			new Tuple<string, byte>("rain", 1),
			new Tuple<string, byte>("storm", 2),
			new Tuple<string, byte>("snow", 3),
			new Tuple<string, byte>("underwater", 4),
			new Tuple<string, byte>("lighting", 5)
		};

		// Token: 0x040000D0 RID: 208
		private Dictionary<int, Point[]> collisionMasks = new Dictionary<int, Point[]>
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
	}
}
