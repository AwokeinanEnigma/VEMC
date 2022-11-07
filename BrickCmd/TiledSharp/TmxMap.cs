using System;
using System.Xml.Linq;

namespace TiledSharp
{
    public class TmxMap : TmxDocument
    {
        public string Version { get; private set; }
        public TmxMap.OrientationType Orientation { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }
        public TmxColor BackgroundColor { get; private set; }
        public TmxList<TmxTileset> Tilesets { get; private set; }
        public TmxList<TmxLayer> Layers { get; private set; }
        public TmxList<TmxObjectGroup> ObjectGroups { get; private set; }
        public TmxList<TmxImageLayer> ImageLayers { get; private set; }
        public PropertyDict Properties { get; private set; }
        public TmxMap(string filename)
        {
            XDocument xdocument = base.ReadXml(filename);
            XElement xelement = xdocument.Element("map");
            Version = (string)xelement.Attribute("version");
            Orientation = (TmxMap.OrientationType)Enum.Parse(typeof(TmxMap.OrientationType), xelement.Attribute("orientation").Value, true);
            Width = (int)xelement.Attribute("width");
            Height = (int)xelement.Attribute("height");
            TileWidth = (int)xelement.Attribute("tilewidth");
            TileHeight = (int)xelement.Attribute("tileheight");
            BackgroundColor = new TmxColor(xelement.Attribute("backgroundcolor"));
            Tilesets = new TmxList<TmxTileset>();
            foreach (XElement xTileset in xelement.Elements("tileset"))
            {
                Tilesets.Add(new TmxTileset(xTileset, base.TmxDirectory));
            }
            Layers = new TmxList<TmxLayer>();
            foreach (XElement xLayer in xelement.Elements("layer"))
            {
                Layers.Add(new TmxLayer(xLayer, Width, Height));
            }
            ObjectGroups = new TmxList<TmxObjectGroup>();
            foreach (XElement xObjectGroup in xelement.Elements("objectgroup"))
            {
                ObjectGroups.Add(new TmxObjectGroup(xObjectGroup));
            }
            ImageLayers = new TmxList<TmxImageLayer>();
            foreach (XElement xImageLayer in xelement.Elements("imagelayer"))
            {
                ImageLayers.Add(new TmxImageLayer(xImageLayer, base.TmxDirectory));
            }
            Properties = new PropertyDict(xelement.Element("properties"));
        }
        public enum OrientationType : byte
        {
            Orthogonal,
            Isometric,
            Staggered
        }
    }
}
