using System;
using System.Collections.Generic;
using System.Drawing;

namespace VEMC
{
	// Token: 0x02000030 RID: 48
	internal class OptimizedTileset
	{
		// Token: 0x17000067 RID: 103
		// (get) Token: 0x060001FE RID: 510 RVA: 0x00008C5C File Offset: 0x00006E5C
		// (set) Token: 0x060001FF RID: 511 RVA: 0x00008C64 File Offset: 0x00006E64
		public int Width { get; set; }

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000200 RID: 512 RVA: 0x00008C6D File Offset: 0x00006E6D
		// (set) Token: 0x06000201 RID: 513 RVA: 0x00008C75 File Offset: 0x00006E75
		public Dictionary<int, int> TranslationTable { get; set; }

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000202 RID: 514 RVA: 0x00008C7E File Offset: 0x00006E7E
		// (set) Token: 0x06000203 RID: 515 RVA: 0x00008C86 File Offset: 0x00006E86
		public Color[] Palette { get; set; }

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000204 RID: 516 RVA: 0x00008C8F File Offset: 0x00006E8F
		// (set) Token: 0x06000205 RID: 517 RVA: 0x00008C97 File Offset: 0x00006E97
		public byte[] IndexedImage { get; set; }

		// Token: 0x06000206 RID: 518 RVA: 0x00008CA0 File Offset: 0x00006EA0
		public OptimizedTileset()
		{
			this.TranslationTable = new Dictionary<int, int>();
		}

		// Token: 0x06000207 RID: 519 RVA: 0x00008CB3 File Offset: 0x00006EB3
		public int Translate(int old)
		{
            //Console.Write($"translate old {old}. max is {TranslationTable.Count}");

			return this.TranslationTable[old];
		}
	}
}
