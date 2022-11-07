using System;
using JetBrains.Annotations;

namespace fNbt
{
	public delegate bool TagSelector([NotNull] NbtTag tag);
}
