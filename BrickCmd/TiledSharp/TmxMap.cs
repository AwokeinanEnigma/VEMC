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
			this.Version = (string)xelement.Attribute("version");
			this.Orientation = (TmxMap.OrientationType)Enum.Parse(typeof(TmxMap.OrientationType), xelement.Attribute("orientation").Value, true);
			this.Width = (int)xelement.Attribute("width");
			this.Height = (int)xelement.Attribute("height");
			this.TileWidth = (int)xelement.Attribute("tilewidth");
			this.TileHeight = (int)xelement.Attribute("tileheight");
			this.BackgroundColor = new TmxColor(xelement.Attribute("backgroundcolor"));
			this.Tilesets = new TmxList<TmxTileset>();
			foreach (XElement xTileset in xelement.Elements("tileset"))
			{
				this.Tilesets.Add(new TmxTileset(xTileset, base.TmxDirectory));
			}
			this.Layers = new TmxList<TmxLayer>();
			foreach (XElement xLayer in xelement.Elements("layer"))
			{
				this.Layers.Add(new TmxLayer(xLayer, this.Width, this.Height));
			}
			this.ObjectGroups = new TmxList<TmxObjectGroup>();
			foreach (XElement xObjectGroup in xelement.Elements("objectgroup"))
			{
				this.ObjectGroups.Add(new TmxObjectGroup(xObjectGroup));
			}
			this.ImageLayers = new TmxList<TmxImageLayer>();
			foreach (XElement xImageLayer in xelement.Elements("imagelayer"))
			{
				this.ImageLayers.Add(new TmxImageLayer(xImageLayer, base.TmxDirectory));
			}
			this.Properties = new PropertyDict(xelement.Element("properties"));
		}
		public enum OrientationType : byte
		{
			Orthogonal,
			Isometric,
			Staggered
		}
	}
}
