using System;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x02000074 RID: 116
	public class TmxTileOffset
	{
		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x06000422 RID: 1058 RVA: 0x0001A434 File Offset: 0x00018634
		// (set) Token: 0x06000423 RID: 1059 RVA: 0x0001A43C File Offset: 0x0001863C
		public int X { get; private set; }

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x06000424 RID: 1060 RVA: 0x0001A445 File Offset: 0x00018645
		// (set) Token: 0x06000425 RID: 1061 RVA: 0x0001A44D File Offset: 0x0001864D
		public int Y { get; private set; }

		// Token: 0x06000426 RID: 1062 RVA: 0x0001A458 File Offset: 0x00018658
		public TmxTileOffset(XElement xTileOffset)
		{
			if (xTileOffset == null)
			{
				this.X = 0;
				this.Y = 0;
				return;
			}
			this.X = (int)xTileOffset.Attribute("x");
			this.Y = (int)xTileOffset.Attribute("y");
		}
	}
}
