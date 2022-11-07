using System;
using System.Collections.Generic;

namespace VEMC
{
	// Token: 0x02000038 RID: 56
	internal struct TileGroup
	{
		// Token: 0x0600022A RID: 554 RVA: 0x00009208 File Offset: 0x00007408
		public TileGroup(TileGroup group)
		{
			this.tiles = new List<TileGrouper.TileData>(group.tiles);
			this.depth = group.depth;
			this.x = group.x;
			this.y = group.y;
			this.originX = group.originX;
			this.originY = group.originY;
			this.width = group.width;
			this.height = group.height;
			rainaway = group.rainaway;
		}

		// Token: 0x040000AB RID: 171
		public List<TileGrouper.TileData> tiles;

		// Token: 0x040000AC RID: 172
		public int depth;

		// Token: 0x040000AD RID: 173
		public int x;

		// Token: 0x040000AE RID: 174
		public int y;

		// Token: 0x040000AF RID: 175
		public int originX;

		// Token: 0x040000B0 RID: 176
		public int originY;

		// Token: 0x040000B1 RID: 177
		public int width;

		// Token: 0x040000B2 RID: 178
		public int height;

		public int rainaway;
	}
}
