using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x02000073 RID: 115
	public class TmxTileset : TmxDocument, ITmxElement
	{
		// Token: 0x170000EB RID: 235
		// (get) Token: 0x0600040A RID: 1034 RVA: 0x0001A076 File Offset: 0x00018276
		// (set) Token: 0x0600040B RID: 1035 RVA: 0x0001A07E File Offset: 0x0001827E
		public int FirstGid { get; private set; }

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x0600040C RID: 1036 RVA: 0x0001A087 File Offset: 0x00018287
		// (set) Token: 0x0600040D RID: 1037 RVA: 0x0001A08F File Offset: 0x0001828F
		public string Name { get; private set; }

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x0600040E RID: 1038 RVA: 0x0001A098 File Offset: 0x00018298
		// (set) Token: 0x0600040F RID: 1039 RVA: 0x0001A0A0 File Offset: 0x000182A0
		public int TileWidth { get; private set; }

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x06000410 RID: 1040 RVA: 0x0001A0A9 File Offset: 0x000182A9
		// (set) Token: 0x06000411 RID: 1041 RVA: 0x0001A0B1 File Offset: 0x000182B1
		public int TileHeight { get; private set; }

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x06000412 RID: 1042 RVA: 0x0001A0BA File Offset: 0x000182BA
		// (set) Token: 0x06000413 RID: 1043 RVA: 0x0001A0C2 File Offset: 0x000182C2
		public int Spacing { get; private set; }

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x06000414 RID: 1044 RVA: 0x0001A0CB File Offset: 0x000182CB
		// (set) Token: 0x06000415 RID: 1045 RVA: 0x0001A0D3 File Offset: 0x000182D3
		public int Margin { get; private set; }

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x06000416 RID: 1046 RVA: 0x0001A0DC File Offset: 0x000182DC
		// (set) Token: 0x06000417 RID: 1047 RVA: 0x0001A0E4 File Offset: 0x000182E4
		public TmxTileOffset TileOffset { get; private set; }

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x06000418 RID: 1048 RVA: 0x0001A0ED File Offset: 0x000182ED
		// (set) Token: 0x06000419 RID: 1049 RVA: 0x0001A0F5 File Offset: 0x000182F5
		public TmxImage Image { get; private set; }

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x0600041A RID: 1050 RVA: 0x0001A0FE File Offset: 0x000182FE
		// (set) Token: 0x0600041B RID: 1051 RVA: 0x0001A106 File Offset: 0x00018306
		public TmxList<TmxTerrain> Terrains { get; private set; }

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x0600041C RID: 1052 RVA: 0x0001A10F File Offset: 0x0001830F
		// (set) Token: 0x0600041D RID: 1053 RVA: 0x0001A117 File Offset: 0x00018317
		public List<TmxTilesetTile> Tiles { get; private set; }

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x0600041E RID: 1054 RVA: 0x0001A120 File Offset: 0x00018320
		// (set) Token: 0x0600041F RID: 1055 RVA: 0x0001A128 File Offset: 0x00018328
		public PropertyDict Properties { get; private set; }

		// Token: 0x06000420 RID: 1056 RVA: 0x0001A131 File Offset: 0x00018331
		public TmxTileset(XDocument xDoc, string tmxDir) : this(xDoc.Element("tileset"), tmxDir)
		{
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x0001A14C File Offset: 0x0001834C
		public TmxTileset(XElement xTileset, string tmxDir = "")
		{
			XAttribute xattribute = xTileset.Attribute("firstgid");
			string text = (string)xTileset.Attribute("source");
			if (text != null)
			{
				text = Path.Combine(tmxDir, text);
				this.FirstGid = (int)xattribute;
				XDocument xDoc = base.ReadXml(text);
				TmxTileset tmxTileset = new TmxTileset(xDoc, base.TmxDirectory);
				this.Name = tmxTileset.Name;
				this.TileWidth = tmxTileset.TileWidth;
				this.TileHeight = tmxTileset.TileHeight;
				this.Spacing = tmxTileset.Spacing;
				this.Margin = tmxTileset.Margin;
				this.TileOffset = tmxTileset.TileOffset;
				this.Image = tmxTileset.Image;
				this.Terrains = tmxTileset.Terrains;
				this.Tiles = tmxTileset.Tiles;
				this.Properties = tmxTileset.Properties;
				return;
			}
			if (xattribute != null)
			{
				this.FirstGid = (int)xattribute;
			}
			this.Name = (string)xTileset.Attribute("name");
			this.TileWidth = (int)xTileset.Attribute("tilewidth");
			this.TileHeight = (int)xTileset.Attribute("tileheight");
			this.Spacing = (((int?)xTileset.Attribute("spacing")) ?? 0);
			this.Margin = (((int?)xTileset.Attribute("margin")) ?? 0);
			this.TileOffset = new TmxTileOffset(xTileset.Element("tileoffset"));
			XElement xelement = xTileset.Element("image");
			if (xelement != null)
			{
				this.Image = new TmxImage(xelement, tmxDir);
			}
			this.Terrains = new TmxList<TmxTerrain>();
			XElement xelement2 = xTileset.Element("terraintype");
			if (xelement2 != null)
			{
				foreach (XElement xTerrain in xelement2.Elements("terrain"))
				{
					this.Terrains.Add(new TmxTerrain(xTerrain));
				}
			}
			this.Tiles = new List<TmxTilesetTile>();
			foreach (XElement xTile in xTileset.Elements("tile"))
			{
				TmxTilesetTile item = new TmxTilesetTile(xTile, this.Terrains, tmxDir);
				this.Tiles.Add(item);
			}
			this.Properties = new PropertyDict(xTileset.Element("properties"));
		}
	}
}
