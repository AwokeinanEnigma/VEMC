using System;

namespace VEMC
{
	// Token: 0x0200002E RID: 46
	internal class MapPropertyException : Exception
	{
		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060001F2 RID: 498 RVA: 0x0000870A File Offset: 0x0000690A
		// (set) Token: 0x060001F3 RID: 499 RVA: 0x00008712 File Offset: 0x00006912
		public string Property { get; private set; }

		// Token: 0x060001F4 RID: 500 RVA: 0x0000871B File Offset: 0x0000691B
		public MapPropertyException(string property) : base("Map property \"" + property + "\" is missing.")
		{
			this.Property = property;
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x0000873A File Offset: 0x0000693A
		public MapPropertyException(string property, Exception innerException) : base("Map property \"" + property + "\" is missing.", innerException)
		{
			this.Property = property;
		}
	}
}
