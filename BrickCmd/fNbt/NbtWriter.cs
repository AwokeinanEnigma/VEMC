using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;

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
            writer = new NbtBinaryWriter(stream, bigEndian);
            writer.Write(10);
            writer.Write(rootTagName);
            parentType = NbtTagType.Compound;
        }
        public bool IsDone { get; private set; }
        [NotNull]
        public Stream BaseStream => writer.BaseStream;
        public void BeginCompound()
        {
            EnforceConstraints(null, NbtTagType.Compound);
            GoDown(NbtTagType.Compound);
        }
        public void BeginCompound([NotNull] string tagName)
        {
            EnforceConstraints(tagName, NbtTagType.Compound);
            GoDown(NbtTagType.Compound);
            writer.Write(10);
            writer.Write(tagName);
        }
        public void EndCompound()
        {
            if (IsDone || parentType != NbtTagType.Compound)
            {
                throw new NbtFormatException("Not currently in a compound.");
            }
            GoUp();
            writer.Write(NbtTagType.End);
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
            EnforceConstraints(null, NbtTagType.List);
            GoDown(NbtTagType.List);
            listType = elementType;
            listSize = size;
            writer.Write((byte)elementType);
            writer.Write(size);
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
            EnforceConstraints(tagName, NbtTagType.List);
            GoDown(NbtTagType.List);
            listType = elementType;
            listSize = size;
            writer.Write(9);
            writer.Write(tagName);
            writer.Write((byte)elementType);
            writer.Write(size);
        }
        public void EndList()
        {
            if (parentType != NbtTagType.List || IsDone)
            {
                throw new NbtFormatException("Not currently in a list.");
            }
            if (listIndex < listSize)
            {
                throw new NbtFormatException(string.Concat(new object[]
                {
                    "Cannot end list: not all list elements have been written yet. Expected: ",
                    listSize,
                    ", written: ",
                    listIndex
                }));
            }
            GoUp();
        }
        public void WriteByte(byte value)
        {
            EnforceConstraints(null, NbtTagType.Byte);
            writer.Write(value);
        }
        public void WriteByte([NotNull] string tagName, byte value)
        {
            EnforceConstraints(tagName, NbtTagType.Byte);
            writer.Write(1);
            writer.Write(tagName);
            writer.Write(value);
        }
        public void WriteDouble(double value)
        {
            EnforceConstraints(null, NbtTagType.Double);
            writer.Write(value);
        }
        public void WriteDouble([NotNull] string tagName, double value)
        {
            EnforceConstraints(tagName, NbtTagType.Double);
            writer.Write(6);
            writer.Write(tagName);
            writer.Write(value);
        }
        public void WriteFloat(float value)
        {
            EnforceConstraints(null, NbtTagType.Float);
            writer.Write(value);
        }
        public void WriteFloat([NotNull] string tagName, float value)
        {
            EnforceConstraints(tagName, NbtTagType.Float);
            writer.Write(5);
            writer.Write(tagName);
            writer.Write(value);
        }
        public void WriteInt(int value)
        {
            EnforceConstraints(null, NbtTagType.Int);
            writer.Write(value);
        }
        public void WriteInt([NotNull] string tagName, int value)
        {
            EnforceConstraints(tagName, NbtTagType.Int);
            writer.Write(3);
            writer.Write(tagName);
            writer.Write(value);
        }
        public void WriteLong(long value)
        {
            EnforceConstraints(null, NbtTagType.Long);
            writer.Write(value);
        }
        public void WriteLong([NotNull] string tagName, long value)
        {
            EnforceConstraints(tagName, NbtTagType.Long);
            writer.Write(4);
            writer.Write(tagName);
            writer.Write(value);
        }
        public void WriteShort(short value)
        {
            EnforceConstraints(null, NbtTagType.Short);
            writer.Write(value);
        }
        public void WriteShort([NotNull] string tagName, short value)
        {
            EnforceConstraints(tagName, NbtTagType.Short);
            writer.Write(2);
            writer.Write(tagName);
            writer.Write(value);
        }
        public void WriteString([NotNull] string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            EnforceConstraints(null, NbtTagType.String);
            writer.Write(value);
        }
        public void WriteString([NotNull] string tagName, [NotNull] string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            EnforceConstraints(tagName, NbtTagType.String);
            writer.Write(8);
            writer.Write(tagName);
            writer.Write(value);
        }
        public void WriteByteArray([NotNull] byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            WriteByteArray(data, 0, data.Length);
        }
        public void WriteByteArray([NotNull] byte[] data, int offset, int count)
        {
            NbtWriter.CheckArray(data, offset, count);
            EnforceConstraints(null, NbtTagType.ByteArray);
            writer.Write(count);
            writer.Write(data, offset, count);
        }
        public void WriteByteArray([NotNull] string tagName, [NotNull] byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            WriteByteArray(tagName, data, 0, data.Length);
        }
        public void WriteByteArray([NotNull] string tagName, [NotNull] byte[] data, int offset, int count)
        {
            NbtWriter.CheckArray(data, offset, count);
            EnforceConstraints(tagName, NbtTagType.ByteArray);
            writer.Write(7);
            writer.Write(tagName);
            writer.Write(count);
            writer.Write(data, offset, count);
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
            WriteByteArray(dataSource, count, buffer);
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
            EnforceConstraints(null, NbtTagType.ByteArray);
            WriteByteArrayFromStreamImpl(dataSource, count, buffer);
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
            WriteByteArray(tagName, dataSource, count, buffer);
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
            EnforceConstraints(tagName, NbtTagType.ByteArray);
            writer.Write(7);
            writer.Write(tagName);
            WriteByteArrayFromStreamImpl(dataSource, count, buffer);
        }
        public void WriteIntArray([NotNull] int[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            WriteIntArray(data, 0, data.Length);
        }
        public void WriteIntArray([NotNull] int[] data, int offset, int count)
        {
            NbtWriter.CheckArray(data, offset, count);
            EnforceConstraints(null, NbtTagType.IntArray);
            writer.Write(count);
            for (int i = offset; i < count; i++)
            {
                writer.Write(data[i]);
            }
        }
        public void WriteIntArray([NotNull] string tagName, [NotNull] int[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            WriteIntArray(tagName, data, 0, data.Length);
        }
        public void WriteIntArray([NotNull] string tagName, [NotNull] int[] data, int offset, int count)
        {
            NbtWriter.CheckArray(data, offset, count);
            EnforceConstraints(tagName, NbtTagType.IntArray);
            writer.Write(11);
            writer.Write(tagName);
            writer.Write(count);
            for (int i = offset; i < count; i++)
            {
                writer.Write(data[i]);
            }
        }
        public void WriteTag([NotNull] NbtTag tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }
            EnforceConstraints(tag.Name, tag.TagType);
            if (tag.Name != null)
            {
                tag.WriteTag(writer);
                return;
            }
            tag.WriteData(writer);
        }
        public void Finish()
        {
            if (!IsDone)
            {
                throw new NbtFormatException("Cannot finish: not all tags have been closed yet.");
            }
        }
        private void GoDown(NbtTagType thisType)
        {
            if (nodes == null)
            {
                nodes = new Stack<NbtWriterNode>();
            }
            NbtWriterNode item = new NbtWriterNode
            {
                ParentType = parentType,
                ListType = listType,
                ListSize = listSize,
                ListIndex = listIndex
            };
            nodes.Push(item);
            parentType = thisType;
            listType = NbtTagType.Unknown;
            listSize = 0;
            listIndex = 0;
        }
        private void GoUp()
        {
            if (nodes == null || nodes.Count == 0)
            {
                IsDone = true;
                return;
            }
            NbtWriterNode nbtWriterNode = nodes.Pop();
            parentType = nbtWriterNode.ParentType;
            listType = nbtWriterNode.ListType;
            listSize = nbtWriterNode.ListSize;
            listIndex = nbtWriterNode.ListIndex;
        }
        private void EnforceConstraints([CanBeNull] string name, NbtTagType desiredType)
        {
            if (IsDone)
            {
                throw new NbtFormatException("Cannot write any more tags: root tag has been closed.");
            }
            if (parentType == NbtTagType.List)
            {
                if (name != null)
                {
                    throw new NbtFormatException("Expecting an unnamed tag.");
                }
                if (listType != desiredType)
                {
                    throw new NbtFormatException(string.Concat(new object[]
                    {
                        "Unexpected tag type (expected: ",
                        listType,
                        ", given: ",
                        desiredType
                    }));
                }
                if (listIndex >= listSize)
                {
                    throw new NbtFormatException("Given list size exceeded.");
                }
                listIndex++;
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
            writer.Write(count);
            int val = Math.Min(buffer.Length, 536870912);
            int num;
            for (int i = 0; i < count; i += num)
            {
                int count2 = Math.Min(count - i, val);
                num = dataSource.Read(buffer, 0, count2);
                writer.Write(buffer, 0, num);
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
