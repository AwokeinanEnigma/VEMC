using System;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x02000075 RID: 117
	public class TmxTerrain : ITmxElement
	{
		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x06000427 RID: 1063 RVA: 0x0001A4B3 File Offset: 0x000186B3
		// (set) Token: 0x06000428 RID: 1064 RVA: 0x0001A4BB File Offset: 0x000186BB
		public string Name { get; private set; }

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x06000429 RID: 1065 RVA: 0x0001A4C4 File Offset: 0x000186C4
		// (set) Token: 0x0600042A RID: 1066 RVA: 0x0001A4CC File Offset: 0x000186CC
		public int Tile { get; private set; }

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x0600042B RID: 1067 RVA: 0x0001A4D5 File Offset: 0x000186D5
		// (set) Token: 0x0600042C RID: 1068 RVA: 0x0001A4DD File Offset: 0x000186DD
		public PropertyDict Properties { get; private set; }

		// Token: 0x0600042D RID: 1069 RVA: 0x0001A4E8 File Offset: 0x000186E8
		public TmxTerrain(XElement xTerrain)
		{
			this.Name = (string)xTerrain.Attribute("name");
			this.Tile = (int)xTerrain.Attribute("tile");
			this.Properties = new PropertyDict(xTerrain.Element("properties"));
		}
	}
}
