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
                FirstGid = (int)xattribute;
                XDocument xDoc = base.ReadXml(text);
                TmxTileset tmxTileset = new TmxTileset(xDoc, base.TmxDirectory);
                Name = tmxTileset.Name;
                TileWidth = tmxTileset.TileWidth;
                TileHeight = tmxTileset.TileHeight;
                Spacing = tmxTileset.Spacing;
                Margin = tmxTileset.Margin;
                TileOffset = tmxTileset.TileOffset;
                Image = tmxTileset.Image;
                Terrains = tmxTileset.Terrains;
                Tiles = tmxTileset.Tiles;
                Properties = tmxTileset.Properties;
                return;
            }
            if (xattribute != null)
            {
                FirstGid = (int)xattribute;
            }
            Name = (string)xTileset.Attribute("name");
            TileWidth = (int)xTileset.Attribute("tilewidth");
            TileHeight = (int)xTileset.Attribute("tileheight");
            Spacing = (((int?)xTileset.Attribute("spacing")) ?? 0);
            Margin = (((int?)xTileset.Attribute("margin")) ?? 0);
            TileOffset = new TmxTileOffset(xTileset.Element("tileoffset"));
            XElement xelement = xTileset.Element("image");
            if (xelement != null)
            {
                Image = new TmxImage(xelement, tmxDir);
            }
            Terrains = new TmxList<TmxTerrain>();
            XElement xelement2 = xTileset.Element("terraintype");
            if (xelement2 != null)
            {
                foreach (XElement xTerrain in xelement2.Elements("terrain"))
                {
                    Terrains.Add(new TmxTerrain(xTerrain));
                }
            }
            Tiles = new List<TmxTilesetTile>();
            foreach (XElement xTile in xTileset.Elements("tile"))
            {
                TmxTilesetTile item = new TmxTilesetTile(xTile, Terrains, tmxDir);
                Tiles.Add(item);
            }
            Properties = new PropertyDict(xTileset.Element("properties"));
        }
    }
}
