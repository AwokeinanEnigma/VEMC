using System;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x0200000B RID: 11
	[Serializable]
	public sealed class NbtFormatException : Exception
	{
		// Token: 0x06000057 RID: 87 RVA: 0x0000339C File Offset: 0x0000159C
		internal NbtFormatException([NotNull] string message) : base(message)
		{
		}
	}
}
