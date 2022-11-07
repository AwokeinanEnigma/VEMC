using System;

namespace fNbt
{
	// Token: 0x0200000C RID: 12
	internal enum NbtParseState
	{
		// Token: 0x04000026 RID: 38
		AtStreamBeginning,
		// Token: 0x04000027 RID: 39
		AtCompoundBeginning,
		// Token: 0x04000028 RID: 40
		InCompound,
		// Token: 0x04000029 RID: 41
		AtCompoundEnd,
		// Token: 0x0400002A RID: 42
		AtListBeginning,
		// Token: 0x0400002B RID: 43
		InList,
		// Token: 0x0400002C RID: 44
		AtStreamEnd,
		// Token: 0x0400002D RID: 45
		Error
	}
}
