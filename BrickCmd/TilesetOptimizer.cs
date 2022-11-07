using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using TiledSharp;

namespace VEMC
{
	internal class TilesetOptimizer
	{
		public static OptimizedTileset Optimize(TmxTileset tset)
		{
			OptimizedTileset optimizedTileset = new OptimizedTileset();
			List<Color> list;
			byte[] array;
			Point point = TilesetOptimizer.LoadImage(tset.Image.Source, tset.Image.Trans, out list, out array);
			int x = point.X;
			int y = point.Y;
			if (x % 8 != 0 || y % 8 != 0)
			{
				string message = string.Format("The dimensions of the tileset image \"{0}\" are not multiples of {1}. Tiles are {1}x{1}, so this size doesn't make sense. Expand the tileset image to the next multiple of {1} and try again.", tset.Image.Source, 8);
				throw new MapBuildException(message);
			}
			IDictionary<int, int> dictionary = new Dictionary<int, int>();
			IList<byte[]> list2 = new List<byte[]>();
			int num = 1;
			for (int i = 0; i < y; i += 8)
			{
				for (int j = 0; j < x; j += 8)
				{
					int num2 = i / 8 * (x / 8) + j / 8;
					int key = tset.FirstGid + num2;
					byte[] array2 = new byte[64];
					int num3 = 0;
					for (int k = i; k < i + 8; k++)
					{
						for (int l = j; l < j + 8; l++)
						{
							int num4 = k * x + l;
							if (num4 < array.Length)
							{
								array2[num3] = array[num4];
							}
							num3++;
						}
					}
					int key2 = Utility.Hash(array2);
					if (!dictionary.ContainsKey(key2))
					{
						dictionary.Add(key2, num);
						optimizedTileset.TranslationTable.Add(key, num);
						list2.Add(array2);
						num++;
					}
					else
					{
						optimizedTileset.TranslationTable.Add(key, dictionary[key2]);
					}
				}
			}
			optimizedTileset.Palette = list.ToArray();
			int width = tset.Image.Width;
			int height = tset.Image.Height;
			int num5 = num * 8 * 8;
			int num6 = 0;
			for (int m = 0; m < TilesetOptimizer.TILESET_SIZES.Length; m++)
			{
				if (num5 < TilesetOptimizer.TILESET_SIZES[m] * TilesetOptimizer.TILESET_SIZES[m])
				{
					num6 = TilesetOptimizer.TILESET_SIZES[m];
					break;
				}
			}
			if (num6 > 0)
			{
				byte[] array3 = new byte[num6 * num6];
				for (int n = 0; n < num - 1; n++)
				{
					int num7 = n * 8 % num6;
					int num8 = n * 8 / num6 * 8;
					for (int num9 = 0; num9 < 64; num9++)
					{
						int num10 = num9 % 8;
						int num11 = num9 / 8;
						int num12 = (num8 + num11) * num6 + (num7 + num10);
						array3[num12] = list2[n][num9];
					}
				}
				optimizedTileset.Width = num6;
				optimizedTileset.IndexedImage = array3;
				return optimizedTileset;
			}
			throw new InvalidOperationException(string.Format("The tileset is too large to be stored in a {0}x{0} pixel image.", TilesetOptimizer.TILESET_SIZES[TilesetOptimizer.TILESET_SIZES.Length - 1]));
		}
		private static Point LoadImage(string file, TmxColor transColor, out List<Color> colors, out byte[] pixelcols)
		{
			Bitmap bitmap = new Bitmap(file);
			Bitmap bitmap2 = bitmap.Clone(new Rectangle(Point.Empty, bitmap.Size), PixelFormat.Format32bppArgb);
			colors = new List<Color>();
			pixelcols = new byte[bitmap2.Width * bitmap2.Height];
			Rectangle rect = new Rectangle(0, 0, bitmap2.Width, bitmap2.Height);
			BitmapData bitmapData = bitmap2.LockBits(rect, ImageLockMode.ReadOnly, bitmap2.PixelFormat);
			IntPtr scan = bitmapData.Scan0;
			int num = Math.Abs(bitmapData.Stride) * bitmap2.Height;
			byte[] array = new byte[num];
			Marshal.Copy(scan, array, 0, num);
			bitmap2.UnlockBits(bitmapData);
			if (bitmap2.PixelFormat == PixelFormat.Format32bppArgb)
			{
				for (int i = 0; i < num; i += 4)
				{
					Color item = Color.FromArgb((int)array[i + 3], (int)array[i + 2], (int)array[i + 1], (int)array[i]);
					if (!colors.Contains(item))
					{
						colors.Add(item);
					}
					pixelcols[i / 4] = (byte)colors.IndexOf(item);
				}
				if (colors.Count > 256)
				{
					ConsoleColor foregroundColor = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Warning: There are more than 256 colors in \"{0}\"", Path.GetFileName(file));
					Console.ForegroundColor = foregroundColor;
				}
				Color item2 = Color.FromArgb(255, transColor.R, transColor.G, transColor.B);
				int num2 = colors.IndexOf(item2);
				if (num2 >= 0)
				{
					colors[num2] = Color.Transparent;
				}
				return new Point(bitmap2.Width, bitmap2.Height);
			}
			throw new MapBuildException("Tileset image is not 32bppARGB, but it should be at this point. Tell Dave about this.");
		}
		private const int TILE_SIZE = 8;
		private static readonly int[] TILESET_SIZES = new int[]
		{
			32,
			64,
			128,
			256,
			384,
			480,
			512,
			640,
			768,
			1024
		};
	}
}
