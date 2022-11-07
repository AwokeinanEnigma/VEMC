using JetBrains.Annotations;
using System;
using System.IO;
using System.IO.Compression;

namespace fNbt
{
    public sealed class NbtFile
    {
        [CanBeNull]
        public string FileName { get; private set; }
        public NbtCompression FileCompression { get; private set; }
        [NotNull]
        public NbtCompound RootTag
        {
            get => rootTag;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Name == null)
                {
                    throw new ArgumentException("Root tag must be named.");
                }
                rootTag = value;
            }
        }
        public static bool BigEndianByDefault { get; set; } = true;
        public bool BigEndian { get; set; }
        public static int DefaultBufferSize
        {
            get => NbtFile.defaultBufferSize;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", value, "DefaultBufferSize cannot be negative.");
                }
                NbtFile.defaultBufferSize = value;
            }
        }
        public int BufferSize
        {
            get => bufferSize;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", value, "BufferSize cannot be negative.");
                }
                bufferSize = value;
            }
        }
        public NbtFile()
        {
            BigEndian = NbtFile.BigEndianByDefault;
            BufferSize = NbtFile.DefaultBufferSize;
            rootTag = new NbtCompound("");
        }
        public NbtFile([NotNull] NbtCompound rootTag) : this()
        {
            if (rootTag == null)
            {
                throw new ArgumentNullException("rootTag");
            }
            RootTag = rootTag;
        }
        public NbtFile([NotNull] string fileName) : this()
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            LoadFromFile(fileName, NbtCompression.AutoDetect, null);
        }
        public long LoadFromFile([NotNull] string fileName)
        {
            return LoadFromFile(fileName, NbtCompression.AutoDetect, null);
        }
        public long LoadFromFile([NotNull] string fileName, NbtCompression compression, [CanBeNull] TagSelector selector)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            long position;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.SequentialScan))
            {
                LoadFromStream(fileStream, compression, selector);
                FileName = fileName;
                position = fileStream.Position;
            }
            return position;
        }
        public long LoadFromBuffer([NotNull] byte[] buffer, int index, int length, NbtCompression compression, [CanBeNull] TagSelector selector)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            long position;
            using (MemoryStream memoryStream = new MemoryStream(buffer, index, length))
            {
                LoadFromStream(memoryStream, compression, selector);
                FileName = null;
                position = memoryStream.Position;
            }
            return position;
        }
        public long LoadFromBuffer([NotNull] byte[] buffer, int index, int length, NbtCompression compression)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            long position;
            using (MemoryStream memoryStream = new MemoryStream(buffer, index, length))
            {
                LoadFromStream(memoryStream, compression, null);
                FileName = null;
                position = memoryStream.Position;
            }
            return position;
        }
        public long LoadFromStream([NotNull] Stream stream, NbtCompression compression, [CanBeNull] TagSelector selector)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            FileName = null;
            if (compression == NbtCompression.AutoDetect)
            {
                FileCompression = NbtFile.DetectCompression(stream);
            }
            else
            {
                FileCompression = compression;
            }
            long num = 0L;
            if (stream.CanSeek)
            {
                num = stream.Position;
            }
            else
            {
                stream = new ByteCountingStream(stream);
            }
            switch (FileCompression)
            {
                case NbtCompression.None:
                    break;
                case NbtCompression.GZip:
                    using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress, true))
                    {
                        if (bufferSize > 0)
                        {
                            LoadFromStreamInternal(new BufferedStream(gzipStream, bufferSize), selector);
                        }
                        else
                        {
                            LoadFromStreamInternal(gzipStream, selector);
                        }
                        goto IL_110;
                    }
                    break;
                case NbtCompression.ZLib:
                    if (stream.ReadByte() != 120)
                    {
                        throw new InvalidDataException("Unrecognized ZLib header. Expected 0x78");
                    }
                    stream.ReadByte();
                    using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress, true))
                    {
                        if (bufferSize > 0)
                        {
                            LoadFromStreamInternal(new BufferedStream(deflateStream, bufferSize), selector);
                        }
                        else
                        {
                            LoadFromStreamInternal(deflateStream, selector);
                        }
                        goto IL_110;
                    }
                    goto IL_105;
                default:
                    goto IL_105;
            }
            LoadFromStreamInternal(stream, selector);
            goto IL_110;
        IL_105:
            throw new ArgumentOutOfRangeException("compression");
        IL_110:
            if (stream.CanSeek)
            {
                return stream.Position - num;
            }
            return ((ByteCountingStream)stream).BytesRead;
        }
        public long LoadFromStream([NotNull] Stream stream, NbtCompression compression)
        {
            return LoadFromStream(stream, compression, null);
        }
        private static NbtCompression DetectCompression([NotNull] Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new NotSupportedException("Cannot auto-detect compression on a stream that's not seekable.");
            }
            int num = stream.ReadByte();
            int num2 = num;
            NbtCompression result;
            if (num2 <= 10)
            {
                if (num2 == -1)
                {
                    throw new EndOfStreamException();
                }
                if (num2 == 10)
                {
                    result = NbtCompression.None;
                    goto IL_55;
                }
            }
            else
            {
                if (num2 == 31)
                {
                    result = NbtCompression.GZip;
                    goto IL_55;
                }
                if (num2 == 120)
                {
                    result = NbtCompression.ZLib;
                    goto IL_55;
                }
            }
            throw new InvalidDataException("Could not auto-detect compression format.");
        IL_55:
            stream.Seek(-1L, SeekOrigin.Current);
            return result;
        }
        private void LoadFromStreamInternal([NotNull] Stream stream, [CanBeNull] TagSelector tagSelector)
        {
            int num = stream.ReadByte();
            if (num < 0)
            {
                throw new EndOfStreamException();
            }
            if (num != 10)
            {
                throw new NbtFormatException("Given NBT stream does not start with a TAG_Compound");
            }
            NbtBinaryReader nbtBinaryReader = new NbtBinaryReader(stream, BigEndian)
            {
                Selector = tagSelector
            };
            NbtCompound nbtCompound = new NbtCompound(nbtBinaryReader.ReadString());
            nbtCompound.ReadTag(nbtBinaryReader);
            RootTag = nbtCompound;
        }
        public long SaveToFile([NotNull] string fileName, NbtCompression compression)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            long result;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 65536, FileOptions.SequentialScan))
            {
                result = SaveToStream(fileStream, compression);
            }
            return result;
        }
        public long SaveToBuffer([NotNull] byte[] buffer, int index, NbtCompression compression)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            long result;
            using (MemoryStream memoryStream = new MemoryStream(buffer, index, buffer.Length - index))
            {
                result = SaveToStream(memoryStream, compression);
            }
            return result;
        }
        [NotNull]
        public byte[] SaveToBuffer(NbtCompression compression)
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                SaveToStream(memoryStream, compression);
                result = memoryStream.ToArray();
            }
            return result;
        }
        public long SaveToStream([NotNull] Stream stream, NbtCompression compression)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            switch (compression)
            {
                case NbtCompression.AutoDetect:
                    throw new ArgumentException("AutoDetect is not a valid NbtCompression value for saving.");
                case NbtCompression.None:
                case NbtCompression.GZip:
                case NbtCompression.ZLib:
                    {
                        if (rootTag.Name == null)
                        {
                            throw new NbtFormatException("Cannot save NbtFile: Root tag is not named. Its name may be an empty string, but not null.");
                        }
                        long num = 0L;
                        if (stream.CanSeek)
                        {
                            num = stream.Position;
                        }
                        else
                        {
                            stream = new ByteCountingStream(stream);
                        }
                        switch (compression)
                        {
                            case NbtCompression.None:
                                break;
                            case NbtCompression.GZip:
                                using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Compress, true))
                                {
                                    BufferedStream bufferedStream = new BufferedStream(gzipStream, 8192);
                                    RootTag.WriteTag(new NbtBinaryWriter(bufferedStream, BigEndian));
                                    bufferedStream.Flush();
                                    goto IL_177;
                                }
                                break;
                            case NbtCompression.ZLib:
                                {
                                    stream.WriteByte(120);
                                    stream.WriteByte(1);
                                    int checksum;
                                    using (ZLibStream zlibStream = new ZLibStream(stream, CompressionMode.Compress, true))
                                    {
                                        BufferedStream bufferedStream2 = new BufferedStream(zlibStream, 8192);
                                        RootTag.WriteTag(new NbtBinaryWriter(bufferedStream2, BigEndian));
                                        bufferedStream2.Flush();
                                        checksum = zlibStream.Checksum;
                                    }
                                    byte[] bytes = BitConverter.GetBytes(checksum);
                                    if (BitConverter.IsLittleEndian)
                                    {
                                        Array.Reverse(bytes);
                                    }
                                    stream.Write(bytes, 0, bytes.Length);
                                    goto IL_177;
                                }
                            default:
                                throw new ArgumentOutOfRangeException("compression");
                        }
                        NbtBinaryWriter writeReader = new NbtBinaryWriter(stream, BigEndian);
                        RootTag.WriteTag(writeReader);
                    IL_177:
                        if (stream.CanSeek)
                        {
                            return stream.Position - num;
                        }
                        return ((ByteCountingStream)stream).BytesWritten;
                    }
                default:
                    throw new ArgumentOutOfRangeException("compression");
            }
        }
        [NotNull]
        public static string ReadRootTagName([NotNull] string fileName)
        {
            return NbtFile.ReadRootTagName(fileName, NbtCompression.AutoDetect, NbtFile.BigEndianByDefault, NbtFile.defaultBufferSize);
        }
        [NotNull]
        public static string ReadRootTagName([NotNull] string fileName, NbtCompression compression, bool bigEndian, int bufferSize)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Could not find the given NBT file.", fileName);
            }
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "DefaultBufferSize cannot be negative.");
            }
            string result;
            using (FileStream fileStream = File.OpenRead(fileName))
            {
                result = NbtFile.ReadRootTagName(fileStream, compression, bigEndian, bufferSize);
            }
            return result;
        }
        [NotNull]
        public static string ReadRootTagName([NotNull] Stream stream, NbtCompression compression, bool bigEndian, int bufferSize)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "DefaultBufferSize cannot be negative.");
            }
            if (compression == NbtCompression.AutoDetect)
            {
                compression = NbtFile.DetectCompression(stream);
            }
            switch (compression)
            {
                case NbtCompression.None:
                    break;
                case NbtCompression.GZip:
                    using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress, true))
                    {
                        if (bufferSize > 0)
                        {
                            return NbtFile.GetRootNameInternal(new BufferedStream(gzipStream, bufferSize), bigEndian);
                        }
                        return NbtFile.GetRootNameInternal(gzipStream, bigEndian);
                    }
                    break;
                case NbtCompression.ZLib:
                    if (stream.ReadByte() != 120)
                    {
                        throw new InvalidDataException("Unrecognized ZLib header. Expected 0x78");
                    }
                    stream.ReadByte();
                    using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress, true))
                    {
                        if (bufferSize > 0)
                        {
                            return NbtFile.GetRootNameInternal(new BufferedStream(deflateStream, bufferSize), bigEndian);
                        }
                        return NbtFile.GetRootNameInternal(deflateStream, bigEndian);
                    }
                    goto IL_D4;
                default:
                    goto IL_D4;
            }
            return NbtFile.GetRootNameInternal(stream, bigEndian);
        IL_D4:
            throw new ArgumentOutOfRangeException("compression");
        }
        [NotNull]
        private static string GetRootNameInternal([NotNull] Stream stream, bool bigEndian)
        {
            int num = stream.ReadByte();
            if (num < 0)
            {
                throw new EndOfStreamException();
            }
            if (num != 10)
            {
                throw new NbtFormatException("Given NBT stream does not start with a TAG_Compound");
            }
            NbtBinaryReader nbtBinaryReader = new NbtBinaryReader(stream, bigEndian);
            return nbtBinaryReader.ReadString();
        }
        public override string ToString()
        {
            return RootTag.ToString(NbtTag.DefaultIndentString);
        }
        [NotNull]
        public string ToString([NotNull] string indentString)
        {
            return RootTag.ToString(indentString);
        }
        private const int WriteBufferSize = 8192;
        private const int FileStreamBufferSize = 65536;
        private const string WrongZLibHeaderMessage = "Unrecognized ZLib header. Expected 0x78";
        [NotNull]
        private NbtCompound rootTag;
        private static int defaultBufferSize = 8192;
        private int bufferSize;
    }
}
