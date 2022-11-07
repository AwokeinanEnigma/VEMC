using JetBrains.Annotations;
using System;

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
