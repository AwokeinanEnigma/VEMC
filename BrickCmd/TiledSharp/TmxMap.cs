using System;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x02000068 RID: 104
	public class TmxMap : TmxDocument
	{
		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x060003B0 RID: 944 RVA: 0x00018F3A File Offset: 0x0001713A
		// (set) Token: 0x060003B1 RID: 945 RVA: 0x00018F42 File Offset: 0x00017142
		public string Version { get; private set; }

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x060003B2 RID: 946 RVA: 0x00018F4B File Offset: 0x0001714B
		// (set) Token: 0x060003B3 RID: 947 RVA: 0x00018F53 File Offset: 0x00017153
		public TmxMap.OrientationType Orientation { get; private set; }

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060003B4 RID: 948 RVA: 0x00018F5C File Offset: 0x0001715C
		// (set) Token: 0x060003B5 RID: 949 RVA: 0x00018F64 File Offset: 0x00017164
		public int Width { get; private set; }

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x060003B6 RID: 950 RVA: 0x00018F6D File Offset: 0x0001716D
		// (set) Token: 0x060003B7 RID: 951 RVA: 0x00018F75 File Offset: 0x00017175
		public int Height { get; private set; }

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x060003B8 RID: 952 RVA: 0x00018F7E File Offset: 0x0001717E
		// (set) Token: 0x060003B9 RID: 953 RVA: 0x00018F86 File Offset: 0x00017186
		public int TileWidth { get; private set; }

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x060003BA RID: 954 RVA: 0x00018F8F File Offset: 0x0001718F
		// (set) Token: 0x060003BB RID: 955 RVA: 0x00018F97 File Offset: 0x00017197
		public int TileHeight { get; private set; }

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x060003BC RID: 956 RVA: 0x00018FA0 File Offset: 0x000171A0
		// (set) Token: 0x060003BD RID: 957 RVA: 0x00018FA8 File Offset: 0x000171A8
		public TmxColor BackgroundColor { get; private set; }

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x060003BE RID: 958 RVA: 0x00018FB1 File Offset: 0x000171B1
		// (set) Token: 0x060003BF RID: 959 RVA: 0x00018FB9 File Offset: 0x000171B9
		public TmxList<TmxTileset> Tilesets { get; private set; }

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x060003C0 RID: 960 RVA: 0x00018FC2 File Offset: 0x000171C2
		// (set) Token: 0x060003C1 RID: 961 RVA: 0x00018FCA File Offset: 0x000171CA
		public TmxList<TmxLayer> Layers { get; private set; }

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x060003C2 RID: 962 RVA: 0x00018FD3 File Offset: 0x000171D3
		// (set) Token: 0x060003C3 RID: 963 RVA: 0x00018FDB File Offset: 0x000171DB
		public TmxList<TmxObjectGroup> ObjectGroups { get; private set; }

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x060003C4 RID: 964 RVA: 0x00018FE4 File Offset: 0x000171E4
		// (set) Token: 0x060003C5 RID: 965 RVA: 0x00018FEC File Offset: 0x000171EC
		public TmxList<TmxImageLayer> ImageLayers { get; private set; }

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x060003C6 RID: 966 RVA: 0x00018FF5 File Offset: 0x000171F5
		// (set) Token: 0x060003C7 RID: 967 RVA: 0x00018FFD File Offset: 0x000171FD
		public PropertyDict Properties { get; private set; }

		// Token: 0x060003C8 RID: 968 RVA: 0x00019008 File Offset: 0x00017208
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

		// Token: 0x02000069 RID: 105
		public enum OrientationType : byte
		{
			// Token: 0x04000278 RID: 632
			Orthogonal,
			// Token: 0x04000279 RID: 633
			Isometric,
			// Token: 0x0400027A RID: 634
			Staggered
		}
	}
}
