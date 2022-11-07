using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x02000010 RID: 16
	public sealed class NbtWriter
	{
		// Token: 0x06000093 RID: 147 RVA: 0x0000459A File Offset: 0x0000279A
		public NbtWriter([NotNull] Stream stream, [NotNull] string rootTagName) : this(stream, rootTagName, true)
		{
		}

		// Token: 0x06000094 RID: 148 RVA: 0x000045A8 File Offset: 0x000027A8
		public NbtWriter([NotNull] Stream stream, [NotNull] string rootTagName, bool bigEndian)
		{
			if (rootTagName == null)
			{
				throw new ArgumentNullException("rootTagName");
			}
			this.writer = new NbtBinaryWriter(stream, bigEndian);
			this.writer.Write(10);
			this.writer.Write(rootTagName);
			this.parentType = NbtTagType.Compound;
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000095 RID: 149 RVA: 0x000045F7 File Offset: 0x000027F7
		// (set) Token: 0x06000096 RID: 150 RVA: 0x000045FF File Offset: 0x000027FF
		public bool IsDone { get; private set; }

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000097 RID: 151 RVA: 0x00004608 File Offset: 0x00002808
		[NotNull]
		public Stream BaseStream
		{
			get
			{
				return this.writer.BaseStream;
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00004615 File Offset: 0x00002815
		public void BeginCompound()
		{
			this.EnforceConstraints(null, NbtTagType.Compound);
			this.GoDown(NbtTagType.Compound);
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00004628 File Offset: 0x00002828
		public void BeginCompound([NotNull] string tagName)
		{
			this.EnforceConstraints(tagName, NbtTagType.Compound);
			this.GoDown(NbtTagType.Compound);
			this.writer.Write(10);
			this.writer.Write(tagName);
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00004654 File Offset: 0x00002854
		public void EndCompound()
		{
			if (this.IsDone || this.parentType != NbtTagType.Compound)
			{
				throw new NbtFormatException("Not currently in a compound.");
			}
			this.GoUp();
			this.writer.Write(NbtTagType.End);
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00004688 File Offset: 0x00002888
		public void BeginList(NbtTagType elementType, int size)
		{
			if (size < 0)
			{
				throw new ArgumentOutOfRangeException("size", "List size may not be negative.");
			}
			if (elementType < NbtTagType.Byte || elementType > NbtTagType.IntArray)
			{
				throw new ArgumentOutOfRangeException("elementType");
			}
			this.EnforceConstraints(null, NbtTagType.List);
			this.GoDown(NbtTagType.List);
			this.listType = elementType;
			this.listSize = size;
			this.writer.Write((byte)elementType);
			this.writer.Write(size);
		}

		// Token: 0x0600009C RID: 156 RVA: 0x000046F4 File Offset: 0x000028F4
		public void BeginList([NotNull] string tagName, NbtTagType elementType, int size)
		{
			if (size < 0)
			{
				throw new ArgumentOutOfRangeException("size", "List size may not be negative.");
			}
			if (elementType < NbtTagType.Byte || elementType > NbtTagType.IntArray)
			{
				throw new ArgumentOutOfRangeException("elementType");
			}
			this.EnforceConstraints(tagName, NbtTagType.List);
			this.GoDown(NbtTagType.List);
			this.listType = elementType;
			this.listSize = size;
			this.writer.Write(9);
			this.writer.Write(tagName);
			this.writer.Write((byte)elementType);
			this.writer.Write(size);
		}

		// Token: 0x0600009D RID: 157 RVA: 0x0000477C File Offset: 0x0000297C
		public void EndList()
		{
			if (this.parentType != NbtTagType.List || this.IsDone)
			{
				throw new NbtFormatException("Not currently in a list.");
			}
			if (this.listIndex < this.listSize)
			{
				throw new NbtFormatException(string.Concat(new object[]
				{
					"Cannot end list: not all list elements have been written yet. Expected: ",
					this.listSize,
					", written: ",
					this.listIndex
				}));
			}
			this.GoUp();
		}

		// Token: 0x0600009E RID: 158 RVA: 0x000047F9 File Offset: 0x000029F9
		public void WriteByte(byte value)
		{
			this.EnforceConstraints(null, NbtTagType.Byte);
			this.writer.Write(value);
		}

		// Token: 0x0600009F RID: 159 RVA: 0x0000480F File Offset: 0x00002A0F
		public void WriteByte([NotNull] string tagName, byte value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Byte);
			this.writer.Write(1);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x0000483D File Offset: 0x00002A3D
		public void WriteDouble(double value)
		{
			this.EnforceConstraints(null, NbtTagType.Double);
			this.writer.Write(value);
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00004853 File Offset: 0x00002A53
		public void WriteDouble([NotNull] string tagName, double value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Double);
			this.writer.Write(6);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00004881 File Offset: 0x00002A81
		public void WriteFloat(float value)
		{
			this.EnforceConstraints(null, NbtTagType.Float);
			this.writer.Write(value);
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00004897 File Offset: 0x00002A97
		public void WriteFloat([NotNull] string tagName, float value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Float);
			this.writer.Write(5);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x000048C5 File Offset: 0x00002AC5
		public void WriteInt(int value)
		{
			this.EnforceConstraints(null, NbtTagType.Int);
			this.writer.Write(value);
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x000048DB File Offset: 0x00002ADB
		public void WriteInt([NotNull] string tagName, int value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Int);
			this.writer.Write(3);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00004909 File Offset: 0x00002B09
		public void WriteLong(long value)
		{
			this.EnforceConstraints(null, NbtTagType.Long);
			this.writer.Write(value);
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x0000491F File Offset: 0x00002B1F
		public void WriteLong([NotNull] string tagName, long value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Long);
			this.writer.Write(4);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x0000494D File Offset: 0x00002B4D
		public void WriteShort(short value)
		{
			this.EnforceConstraints(null, NbtTagType.Short);
			this.writer.Write(value);
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00004963 File Offset: 0x00002B63
		public void WriteShort([NotNull] string tagName, short value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Short);
			this.writer.Write(2);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00004991 File Offset: 0x00002B91
		public void WriteString([NotNull] string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			this.EnforceConstraints(null, NbtTagType.String);
			this.writer.Write(value);
		}

		// Token: 0x060000AB RID: 171 RVA: 0x000049B5 File Offset: 0x00002BB5
		public void WriteString([NotNull] string tagName, [NotNull] string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			this.EnforceConstraints(tagName, NbtTagType.String);
			this.writer.Write(8);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}

		// Token: 0x060000AC RID: 172 RVA: 0x000049F1 File Offset: 0x00002BF1
		public void WriteByteArray([NotNull] byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.WriteByteArray(data, 0, data.Length);
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00004A0C File Offset: 0x00002C0C
		public void WriteByteArray([NotNull] byte[] data, int offset, int count)
		{
			NbtWriter.CheckArray(data, offset, count);
			this.EnforceConstraints(null, NbtTagType.ByteArray);
			this.writer.Write(count);
			this.writer.Write(data, offset, count);
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00004A38 File Offset: 0x00002C38
		public void WriteByteArray([NotNull] string tagName, [NotNull] byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.WriteByteArray(tagName, data, 0, data.Length);
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00004A54 File Offset: 0x00002C54
		public void WriteByteArray([NotNull] string tagName, [NotNull] byte[] data, int offset, int count)
		{
			NbtWriter.CheckArray(data, offset, count);
			this.EnforceConstraints(tagName, NbtTagType.ByteArray);
			this.writer.Write(7);
			this.writer.Write(tagName);
			this.writer.Write(count);
			this.writer.Write(data, offset, count);
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00004AA8 File Offset: 0x00002CA8
		public void WriteByteArray([NotNull] Stream dataSource, int count)
		{
			if (dataSource == null)
			{
				throw new ArgumentNullException("dataSource");
			}
			if (!dataSource.CanRead)
			{
				throw new ArgumentException("Given stream does not support reading.", "dataSource");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "count may not be negative");
			}
			int num = Math.Min(count, 8192);
			byte[] buffer = new byte[num];
			this.WriteByteArray(dataSource, count, buffer);
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00004B0C File Offset: 0x00002D0C
		public void WriteByteArray([NotNull] Stream dataSource, int count, [NotNull] byte[] buffer)
		{
			if (dataSource == null)
			{
				throw new ArgumentNullException("dataSource");
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (!dataSource.CanRead)
			{
				throw new ArgumentException("Given stream does not support reading.", "dataSource");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "count may not be negative");
			}
			if (buffer.Length == 0 && count > 0)
			{
				throw new ArgumentException("buffer size must be greater than 0 when count is greater than 0", "buffer");
			}
			this.EnforceConstraints(null, NbtTagType.ByteArray);
			this.WriteByteArrayFromStreamImpl(dataSource, count, buffer);
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x00004B8C File Offset: 0x00002D8C
		public void WriteByteArray([NotNull] string tagName, [NotNull] Stream dataSource, int count)
		{
			if (dataSource == null)
			{
				throw new ArgumentNullException("dataSource");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "count may not be negative");
			}
			int num = Math.Min(count, 8192);
			byte[] buffer = new byte[num];
			this.WriteByteArray(tagName, dataSource, count, buffer);
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x00004BD8 File Offset: 0x00002DD8
		public void WriteByteArray([NotNull] string tagName, [NotNull] Stream dataSource, int count, [NotNull] byte[] buffer)
		{
			if (dataSource == null)
			{
				throw new ArgumentNullException("dataSource");
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (!dataSource.CanRead)
			{
				throw new ArgumentException("Given stream does not support reading.", "dataSource");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "count may not be negative");
			}
			if (buffer.Length == 0 && count > 0)
			{
				throw new ArgumentException("buffer size must be greater than 0 when count is greater than 0", "buffer");
			}
			this.EnforceConstraints(tagName, NbtTagType.ByteArray);
			this.writer.Write(7);
			this.writer.Write(tagName);
			this.WriteByteArrayFromStreamImpl(dataSource, count, buffer);
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x00004C72 File Offset: 0x00002E72
		public void WriteIntArray([NotNull] int[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.WriteIntArray(data, 0, data.Length);
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00004C90 File Offset: 0x00002E90
		public void WriteIntArray([NotNull] int[] data, int offset, int count)
		{
			NbtWriter.CheckArray(data, offset, count);
			this.EnforceConstraints(null, NbtTagType.IntArray);
			this.writer.Write(count);
			for (int i = offset; i < count; i++)
			{
				this.writer.Write(data[i]);
			}
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00004CD4 File Offset: 0x00002ED4
		public void WriteIntArray([NotNull] string tagName, [NotNull] int[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.WriteIntArray(tagName, data, 0, data.Length);
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00004CF0 File Offset: 0x00002EF0
		public void WriteIntArray([NotNull] string tagName, [NotNull] int[] data, int offset, int count)
		{
			NbtWriter.CheckArray(data, offset, count);
			this.EnforceConstraints(tagName, NbtTagType.IntArray);
			this.writer.Write(11);
			this.writer.Write(tagName);
			this.writer.Write(count);
			for (int i = offset; i < count; i++)
			{
				this.writer.Write(data[i]);
			}
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00004D50 File Offset: 0x00002F50
		public void WriteTag([NotNull] NbtTag tag)
		{
			if (tag == null)
			{
				throw new ArgumentNullException("tag");
			}
			this.EnforceConstraints(tag.Name, tag.TagType);
			if (tag.Name != null)
			{
				tag.WriteTag(this.writer);
				return;
			}
			tag.WriteData(this.writer);
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00004D9E File Offset: 0x00002F9E
		public void Finish()
		{
			if (!this.IsDone)
			{
				throw new NbtFormatException("Cannot finish: not all tags have been closed yet.");
			}
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00004DB4 File Offset: 0x00002FB4
		private void GoDown(NbtTagType thisType)
		{
			if (this.nodes == null)
			{
				this.nodes = new Stack<NbtWriterNode>();
			}
			NbtWriterNode item = new NbtWriterNode
			{
				ParentType = this.parentType,
				ListType = this.listType,
				ListSize = this.listSize,
				ListIndex = this.listIndex
			};
			this.nodes.Push(item);
			this.parentType = thisType;
			this.listType = NbtTagType.Unknown;
			this.listSize = 0;
			this.listIndex = 0;
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00004E38 File Offset: 0x00003038
		private void GoUp()
		{
			if (this.nodes == null || this.nodes.Count == 0)
			{
				this.IsDone = true;
				return;
			}
			NbtWriterNode nbtWriterNode = this.nodes.Pop();
			this.parentType = nbtWriterNode.ParentType;
			this.listType = nbtWriterNode.ListType;
			this.listSize = nbtWriterNode.ListSize;
			this.listIndex = nbtWriterNode.ListIndex;
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00004EA0 File Offset: 0x000030A0
		private void EnforceConstraints([CanBeNull] string name, NbtTagType desiredType)
		{
			if (this.IsDone)
			{
				throw new NbtFormatException("Cannot write any more tags: root tag has been closed.");
			}
			if (this.parentType == NbtTagType.List)
			{
				if (name != null)
				{
					throw new NbtFormatException("Expecting an unnamed tag.");
				}
				if (this.listType != desiredType)
				{
					throw new NbtFormatException(string.Concat(new object[]
					{
						"Unexpected tag type (expected: ",
						this.listType,
						", given: ",
						desiredType
					}));
				}
				if (this.listIndex >= this.listSize)
				{
					throw new NbtFormatException("Given list size exceeded.");
				}
				this.listIndex++;
				return;
			}
			else
			{
				if (name == null)
				{
					throw new NbtFormatException("Expecting a named tag.");
				}
				return;
			}
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00004F54 File Offset: 0x00003154
		private static void CheckArray([NotNull] Array data, int offset, int count)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "offset may not be negative.");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "count may not be negative.");
			}
			if (data.Length - offset < count)
			{
				throw new ArgumentException("count may not be greater than offset subtracted from the array length.");
			}
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00004FB0 File Offset: 0x000031B0
		private void WriteByteArrayFromStreamImpl([NotNull] Stream dataSource, int count, [NotNull] byte[] buffer)
		{
			this.writer.Write(count);
			int val = Math.Min(buffer.Length, 536870912);
			int num;
			for (int i = 0; i < count; i += num)
			{
				int count2 = Math.Min(count - i, val);
				num = dataSource.Read(buffer, 0, count2);
				this.writer.Write(buffer, 0, num);
			}
		}

		// Token: 0x0400005A RID: 90
		private const int MaxStreamCopyBufferSize = 8192;

		// Token: 0x0400005B RID: 91
		private readonly NbtBinaryWriter writer;

		// Token: 0x0400005C RID: 92
		private NbtTagType listType;

		// Token: 0x0400005D RID: 93
		private NbtTagType parentType;

		// Token: 0x0400005E RID: 94
		private int listIndex;

		// Token: 0x0400005F RID: 95
		private int listSize;

		// Token: 0x04000060 RID: 96
		private Stack<NbtWriterNode> nodes;
	}
}
