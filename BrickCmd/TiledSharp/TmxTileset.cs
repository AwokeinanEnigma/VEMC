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
            var xFirstGid = xTileset.Attribute("firstgid");
            var source = (string)xTileset.Attribute("source");

            if (source != null)
            {
                // Prepend the parent TMX directory if necessary
                source = Path.Combine(tmxDir, source);

                // source is always preceded by firstgid
                FirstGid = (int)xFirstGid;

                // Everything else is in the TSX file
                var xDocTileset = ReadXml(source);
                var ts = new TmxTileset(xDocTileset, TmxDirectory);
                Name = ts.Name;
                TileWidth = ts.TileWidth;
                TileHeight = ts.TileHeight;
                Spacing = ts.Spacing;
                Margin = ts.Margin;
                TileOffset = ts.TileOffset;
                Image = ts.Image;
                Terrains = ts.Terrains;
                Tiles = ts.Tiles;
                Properties = ts.Properties;
            }
            else
            {
                // firstgid is always in TMX, but not TSX
                if (xFirstGid != null)
                    FirstGid = (int)xFirstGid;

                Name = (string)xTileset.Attribute("name");
                TileWidth = (int)xTileset.Attribute("tilewidth");
                TileHeight = (int)xTileset.Attribute("tileheight");
                Spacing = (int?)xTileset.Attribute("spacing") ?? 0;
                Margin = (int?)xTileset.Attribute("margin") ?? 0;
                TileOffset = new TmxTileOffset(xTileset.Element("tileoffset"));
                Image = new TmxImage(xTileset.Element("image"), tmxDir);

                Terrains = new TmxList<TmxTerrain>();
                var xTerrainType = xTileset.Element("terraintypes");
                if (xTerrainType != null)
                {
                    foreach (var e in xTerrainType.Elements("terrain"))
                        Terrains.Add(new TmxTerrain(e));
                }

                Tiles = new List<TmxTilesetTile>();
                foreach (var xTile in xTileset.Elements("tile"))
                {
                    var tile = new TmxTilesetTile(xTile, Terrains, tmxDir);
                    Tiles.Add(tile);
                }

                Properties = new PropertyDict(xTileset.Element("properties"));
            }
        }
    }
}
