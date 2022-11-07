﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using TiledSharp;

namespace VEMC
{
	// Token: 0x0200003F RID: 63
	internal class TilesetResizer
	{
		// Token: 0x17000078 RID: 120
		// (get) Token: 0x0600023E RID: 574 RVA: 0x0000A539 File Offset: 0x00008739
		public Image TilesetImage
		{
			get
			{
				return this.tilesetImage;
			}
		}

		// Token: 0x0600023F RID: 575 RVA: 0x0000A541 File Offset: 0x00008741
		public TilesetResizer(TmxTileset tileset, int tileCount)
		{
			this.tilesetImage = new Bitmap(tileset.Image.Source);
			this.tileCount = tileCount;
			this.Resize();
		}

		// Token: 0x06000240 RID: 576 RVA: 0x0000A56C File Offset: 0x0000876C
		public TilesetResizer(Image tilesetImage, int tileCount)
		{
			this.tilesetImage = new Bitmap(tilesetImage);
			this.tileCount = tileCount;
			this.Resize();
		}

		// Token: 0x06000241 RID: 577 RVA: 0x0000A590 File Offset: 0x00008790
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

		// Token: 0x040000CA RID: 202
		private const int TILE_SIZE = 8;

		// Token: 0x040000CB RID: 203
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

		// Token: 0x040000CC RID: 204
		private Bitmap tilesetImage;

		// Token: 0x040000CD RID: 205
		private int tileCount;
	}
}
