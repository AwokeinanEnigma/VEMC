using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;


namespace VEMC
{
    internal static class Utility
    {
        public static string AppDirectory
        {
            get
            {
                string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                return directoryName.Substring(6);
            }
        }
        public static uint Hash(string val)
        {
            uint num = 5381U;
            int length = val.Length;
            foreach (char c in val)
            {
                num = (num * 33U ^ c);
            }
            return num;
        }
        public static int Hash(byte[] bytes)
        {
            int num = -2128831035;
            for (int i = 0; i < bytes.Length; i++)
            {
                num = (num ^ bytes[i]) * 16777619;
            }
            num += num << 13;
            num ^= num >> 7;
            num += num << 3;
            num ^= num >> 17;
            return num + (num << 5);
        }
        public static uint HexColorToInt(string hex)
        {
            uint num = uint.Parse(hex, NumberStyles.HexNumber);
            return num | 4278190080U;
        }
        public static uint TmxColorToInt(TmxColor col)
        {
            uint num = 255U;
            num <<= 8;
            num |= (byte)col.R;
            num <<= 8;
            num |= (byte)col.G;
            num <<= 8;
            return num | (byte)col.B;
        }
        public static uint ColorToUInt(Color color)
        {
            return (uint)(color.A << 24 | color.R << 16 | color.G << 8 | color.B);
        }
        public static Color UIntToColor(uint color)
        {
            byte alpha = (byte)(color >> 24);
            byte red = (byte)(color >> 16);
            byte green = (byte)(color >> 8);
            byte blue = (byte)color;
            return Color.FromArgb(alpha, red, green, blue);
        }
        public static void ConsoleWrite(string text, params object[] args)
        {
            int count = Console.WindowWidth - text.Length % Console.WindowWidth;
            string format = text + new string(' ', count);
            int cursorTop = Console.CursorTop;
            Console.SetCursorPosition(0, cursorTop);
            Console.Write(format, args);
            Console.SetCursorPosition(0, cursorTop);
        }
        public static TiledTilesetTile GetTileById(this TiledTileset tileset, int gid)
        {
            TiledTilesetTile result = null;
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
        public static decimal TryGetDecimal(this PropertyDict dict, string key)
        {
            dict.TryGetValue(key, out string text);
            return (text != null) ? decimal.Parse(text) : 0m;
        }
    }
}
