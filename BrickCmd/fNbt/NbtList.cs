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

        public override NbtTagType TagType
        {
            get
            {
                return NbtTagType.List;
            }
        }

        public NbtTagType ListType
        {
            get
            {
                return this.listType;
            }
            set
            {
                if (value == NbtTagType.End)
                {
                    if (this.tags.Count > 0)
                        throw new ArgumentException("Only empty list tags may have TagType of End.");
                }
                else if (value < NbtTagType.Byte || value > NbtTagType.IntArray && value != NbtTagType.Unknown)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (this.tags.Count > 0)
                {
                    NbtTagType tagType = this.tags[0].TagType;
                    if (tagType != value)
                        throw new ArgumentException(string.Format("Given NbtTagType ({0}) does not match actual element type ({1})", (object)value, (object)tagType));
                }
                this.listType = value;
            }
        }

        public NbtList()
          : this((string)null, (IEnumerable<NbtTag>)null, NbtTagType.Unknown)
        {
        }

        public NbtList([CanBeNull] string tagName)
          : this(tagName, (IEnumerable<NbtTag>)null, NbtTagType.Unknown)
        {
        }

        public NbtList([NotNull] IEnumerable<NbtTag> tags)
          : this((string)null, tags, NbtTagType.Unknown)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));
        }

        public NbtList(NbtTagType givenListType)
          : this((string)null, (IEnumerable<NbtTag>)null, givenListType)
        {
        }

        public NbtList([CanBeNull] string tagName, [NotNull] IEnumerable<NbtTag> tags)
          : this(tagName, tags, NbtTagType.Unknown)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));
        }

        public NbtList([NotNull] IEnumerable<NbtTag> tags, NbtTagType givenListType)
          : this((string)null, tags, givenListType)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));
        }

        public NbtList([CanBeNull] string tagName, NbtTagType givenListType)
          : this(tagName, (IEnumerable<NbtTag>)null, givenListType)
        {
        }

        public NbtList([CanBeNull] string tagName, [CanBeNull] IEnumerable<NbtTag> tags, NbtTagType givenListType)
        {
            this.name = tagName;
            this.ListType = givenListType;
            if (tags == null)
                return;
            foreach (NbtTag tag in tags)
                this.Add(tag);
        }

        public NbtList([NotNull] NbtList other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            this.name = other.name;
            this.listType = other.listType;
            foreach (NbtTag tag in other.tags)
                this.tags.Add((NbtTag)tag.Clone());
        }

        [NotNull]
        public override NbtTag this[int tagIndex]
        {
            get
            {
                return this.tags[tagIndex];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (value.Parent != null)
                    throw new ArgumentException("A tag may only be added to one compound/list at a time.");
                if (value == this || value == this.Parent)
                    throw new ArgumentException("A list tag may not be added to itself or to its child tag.");
                if (value.Name != null)
                    throw new ArgumentException("Named tag given. A list may only contain unnamed tags.");
                if (this.listType != NbtTagType.Unknown && value.TagType != this.listType)
                    throw new ArgumentException("Items must be of type " + (object)this.listType);
                this.tags[tagIndex] = value;
                value.Parent = (NbtTag)this;
            }
        }

        [NotNull]
        [Pure]
        public T Get<T>(int tagIndex) where T : NbtTag
        {
            return (T)this.tags[tagIndex];
        }

        public void AddRange([NotNull] IEnumerable<NbtTag> newTags)
        {
            if (newTags == null)
                throw new ArgumentNullException(nameof(newTags));
            foreach (NbtTag newTag in newTags)
                this.Add(newTag);
        }

        [Pure]
        [NotNull]
        public NbtTag[] ToArray()
        {
            return this.tags.ToArray();
        }

        [NotNull]
        [Pure]
        public T[] ToArray<T>() where T : NbtTag
        {
            T[] objArray = new T[this.tags.Count];
            for (int index = 0; index < objArray.Length; ++index)
                objArray[index] = (T)this.tags[index];
            return objArray;
        }

        internal override bool ReadTag(NbtBinaryReader readStream)
        {
            if (readStream.Selector != null && !readStream.Selector((NbtTag)this))
            {
                this.SkipTag(readStream);
                return false;
            }
            this.ListType = readStream.ReadTagType();
            int num = readStream.ReadInt32();
            if (num < 0)
                throw new NbtFormatException("Negative list size given.");
            for (int index = 0; index < num; ++index)
            {
                NbtTag nbtTag;
                switch (this.ListType)
                {
                    case NbtTagType.Byte:
                        nbtTag = (NbtTag)new NbtByte();
                        break;
                    case NbtTagType.Short:
                        nbtTag = (NbtTag)new NbtShort();
                        break;
                    case NbtTagType.Int:
                        nbtTag = (NbtTag)new NbtInt();
                        break;
                    case NbtTagType.Long:
                        nbtTag = (NbtTag)new NbtLong();
                        break;
                    case NbtTagType.Float:
                        nbtTag = (NbtTag)new NbtFloat();
                        break;
                    case NbtTagType.Double:
                        nbtTag = (NbtTag)new NbtDouble();
                        break;
                    case NbtTagType.ByteArray:
                        nbtTag = (NbtTag)new NbtByteArray();
                        break;
                    case NbtTagType.String:
                        nbtTag = (NbtTag)new NbtString();
                        break;
                    case NbtTagType.List:
                        nbtTag = (NbtTag)new NbtList();
                        break;
                    case NbtTagType.Compound:
                        nbtTag = (NbtTag)new NbtCompound();
                        break;
                    case NbtTagType.IntArray:
                        nbtTag = (NbtTag)new NbtIntArray();
                        break;
                    default:
                        throw new NbtFormatException("Unsupported tag type found in a list: " + (object)this.ListType);
                }
                nbtTag.Parent = (NbtTag)this;
                if (nbtTag.ReadTag(readStream))
                    this.tags.Add(nbtTag);
            }
            return true;
        }

        internal override void SkipTag(NbtBinaryReader readStream)
        {
            this.ListType = readStream.ReadTagType();
            int bytesToSkip = readStream.ReadInt32();
            if (bytesToSkip < 0)
                throw new NbtFormatException("Negative list size given.");
            switch (this.ListType)
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
                        switch (this.listType)
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
            if (this.Name == null)
                throw new NbtFormatException("Name is null");
            writeStream.Write(this.Name);
            this.WriteData(writeStream);
        }

        internal override void WriteData(NbtBinaryWriter writeStream)
        {
            if (this.ListType == NbtTagType.Unknown)
                throw new NbtFormatException("NbtList had no elements and an Unknown ListType");
            writeStream.Write(this.ListType);
            writeStream.Write(this.tags.Count);
            foreach (NbtTag tag in this.tags)
                tag.WriteData(writeStream);
        }

        public IEnumerator<NbtTag> GetEnumerator()
        {
            return (IEnumerator<NbtTag>)this.tags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.tags.GetEnumerator();
        }

        public int IndexOf([CanBeNull] NbtTag tag)
        {
            return tag == null ? -1 : this.tags.IndexOf(tag);
        }

        public void Insert(int tagIndex, [NotNull] NbtTag newTag)
        {
            if (newTag == null)
                throw new ArgumentNullException(nameof(newTag));
            if (this.listType != NbtTagType.Unknown && newTag.TagType != this.listType)
                throw new ArgumentException("Items must be of type " + (object)this.listType);
            if (newTag.Parent != null)
                throw new ArgumentException("A tag may only be added to one compound/list at a time.");
            this.tags.Insert(tagIndex, newTag);
            if (this.listType == NbtTagType.Unknown)
                this.listType = newTag.TagType;
            newTag.Parent = (NbtTag)this;
        }

        public void RemoveAt(int index)
        {
            NbtTag nbtTag = this[index];
            this.tags.RemoveAt(index);
            nbtTag.Parent = (NbtTag)null;
        }

        public void Add([NotNull] NbtTag newTag)
        {
            if (newTag == null)
                throw new ArgumentNullException(nameof(newTag));
            if (newTag.Parent != null)
                throw new ArgumentException("A tag may only be added to one compound/list at a time.");
            if (newTag == this || newTag == this.Parent)
                throw new ArgumentException("A list tag may not be added to itself or to its child tag.");
            if (newTag.Name != null)
                throw new ArgumentException("Named tag given. A list may only contain unnamed tags.");
            if (this.listType != NbtTagType.Unknown && newTag.TagType != this.listType)
                throw new ArgumentException("Items in this list must be of type " + (object)this.listType + ". Given type: " + (object)newTag.TagType);
            this.tags.Add(newTag);
            newTag.Parent = (NbtTag)this;
            if (this.listType != NbtTagType.Unknown)
                return;
            this.listType = newTag.TagType;
        }

        public void Clear()
        {
            for (int index = 0; index < this.tags.Count; ++index)
                this.tags[index].Parent = (NbtTag)null;
            this.tags.Clear();
        }

        public bool Contains([NotNull] NbtTag item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            return this.tags.Contains(item);
        }

        public void CopyTo(NbtTag[] array, int arrayIndex)
        {
            this.tags.CopyTo(array, arrayIndex);
        }

        public bool Remove([NotNull] NbtTag tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));
            if (!this.tags.Remove(tag))
                return false;
            tag.Parent = (NbtTag)null;
            return true;
        }

        public int Count
        {
            get
            {
                return this.tags.Count;
            }
        }

        bool ICollection<NbtTag>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        void IList.Remove([NotNull] object value)
        {
            this.Remove((NbtTag)value);
        }

        [NotNull]
        object IList.this[int tagIndex]
        {
            get
            {
                return (object)this.tags[tagIndex];
            }
            set
            {
                this[tagIndex] = (NbtTag)value;
            }
        }

        int IList.Add([NotNull] object value)
        {
            this.Add((NbtTag)value);
            return this.tags.Count - 1;
        }

        bool IList.Contains([NotNull] object value)
        {
            return this.tags.Contains((NbtTag)value);
        }

        int IList.IndexOf([NotNull] object value)
        {
            return this.tags.IndexOf((NbtTag)value);
        }

        void IList.Insert(int index, [NotNull] object value)
        {
            this.Insert(index, (NbtTag)value);
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo((NbtTag[])array, index);
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)this.tags).SyncRoot;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override object Clone()
        {
            return (object)new NbtList(this);
        }

        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
        {
            for (int index = 0; index < indentLevel; ++index)
                sb.Append(indentString);
            sb.Append("TAG_List");
            if (!string.IsNullOrEmpty(this.Name))
                sb.AppendFormat("(\"{0}\")", (object)this.Name);
            sb.AppendFormat(": {0} entries {{", (object)this.tags.Count);
            if (this.Count > 0)
            {
                sb.Append('\n');
                foreach (NbtTag tag in this.tags)
                {
                    tag.PrettyPrint(sb, indentString, indentLevel + 1);
                    sb.Append('\n');
                }
                for (int index = 0; index < indentLevel; ++index)
                    sb.Append(indentString);
            }
            sb.Append('}');
        }
    }
}
