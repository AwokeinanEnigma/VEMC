using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace fNbt
{
	public sealed class NbtWriter
	{
		public NbtWriter([NotNull] Stream stream, [NotNull] string rootTagName) : this(stream, rootTagName, true)
		{
		}
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
		public bool IsDone { get; private set; }
		[NotNull]
		public Stream BaseStream
		{
			get
			{
				return this.writer.BaseStream;
			}
		}
		public void BeginCompound()
		{
			this.EnforceConstraints(null, NbtTagType.Compound);
			this.GoDown(NbtTagType.Compound);
		}
		public void BeginCompound([NotNull] string tagName)
		{
			this.EnforceConstraints(tagName, NbtTagType.Compound);
			this.GoDown(NbtTagType.Compound);
			this.writer.Write(10);
			this.writer.Write(tagName);
		}
		public void EndCompound()
		{
			if (this.IsDone || this.parentType != NbtTagType.Compound)
			{
				throw new NbtFormatException("Not currently in a compound.");
			}
			this.GoUp();
			this.writer.Write(NbtTagType.End);
		}
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
		public void WriteByte(byte value)
		{
			this.EnforceConstraints(null, NbtTagType.Byte);
			this.writer.Write(value);
		}
		public void WriteByte([NotNull] string tagName, byte value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Byte);
			this.writer.Write(1);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}
		public void WriteDouble(double value)
		{
			this.EnforceConstraints(null, NbtTagType.Double);
			this.writer.Write(value);
		}
		public void WriteDouble([NotNull] string tagName, double value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Double);
			this.writer.Write(6);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}
		public void WriteFloat(float value)
		{
			this.EnforceConstraints(null, NbtTagType.Float);
			this.writer.Write(value);
		}
		public void WriteFloat([NotNull] string tagName, float value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Float);
			this.writer.Write(5);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}
		public void WriteInt(int value)
		{
			this.EnforceConstraints(null, NbtTagType.Int);
			this.writer.Write(value);
		}
		public void WriteInt([NotNull] string tagName, int value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Int);
			this.writer.Write(3);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}
		public void WriteLong(long value)
		{
			this.EnforceConstraints(null, NbtTagType.Long);
			this.writer.Write(value);
		}
		public void WriteLong([NotNull] string tagName, long value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Long);
			this.writer.Write(4);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}
		public void WriteShort(short value)
		{
			this.EnforceConstraints(null, NbtTagType.Short);
			this.writer.Write(value);
		}
		public void WriteShort([NotNull] string tagName, short value)
		{
			this.EnforceConstraints(tagName, NbtTagType.Short);
			this.writer.Write(2);
			this.writer.Write(tagName);
			this.writer.Write(value);
		}
		public void WriteString([NotNull] string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			this.EnforceConstraints(null, NbtTagType.String);
			this.writer.Write(value);
		}
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
		public void WriteByteArray([NotNull] byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.WriteByteArray(data, 0, data.Length);
		}
		public void WriteByteArray([NotNull] byte[] data, int offset, int count)
		{
			NbtWriter.CheckArray(data, offset, count);
			this.EnforceConstraints(null, NbtTagType.ByteArray);
			this.writer.Write(count);
			this.writer.Write(data, offset, count);
		}
		public void WriteByteArray([NotNull] string tagName, [NotNull] byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.WriteByteArray(tagName, data, 0, data.Length);
		}
		public void WriteByteArray([NotNull] string tagName, [NotNull] byte[] data, int offset, int count)
		{
			NbtWriter.CheckArray(data, offset, count);
			this.EnforceConstraints(tagName, NbtTagType.ByteArray);
			this.writer.Write(7);
			this.writer.Write(tagName);
			this.writer.Write(count);
			this.writer.Write(data, offset, count);
		}
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
		public void WriteIntArray([NotNull] int[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.WriteIntArray(data, 0, data.Length);
		}
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
		public void WriteIntArray([NotNull] string tagName, [NotNull] int[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.WriteIntArray(tagName, data, 0, data.Length);
		}
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
		public void Finish()
		{
			if (!this.IsDone)
			{
				throw new NbtFormatException("Cannot finish: not all tags have been closed yet.");
			}
		}
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
		private const int MaxStreamCopyBufferSize = 8192;
		private readonly NbtBinaryWriter writer;
		private NbtTagType listType;
		private NbtTagType parentType;
		private int listIndex;
		private int listSize;
		private Stack<NbtWriterNode> nodes;
	}
}
