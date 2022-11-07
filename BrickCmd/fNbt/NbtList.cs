// Decompiled with JetBrains decompiler

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace fNbt
{
    public sealed class NbtList : NbtTag, IList<NbtTag>, ICollection<NbtTag>, IEnumerable<NbtTag>, IList, ICollection, IEnumerable
    {
        [NotNull]
        private readonly List<NbtTag> tags = new List<NbtTag>();
        private NbtTagType listType;

        public override NbtTagType TagType => NbtTagType.List;

        public NbtTagType ListType
        {
            get => listType;
            set
            {
                if (value == NbtTagType.End)
                {
                    if (tags.Count > 0)
                    {
                        throw new ArgumentException("Only empty list tags may have TagType of End.");
                    }
                }
                else if (value < NbtTagType.Byte || value > NbtTagType.IntArray && value != NbtTagType.Unknown)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (tags.Count > 0)
                {
                    NbtTagType tagType = tags[0].TagType;
                    if (tagType != value)
                    {
                        throw new ArgumentException(string.Format("Given NbtTagType ({0}) does not match actual element type ({1})", value, tagType));
                    }
                }
                listType = value;
            }
        }

        public NbtList()
          : this(null, null, NbtTagType.Unknown)
        {
        }

        public NbtList([CanBeNull] string tagName)
          : this(tagName, null, NbtTagType.Unknown)
        {
        }

        public NbtList([NotNull] IEnumerable<NbtTag> tags)
          : this(null, tags, NbtTagType.Unknown)
        {
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags));
            }
        }

        public NbtList(NbtTagType givenListType)
          : this(null, null, givenListType)
        {
        }

        public NbtList([CanBeNull] string tagName, [NotNull] IEnumerable<NbtTag> tags)
          : this(tagName, tags, NbtTagType.Unknown)
        {
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags));
            }
        }

        public NbtList([NotNull] IEnumerable<NbtTag> tags, NbtTagType givenListType)
          : this(null, tags, givenListType)
        {
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags));
            }
        }

        public NbtList([CanBeNull] string tagName, NbtTagType givenListType)
          : this(tagName, null, givenListType)
        {
        }

        public NbtList([CanBeNull] string tagName, [CanBeNull] IEnumerable<NbtTag> tags, NbtTagType givenListType)
        {
            name = tagName;
            ListType = givenListType;
            if (tags == null)
            {
                return;
            }

            foreach (NbtTag tag in tags)
            {
                Add(tag);
            }
        }

        public NbtList([NotNull] NbtList other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            name = other.name;
            listType = other.listType;
            foreach (NbtTag tag in other.tags)
            {
                tags.Add((NbtTag)tag.Clone());
            }
        }

        [NotNull]
        public override NbtTag this[int tagIndex]
        {
            get => tags[tagIndex];
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Parent != null)
                {
                    throw new ArgumentException("A tag may only be added to one compound/list at a time.");
                }

                if (value == this || value == Parent)
                {
                    throw new ArgumentException("A list tag may not be added to itself or to its child tag.");
                }

                if (value.Name != null)
                {
                    throw new ArgumentException("Named tag given. A list may only contain unnamed tags.");
                }

                if (listType != NbtTagType.Unknown && value.TagType != listType)
                {
                    throw new ArgumentException("Items must be of type " + listType);
                }

                tags[tagIndex] = value;
                value.Parent = this;
            }
        }

        [NotNull]
        [Pure]
        public T Get<T>(int tagIndex) where T : NbtTag
        {
            return (T)tags[tagIndex];
        }

        public void AddRange([NotNull] IEnumerable<NbtTag> newTags)
        {
            if (newTags == null)
            {
                throw new ArgumentNullException(nameof(newTags));
            }

            foreach (NbtTag newTag in newTags)
            {
                Add(newTag);
            }
        }

        [Pure]
        [NotNull]
        public NbtTag[] ToArray()
        {
            return tags.ToArray();
        }

        [NotNull]
        [Pure]
        public T[] ToArray<T>() where T : NbtTag
        {
            T[] objArray = new T[tags.Count];
            for (int index = 0; index < objArray.Length; ++index)
            {
                objArray[index] = (T)tags[index];
            }

            return objArray;
        }

        internal override bool ReadTag(NbtBinaryReader readStream)
        {
            if (readStream.Selector != null && !readStream.Selector(this))
            {
                SkipTag(readStream);
                return false;
            }
            ListType = readStream.ReadTagType();
            int num = readStream.ReadInt32();
            if (num < 0)
            {
                throw new NbtFormatException("Negative list size given.");
            }

            for (int index = 0; index < num; ++index)
            {
                NbtTag nbtTag;
                switch (ListType)
                {
                    case NbtTagType.Byte:
                        nbtTag = new NbtByte();
                        break;
                    case NbtTagType.Short:
                        nbtTag = new NbtShort();
                        break;
                    case NbtTagType.Int:
                        nbtTag = new NbtInt();
                        break;
                    case NbtTagType.Long:
                        nbtTag = new NbtLong();
                        break;
                    case NbtTagType.Float:
                        nbtTag = new NbtFloat();
                        break;
                    case NbtTagType.Double:
                        nbtTag = new NbtDouble();
                        break;
                    case NbtTagType.ByteArray:
                        nbtTag = new NbtByteArray();
                        break;
                    case NbtTagType.String:
                        nbtTag = new NbtString();
                        break;
                    case NbtTagType.List:
                        nbtTag = new NbtList();
                        break;
                    case NbtTagType.Compound:
                        nbtTag = new NbtCompound();
                        break;
                    case NbtTagType.IntArray:
                        nbtTag = new NbtIntArray();
                        break;
                    default:
                        throw new NbtFormatException("Unsupported tag type found in a list: " + ListType);
                }
                nbtTag.Parent = this;
                if (nbtTag.ReadTag(readStream))
                {
                    tags.Add(nbtTag);
                }
            }
            return true;
        }

        internal override void SkipTag(NbtBinaryReader readStream)
        {
            ListType = readStream.ReadTagType();
            int bytesToSkip = readStream.ReadInt32();
            if (bytesToSkip < 0)
            {
                throw new NbtFormatException("Negative list size given.");
            }

            switch (ListType)
            {
                case NbtTagType.Byte:
                    readStream.Skip(bytesToSkip);
                    break;
                case NbtTagType.Short:
                    readStream.Skip(bytesToSkip * 2);
                    break;
                case NbtTagType.Int:
                    readStream.Skip(bytesToSkip * 4);
                    break;
                case NbtTagType.Long:
                    readStream.Skip(bytesToSkip * 8);
                    break;
                case NbtTagType.Float:
                    readStream.Skip(bytesToSkip * 4);
                    break;
                case NbtTagType.Double:
                    readStream.Skip(bytesToSkip * 8);
                    break;
                default:
                    for (int index = 0; index < bytesToSkip; ++index)
                    {
                        switch (listType)
                        {
                            case NbtTagType.ByteArray:
                                new NbtByteArray().SkipTag(readStream);
                                break;
                            case NbtTagType.String:
                                readStream.SkipString();
                                break;
                            case NbtTagType.List:
                                new NbtList().SkipTag(readStream);
                                break;
                            case NbtTagType.Compound:
                                new NbtCompound().SkipTag(readStream);
                                break;
                            case NbtTagType.IntArray:
                                new NbtIntArray().SkipTag(readStream);
                                break;
                        }
                    }
                    break;
            }
        }

        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.List);
            if (Name == null)
            {
                throw new NbtFormatException("Name is null");
            }

            writeStream.Write(Name);
            WriteData(writeStream);
        }

        internal override void WriteData(NbtBinaryWriter writeStream)
        {
            if (ListType == NbtTagType.Unknown)
            {
                throw new NbtFormatException("NbtList had no elements and an Unknown ListType");
            }

            writeStream.Write(ListType);
            writeStream.Write(tags.Count);
            foreach (NbtTag tag in tags)
            {
                tag.WriteData(writeStream);
            }
        }

        public IEnumerator<NbtTag> GetEnumerator()
        {
            return tags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return tags.GetEnumerator();
        }

        public int IndexOf([CanBeNull] NbtTag tag)
        {
            return tag == null ? -1 : tags.IndexOf(tag);
        }

        public void Insert(int tagIndex, [NotNull] NbtTag newTag)
        {
            if (newTag == null)
            {
                throw new ArgumentNullException(nameof(newTag));
            }

            if (listType != NbtTagType.Unknown && newTag.TagType != listType)
            {
                throw new ArgumentException("Items must be of type " + listType);
            }

            if (newTag.Parent != null)
            {
                throw new ArgumentException("A tag may only be added to one compound/list at a time.");
            }

            tags.Insert(tagIndex, newTag);
            if (listType == NbtTagType.Unknown)
            {
                listType = newTag.TagType;
            }

            newTag.Parent = this;
        }

        public void RemoveAt(int index)
        {
            NbtTag nbtTag = this[index];
            tags.RemoveAt(index);
            nbtTag.Parent = null;
        }

        public void Add([NotNull] NbtTag newTag)
        {
            if (newTag == null)
            {
                throw new ArgumentNullException(nameof(newTag));
            }

            if (newTag.Parent != null)
            {
                throw new ArgumentException("A tag may only be added to one compound/list at a time.");
            }

            if (newTag == this || newTag == Parent)
            {
                throw new ArgumentException("A list tag may not be added to itself or to its child tag.");
            }

            if (newTag.Name != null)
            {
                throw new ArgumentException("Named tag given. A list may only contain unnamed tags.");
            }

            if (listType != NbtTagType.Unknown && newTag.TagType != listType)
            {
                throw new ArgumentException("Items in this list must be of type " + listType + ". Given type: " + newTag.TagType);
            }

            tags.Add(newTag);
            newTag.Parent = this;
            if (listType != NbtTagType.Unknown)
            {
                return;
            }

            listType = newTag.TagType;
        }

        public void Clear()
        {
            for (int index = 0; index < tags.Count; ++index)
            {
                tags[index].Parent = null;
            }

            tags.Clear();
        }

        public bool Contains([NotNull] NbtTag item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return tags.Contains(item);
        }

        public void CopyTo(NbtTag[] array, int arrayIndex)
        {
            tags.CopyTo(array, arrayIndex);
        }

        public bool Remove([NotNull] NbtTag tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            if (!tags.Remove(tag))
            {
                return false;
            }

            tag.Parent = null;
            return true;
        }

        public int Count => tags.Count;

        bool ICollection<NbtTag>.IsReadOnly => false;

        void IList.Remove([NotNull] object value)
        {
            Remove((NbtTag)value);
        }

        [NotNull]
        object IList.this[int tagIndex]
        {
            get => tags[tagIndex];
            set => this[tagIndex] = (NbtTag)value;
        }

        int IList.Add([NotNull] object value)
        {
            Add((NbtTag)value);
            return tags.Count - 1;
        }

        bool IList.Contains([NotNull] object value)
        {
            return tags.Contains((NbtTag)value);
        }

        int IList.IndexOf([NotNull] object value)
        {
            return tags.IndexOf((NbtTag)value);
        }

        void IList.Insert(int index, [NotNull] object value)
        {
            Insert(index, (NbtTag)value);
        }

        bool IList.IsFixedSize => false;

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((NbtTag[])array, index);
        }

        object ICollection.SyncRoot => ((ICollection)tags).SyncRoot;

        bool ICollection.IsSynchronized => false;

        bool IList.IsReadOnly => false;

        public override object Clone()
        {
            return new NbtList(this);
        }

        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
        {
            for (int index = 0; index < indentLevel; ++index)
            {
                sb.Append(indentString);
            }

            sb.Append("TAG_List");
            if (!string.IsNullOrEmpty(Name))
            {
                sb.AppendFormat("(\"{0}\")", Name);
            }

            sb.AppendFormat(": {0} entries {{", tags.Count);
            if (Count > 0)
            {
                sb.Append('\n');
                foreach (NbtTag tag in tags)
                {
                    tag.PrettyPrint(sb, indentString, indentLevel + 1);
                    sb.Append('\n');
                }
                for (int index = 0; index < indentLevel; ++index)
                {
                    sb.Append(indentString);
                }
            }
            sb.Append('}');
        }
    }
}
