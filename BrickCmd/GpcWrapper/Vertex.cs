using System;

namespace GpcWrapper
{
	// Token: 0x02000022 RID: 34
	public struct Vertex
	{
		// Token: 0x060001C6 RID: 454 RVA: 0x00007A8C File Offset: 0x00005C8C
		public Vertex(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x00007A9C File Offset: 0x00005C9C
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"(",
				this.X.ToString(),
				",",
				this.Y.ToString(),
				")"
			});
		}

		// Token: 0x04000081 RID: 129
		public double X;

		// Token: 0x04000082 RID: 130
		public double Y;
	}
}
