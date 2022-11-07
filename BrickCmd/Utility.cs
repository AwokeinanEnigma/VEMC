using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using TiledSharp;

namespace VEMC
{
	// Token: 0x02000040 RID: 64
	internal static class Utility
	{
		// Token: 0x17000079 RID: 121
		// (get) Token: 0x06000243 RID: 579 RVA: 0x0000A734 File Offset: 0x00008934
		public static string AppDirectory
		{
			get
			{
				string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
				return directoryName.Substring(6);
			}
		}

		// Token: 0x06000244 RID: 580 RVA: 0x0000A760 File Offset: 0x00008960
		public static uint Hash(string val)
		{
			uint num = 5381U;
			int length = val.Length;
			foreach (char c in val)
			{
				num = (num * 33U ^ (uint)c);
			}
			return num;
		}

		// Token: 0x06000245 RID: 581 RVA: 0x0000A7A0 File Offset: 0x000089A0
		public static int Hash(byte[] bytes)
		{
			int num = -2128831035;
			for (int i = 0; i < bytes.Length; i++)
			{
				num = (num ^ (int)bytes[i]) * 16777619;
			}
			num += num << 13;
			num ^= num >> 7;
			num += num << 3;
			num ^= num >> 17;
			return num + (num << 5);
		}

		// Token: 0x06000246 RID: 582 RVA: 0x0000A7F0 File Offset: 0x000089F0
		public static uint HexColorToInt(string hex)
		{
			uint num = uint.Parse(hex, NumberStyles.HexNumber);
			return num | 4278190080U;
		}

		// Token: 0x06000247 RID: 583 RVA: 0x0000A814 File Offset: 0x00008A14
		public static uint TmxColorToInt(TmxColor col)
		{
			uint num = 255U;
			num <<= 8;
			num |= (uint)((byte)col.R);
			num <<= 8;
			num |= (uint)((byte)col.G);
			num <<= 8;
			return num | (uint)((byte)col.B);
		}

		// Token: 0x06000248 RID: 584 RVA: 0x0000A852 File Offset: 0x00008A52
		public static uint ColorToUInt(Color color)
		{
			return (uint)((int)color.A << 24 | (int)color.R << 16 | (int)color.G << 8 | (int)color.B);
		}

		// Token: 0x06000249 RID: 585 RVA: 0x0000A87C File Offset: 0x00008A7C
		public static Color UIntToColor(uint color)
		{
			byte alpha = (byte)(color >> 24);
			byte red = (byte)(color >> 16);
			byte green = (byte)(color >> 8);
			byte blue = (byte)color;
			return Color.FromArgb((int)alpha, (int)red, (int)green, (int)blue);
		}

		// Token: 0x0600024A RID: 586 RVA: 0x0000A8A8 File Offset: 0x00008AA8
		public static void ConsoleWrite(string text, params object[] args)
		{
			int count = Console.WindowWidth - text.Length % Console.WindowWidth;
			string format = text + new string(' ', count);
			int cursorTop = Console.CursorTop;
			Console.SetCursorPosition(0, cursorTop);
			Console.Write(format, args);
			Console.SetCursorPosition(0, cursorTop);
		}

		// Token: 0x0600024B RID: 587 RVA: 0x0000A8F4 File Offset: 0x00008AF4
		public static TmxTilesetTile GetTileById(this TmxTileset tileset, int gid)
		{
			TmxTilesetTile result = null;
			int count = tileset.Tiles.Count;
			int firstGid = tileset.FirstGid;
			for (int i = 0; i < count; i++)
			{
				if (tileset.Tiles[i].Id == gid)
				{
					result = tileset.Tiles[i];
					break;
				}
			}
			return result;
		}

		// Token: 0x0600024C RID: 588 RVA: 0x0000A948 File Offset: 0x00008B48
		public static decimal TryGetDecimal(this PropertyDict dict, string key)
		{
			string text = null;
			dict.TryGetValue(key, out text);
			return (text != null) ? decimal.Parse(text) : 0m;
		}
	}
}
