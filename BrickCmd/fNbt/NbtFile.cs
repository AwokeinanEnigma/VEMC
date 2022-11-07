using System;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x0200000A RID: 10
	public sealed class NbtFile
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000033 RID: 51 RVA: 0x00002A61 File Offset: 0x00000C61
		// (set) Token: 0x06000034 RID: 52 RVA: 0x00002A69 File Offset: 0x00000C69
		[CanBeNull]
		public string FileName { get; private set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000035 RID: 53 RVA: 0x00002A72 File Offset: 0x00000C72
		// (set) Token: 0x06000036 RID: 54 RVA: 0x00002A7A File Offset: 0x00000C7A
		public NbtCompression FileCompression { get; private set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000037 RID: 55 RVA: 0x00002A83 File Offset: 0x00000C83
		// (set) Token: 0x06000038 RID: 56 RVA: 0x00002A8B File Offset: 0x00000C8B
		[NotNull]
		public NbtCompound RootTag
		{
			get
			{
				return this.rootTag;
			}
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
				this.rootTag = value;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000039 RID: 57 RVA: 0x00002AB5 File Offset: 0x00000CB5
		// (set) Token: 0x0600003A RID: 58 RVA: 0x00002ABC File Offset: 0x00000CBC
		public static bool BigEndianByDefault { get; set; } = true;

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600003B RID: 59 RVA: 0x00002AC4 File Offset: 0x00000CC4
		// (set) Token: 0x0600003C RID: 60 RVA: 0x00002ACC File Offset: 0x00000CCC
		public bool BigEndian { get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00002AD5 File Offset: 0x00000CD5
		// (set) Token: 0x0600003E RID: 62 RVA: 0x00002ADC File Offset: 0x00000CDC
		public static int DefaultBufferSize
		{
			get
			{
				return NbtFile.defaultBufferSize;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value", value, "DefaultBufferSize cannot be negative.");
				}
				NbtFile.defaultBufferSize = value;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600003F RID: 63 RVA: 0x00002AFE File Offset: 0x00000CFE
		// (set) Token: 0x06000040 RID: 64 RVA: 0x00002B06 File Offset: 0x00000D06
		public int BufferSize
		{
			get
			{
				return this.bufferSize;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value", value, "BufferSize cannot be negative.");
				}
				this.bufferSize = value;
			}
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00002B3B File Offset: 0x00000D3B
		public NbtFile()
		{
			this.BigEndian = NbtFile.BigEndianByDefault;
			this.BufferSize = NbtFile.DefaultBufferSize;
			this.rootTag = new NbtCompound("");
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00002B69 File Offset: 0x00000D69
		public NbtFile([NotNull] NbtCompound rootTag) : this()
		{
			if (rootTag == null)
			{
				throw new ArgumentNullException("rootTag");
			}
			this.RootTag = rootTag;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00002B86 File Offset: 0x00000D86
		public NbtFile([NotNull] string fileName) : this()
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			this.LoadFromFile(fileName, NbtCompression.AutoDetect, null);
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00002BA6 File Offset: 0x00000DA6
		public long LoadFromFile([NotNull] string fileName)
		{
			return this.LoadFromFile(fileName, NbtCompression.AutoDetect, null);
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00002BB4 File Offset: 0x00000DB4
		public long LoadFromFile([NotNull] string fileName, NbtCompression compression, [CanBeNull] TagSelector selector)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			long position;
			using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.SequentialScan))
			{
				this.LoadFromStream(fileStream, compression, selector);
				this.FileName = fileName;
				position = fileStream.Position;
			}
			return position;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00002C18 File Offset: 0x00000E18
		public long LoadFromBuffer([NotNull] byte[] buffer, int index, int length, NbtCompression compression, [CanBeNull] TagSelector selector)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			long position;
			using (MemoryStream memoryStream = new MemoryStream(buffer, index, length))
			{
				this.LoadFromStream(memoryStream, compression, selector);
				this.FileName = null;
				position = memoryStream.Position;
			}
			return position;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00002C74 File Offset: 0x00000E74
		public long LoadFromBuffer([NotNull] byte[] buffer, int index, int length, NbtCompression compression)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			long position;
			using (MemoryStream memoryStream = new MemoryStream(buffer, index, length))
			{
				this.LoadFromStream(memoryStream, compression, null);
				this.FileName = null;
				position = memoryStream.Position;
			}
			return position;
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00002CD0 File Offset: 0x00000ED0
		public long LoadFromStream([NotNull] Stream stream, NbtCompression compression, [CanBeNull] TagSelector selector)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			this.FileName = null;
			if (compression == NbtCompression.AutoDetect)
			{
				this.FileCompression = NbtFile.DetectCompression(stream);
			}
			else
			{
				this.FileCompression = compression;
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
			switch (this.FileCompression)
			{
			case NbtCompression.None:
				break;
			case NbtCompression.GZip:
				using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress, true))
				{
					if (this.bufferSize > 0)
					{
						this.LoadFromStreamInternal(new BufferedStream(gzipStream, this.bufferSize), selector);
					}
					else
					{
						this.LoadFromStreamInternal(gzipStream, selector);
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
					if (this.bufferSize > 0)
					{
						this.LoadFromStreamInternal(new BufferedStream(deflateStream, this.bufferSize), selector);
					}
					else
					{
						this.LoadFromStreamInternal(deflateStream, selector);
					}
					goto IL_110;
				}
				goto IL_105;
			default:
				goto IL_105;
			}
			this.LoadFromStreamInternal(stream, selector);
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

		// Token: 0x0600004A RID: 74 RVA: 0x00002E28 File Offset: 0x00001028
		public long LoadFromStream([NotNull] Stream stream, NbtCompression compression)
		{
			return this.LoadFromStream(stream, compression, null);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00002E34 File Offset: 0x00001034
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

		// Token: 0x0600004C RID: 76 RVA: 0x00002EA4 File Offset: 0x000010A4
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
			NbtBinaryReader nbtBinaryReader = new NbtBinaryReader(stream, this.BigEndian)
			{
				Selector = tagSelector
			};
			NbtCompound nbtCompound = new NbtCompound(nbtBinaryReader.ReadString());
			nbtCompound.ReadTag(nbtBinaryReader);
			this.RootTag = nbtCompound;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00002F04 File Offset: 0x00001104
		public long SaveToFile([NotNull] string fileName, NbtCompression compression)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			long result;
			using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 65536, FileOptions.SequentialScan))
			{
				result = this.SaveToStream(fileStream, compression);
			}
			return result;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00002F5C File Offset: 0x0000115C
		public long SaveToBuffer([NotNull] byte[] buffer, int index, NbtCompression compression)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			long result;
			using (MemoryStream memoryStream = new MemoryStream(buffer, index, buffer.Length - index))
			{
				result = this.SaveToStream(memoryStream, compression);
			}
			return result;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00002FAC File Offset: 0x000011AC
		[NotNull]
		public byte[] SaveToBuffer(NbtCompression compression)
		{
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				this.SaveToStream(memoryStream, compression);
				result = memoryStream.ToArray();
			}
			return result;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00002FEC File Offset: 0x000011EC
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
				if (this.rootTag.Name == null)
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
						this.RootTag.WriteTag(new NbtBinaryWriter(bufferedStream, this.BigEndian));
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
						this.RootTag.WriteTag(new NbtBinaryWriter(bufferedStream2, this.BigEndian));
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
				NbtBinaryWriter writeReader = new NbtBinaryWriter(stream, this.BigEndian);
				this.RootTag.WriteTag(writeReader);
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

		// Token: 0x06000051 RID: 81 RVA: 0x000031A8 File Offset: 0x000013A8
		[NotNull]
		public static string ReadRootTagName([NotNull] string fileName)
		{
			return NbtFile.ReadRootTagName(fileName, NbtCompression.AutoDetect, NbtFile.BigEndianByDefault, NbtFile.defaultBufferSize);
		}

		// Token: 0x06000052 RID: 82 RVA: 0x000031BC File Offset: 0x000013BC
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

		// Token: 0x06000053 RID: 83 RVA: 0x00003234 File Offset: 0x00001434
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

		// Token: 0x06000054 RID: 84 RVA: 0x00003340 File Offset: 0x00001540
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

		// Token: 0x06000055 RID: 85 RVA: 0x0000337C File Offset: 0x0000157C
		public override string ToString()
		{
			return this.RootTag.ToString(NbtTag.DefaultIndentString);
		}

		// Token: 0x06000056 RID: 86 RVA: 0x0000338E File Offset: 0x0000158E
		[NotNull]
		public string ToString([NotNull] string indentString)
		{
			return this.RootTag.ToString(indentString);
		}

		// Token: 0x0400001B RID: 27
		private const int WriteBufferSize = 8192;

		// Token: 0x0400001C RID: 28
		private const int FileStreamBufferSize = 65536;

		// Token: 0x0400001D RID: 29
		private const string WrongZLibHeaderMessage = "Unrecognized ZLib header. Expected 0x78";

		// Token: 0x0400001E RID: 30
		[NotNull]
		private NbtCompound rootTag;

		// Token: 0x0400001F RID: 31
		private static int defaultBufferSize = 8192;

		// Token: 0x04000020 RID: 32
		private int bufferSize;
	}
}
