using System;

namespace fNbt
{
	// Token: 0x0200000F RID: 15
	public enum NbtTagType : byte
	{
		// Token: 0x0400004D RID: 77
		Unknown = 255,
		// Token: 0x0400004E RID: 78
		End = 0,
		// Token: 0x0400004F RID: 79
		Byte,
		// Token: 0x04000050 RID: 80
		Short,
		// Token: 0x04000051 RID: 81
		Int,
		// Token: 0x04000052 RID: 82
		Long,
		// Token: 0x04000053 RID: 83
		Float,
		// Token: 0x04000054 RID: 84
		Double,
		// Token: 0x04000055 RID: 85
		ByteArray,
		// Token: 0x04000056 RID: 86
		String,
		// Token: 0x04000057 RID: 87
		List,
		// Token: 0x04000058 RID: 88
		Compound,
		// Token: 0x04000059 RID: 89
		IntArray
	}
}
