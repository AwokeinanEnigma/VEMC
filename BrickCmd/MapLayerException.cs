using System;

namespace VEMC
{
	// Token: 0x0200002D RID: 45
	internal class MapLayerException : Exception
	{
		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060001EE RID: 494 RVA: 0x000086BA File Offset: 0x000068BA
		// (set) Token: 0x060001EF RID: 495 RVA: 0x000086C2 File Offset: 0x000068C2
		public string Layer { get; private set; }

		// Token: 0x060001F0 RID: 496 RVA: 0x000086CB File Offset: 0x000068CB
		public MapLayerException(string layer) : base("Map layer \"" + layer + "\" is missing.")
		{
			this.Layer = layer;
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x000086EA File Offset: 0x000068EA
		public MapLayerException(string layer, Exception innerException) : base("Map layer \"" + layer + "\" is missing.", innerException)
		{
			this.Layer = layer;
		}
	}
}
