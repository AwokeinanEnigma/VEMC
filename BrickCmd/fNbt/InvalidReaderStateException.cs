using System;
using JetBrains.Annotations;

namespace fNbt
{
	[Serializable]
	public sealed class InvalidReaderStateException : InvalidOperationException
	{
		internal InvalidReaderStateException([NotNull] string message) : base(message)
		{
		}
	}
}
