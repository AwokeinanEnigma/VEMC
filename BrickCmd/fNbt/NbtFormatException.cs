using System;
using JetBrains.Annotations;

namespace fNbt
{
	[Serializable]
	public sealed class NbtFormatException : Exception
	{
		internal NbtFormatException([NotNull] string message) : base(message)
		{
		}
	}
}
