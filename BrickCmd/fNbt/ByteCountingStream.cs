using JetBrains.Annotations;
using System.IO;

namespace fNbt
{
    internal class ByteCountingStream : Stream
    {
        public ByteCountingStream([NotNull] Stream stream)
        {
            baseStream = stream;
        }
        public override void Flush()
        {
            baseStream.Flush();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return baseStream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            baseStream.SetLength(value);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            readingManyBytes = true;
            int num = baseStream.Read(buffer, offset, count);
            readingManyBytes = false;
            if (!readingOneByte)
            {
                BytesRead += num;
            }
            return num;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            writingManyBytes = true;
            baseStream.Write(buffer, offset, count);
            writingManyBytes = false;
            if (!writingOneByte)
            {
                BytesWritten += count;
            }
        }
        public override int ReadByte()
        {
            readingOneByte = true;
            int num = base.ReadByte();
            readingOneByte = false;
            if (num >= 0 && !readingManyBytes)
            {
                BytesRead += 1L;
            }
            return num;
        }
        public override void WriteByte(byte value)
        {
            writingOneByte = true;
            base.WriteByte(value);
            writingOneByte = false;
            if (!writingManyBytes)
            {
                BytesWritten += 1L;
            }
        }
        public override bool CanRead => baseStream.CanRead;
        public override bool CanSeek => baseStream.CanSeek;
        public override bool CanWrite => baseStream.CanWrite;
        public override long Length => baseStream.Length;
        public override long Position
        {
            get => baseStream.Position;
            set => baseStream.Position = value;
        }
        public long BytesRead { get; private set; }
        public long BytesWritten { get; private set; }
        private readonly Stream baseStream;
        private bool readingOneByte;
        private bool writingOneByte;
        private bool readingManyBytes;
        private bool writingManyBytes;
    }
}
