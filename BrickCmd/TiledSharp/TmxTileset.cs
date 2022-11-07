using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace TiledSharp
{
	public class TmxTileset : TmxDocument, ITmxElement
	{
		public int FirstGid { get; private set; }
		public string Name { get; private set; }
		public int TileWidth { get; private set; }
		public int TileHeight { get; private set; }
		public int Spacing { get; private set; }
		public int Margin { get; private set; }
		public TmxTileOffset TileOffset { get; private set; }
		public TmxImage Image { get; private set; }
		public TmxList<TmxTerrain> Terrains { get; private set; }
		public List<TmxTilesetTile> Tiles { get; private set; }
		public PropertyDict Properties { get; private set; }
		public TmxTileset(XDocument xDoc, string tmxDir) : this(xDoc.Element("tileset"), tmxDir)
		{
		}
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
