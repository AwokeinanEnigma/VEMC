using JetBrains.Annotations;
using System;

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
