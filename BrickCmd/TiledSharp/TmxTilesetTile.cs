using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x02000076 RID: 118
	public class TmxTilesetTile
	{
		// Token: 0x170000FB RID: 251
		// (get) Token: 0x0600042E RID: 1070 RVA: 0x0001A54C File Offset: 0x0001874C
		// (set) Token: 0x0600042F RID: 1071 RVA: 0x0001A554 File Offset: 0x00018754
		public int Id { get; private set; }

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x06000430 RID: 1072 RVA: 0x0001A55D File Offset: 0x0001875D
		// (set) Token: 0x06000431 RID: 1073 RVA: 0x0001A565 File Offset: 0x00018765
		public List<TmxTerrain> TerrainEdges { get; private set; }

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x06000432 RID: 1074 RVA: 0x0001A56E File Offset: 0x0001876E
		// (set) Token: 0x06000433 RID: 1075 RVA: 0x0001A576 File Offset: 0x00018776
		public double Probability { get; private set; }

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x06000434 RID: 1076 RVA: 0x0001A57F File Offset: 0x0001877F
		// (set) Token: 0x06000435 RID: 1077 RVA: 0x0001A587 File Offset: 0x00018787
		public TmxImage Image { get; private set; }

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x06000436 RID: 1078 RVA: 0x0001A590 File Offset: 0x00018790
		// (set) Token: 0x06000437 RID: 1079 RVA: 0x0001A598 File Offset: 0x00018798
		public PropertyDict Properties { get; private set; }

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x06000438 RID: 1080 RVA: 0x0001A5A1 File Offset: 0x000187A1
		public TmxTerrain TopLeft
		{
			get
			{
				return this.TerrainEdges[0];
			}
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x06000439 RID: 1081 RVA: 0x0001A5AF File Offset: 0x000187AF
		public TmxTerrain TopRight
		{
			get
			{
				return this.TerrainEdges[1];
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x0600043A RID: 1082 RVA: 0x0001A5BD File Offset: 0x000187BD
		public TmxTerrain BottomLeft
		{
			get
			{
				return this.TerrainEdges[2];
			}
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x0600043B RID: 1083 RVA: 0x0001A5CB File Offset: 0x000187CB
		public TmxTerrain BottomRight
		{
			get
			{
				return this.TerrainEdges[3];
			}
		}

		// Token: 0x0600043C RID: 1084 RVA: 0x0001A5DC File Offset: 0x000187DC
		public TmxTilesetTile(XElement xTile, TmxList<TmxTerrain> Terrains, string tmxDir = "")
		{
			this.Id = (int)xTile.Attribute("id");
			if (Terrains.Count > 0)
			{
				this.TerrainEdges = new List<TmxTerrain>(4);
				string[] array = ((string)xTile.Attribute("terrain")).Split(new char[]
				{
					','
				});
				foreach (string s in array)
				{
					int index;
					bool flag = int.TryParse(s, out index);
					TmxTerrain item;
					if (flag)
					{
						item = Terrains[index];
					}
					else
					{
						item = null;
					}
					this.TerrainEdges.Add(item);
				}
			}
			double? num = (double?)xTile.Attribute("probability");
			this.Probability = ((num != null) ? num.GetValueOrDefault() : 1.0);
			XElement xelement = xTile.Element("image");
			if (xelement != null)
			{
				this.Image = new TmxImage(xelement, tmxDir);
			}
			this.Properties = new PropertyDict(xTile.Element("properties"));
		}
	}
}
