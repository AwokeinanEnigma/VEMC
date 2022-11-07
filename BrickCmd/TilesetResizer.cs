using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using TiledSharp;

namespace VEMC
{
	internal class TilesetResizer
	{
		public Image TilesetImage
		{
			get
			{
				return this.tilesetImage;
			}
		}
		public TilesetResizer(TmxTileset tileset, int tileCount)
		{
			this.tilesetImage = new Bitmap(tileset.Image.Source);
			this.tileCount = tileCount;
			this.Resize();
		}
		public TilesetResizer(Image tilesetImage, int tileCount)
		{
			this.tilesetImage = new Bitmap(tilesetImage);
			this.tileCount = tileCount;
			this.Resize();
		}
		private void Resize()
		{
			int width = this.tilesetImage.Width;
			int height = this.tilesetImage.Height;
			int num = this.tileCount * 8 * 8;
			int num2 = 0;
			for (int i = 0; i < TilesetResizer.TILESET_SIZES.Length; i++)
			{
				if (num < TilesetResizer.TILESET_SIZES[i] * TilesetResizer.TILESET_SIZES[i])
				{
					num2 = TilesetResizer.TILESET_SIZES[i];
					break;
				}
			}
			if (num2 > 0)
			{
				Bitmap bitmap = new Bitmap(num2, num2);
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.SmoothingMode = SmoothingMode.None;
					graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
					for (int j = 0; j < this.tileCount; j++)
					{
						int num3 = j * 8 % this.tilesetImage.Width;
						int num4 = j * 8 / this.tilesetImage.Width * 8;
						int num5 = j * 8 % bitmap.Width;
						int num6 = j * 8 / bitmap.Width * 8;
						graphics.DrawImage(this.tilesetImage, (float)num5, (float)num6, new RectangleF((float)num3, (float)num4, 8f, 8f), GraphicsUnit.Pixel);
					}
				}
				this.tilesetImage.Dispose();
				this.tilesetImage = bitmap;
				return;
			}
			throw new InvalidOperationException(string.Format("The tileset is too large to be stored in a {0}x{0} pixel image.", TilesetResizer.TILESET_SIZES[TilesetResizer.TILESET_SIZES.Length - 1]));
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
		private Bitmap tilesetImage;
		private int tileCount;
	}
}
