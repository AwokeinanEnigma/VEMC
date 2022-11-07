using System;

namespace TiledSharp
{
	// Token: 0x02000066 RID: 102
	public class TmxLayerTile
	{
		// Token: 0x170000BF RID: 191
		// (get) Token: 0x0600039F RID: 927 RVA: 0x00018D94 File Offset: 0x00016F94
		// (set) Token: 0x060003A0 RID: 928 RVA: 0x00018D9C File Offset: 0x00016F9C
		public int Gid { get; private set; }

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x060003A1 RID: 929 RVA: 0x00018DA5 File Offset: 0x00016FA5
		// (set) Token: 0x060003A2 RID: 930 RVA: 0x00018DAD File Offset: 0x00016FAD
		public int X { get; private set; }

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x060003A3 RID: 931 RVA: 0x00018DB6 File Offset: 0x00016FB6
		// (set) Token: 0x060003A4 RID: 932 RVA: 0x00018DBE File Offset: 0x00016FBE
		public int Y { get; private set; }

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x060003A5 RID: 933 RVA: 0x00018DC7 File Offset: 0x00016FC7
		// (set) Token: 0x060003A6 RID: 934 RVA: 0x00018DCF File Offset: 0x00016FCF
		public bool HorizontalFlip { get; private set; }

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x060003A7 RID: 935 RVA: 0x00018DD8 File Offset: 0x00016FD8
		// (set) Token: 0x060003A8 RID: 936 RVA: 0x00018DE0 File Offset: 0x00016FE0
		public bool VerticalFlip { get; private set; }

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x060003A9 RID: 937 RVA: 0x00018DE9 File Offset: 0x00016FE9
		// (set) Token: 0x060003AA RID: 938 RVA: 0x00018DF1 File Offset: 0x00016FF1
		public bool DiagonalFlip { get; private set; }

		// Token: 0x060003AB RID: 939 RVA: 0x00018DFC File Offset: 0x00016FFC
		public TmxLayerTile(uint id, int x, int y)
		{
			this.X = x;
			this.Y = y;
			this.HorizontalFlip = ((id & 2147483648U) != 0U);
			this.VerticalFlip = ((id & 1073741824U) != 0U);
			this.DiagonalFlip = ((id & 536870912U) != 0U);
			uint gid = id & 536870911U;
			this.Gid = (int)gid;
		}

		// Token: 0x04000261 RID: 609
		private const uint FLIPPED_HORIZONTALLY_FLAG = 2147483648U;

		// Token: 0x04000262 RID: 610
		private const uint FLIPPED_VERTICALLY_FLAG = 1073741824U;

		// Token: 0x04000263 RID: 611
		private const uint FLIPPED_DIAGONALLY_FLAG = 536870912U;
	}
}
