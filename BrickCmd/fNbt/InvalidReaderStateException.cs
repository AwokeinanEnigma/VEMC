using System;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x02000003 RID: 3
	[Serializable]
	public sealed class InvalidReaderStateException : InvalidOperationException
	{
		// Token: 0x06000013 RID: 19 RVA: 0x000021E1 File Offset: 0x000003E1
		internal InvalidReaderStateException([NotNull] string message) : base(message)
		{
		}
	}
}
