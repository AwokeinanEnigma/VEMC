// Decompiled with JetBrains decompiler

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace fNbt
{
    public class NbtReader
    {
        private const string NoValueToReadError = "Value aready read, or no value to read.";
        private const string NonValueTagError = "Trying to read value of a non-value tag.";
        private const string InvalidParentTagError = "Parent tag is neither a Compound nor a List.";
        private const string ErroneousStateError = "NbtReader is in an erroneous state!";
        private NbtParseState state;
        private readonly NbtBinaryReader reader;
        private Stack<NbtReaderNode> nodes;
        private readonly long streamStartOffset;
        private bool atValue;
        private object valueCache;
        private readonly bool canSeekStream;
        private bool cacheTagValues;

        public NbtReader([NotNull] Stream stream)
          : this(stream, true)
        {
        }

        public NbtReader([NotNull] Stream stream, bool bigEndian)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            this.SkipEndTags = true;
            this.CacheTagValues = false;
            this.ParentTagType = NbtTagType.Unknown;
            this.TagType = NbtTagType.Unknown;
            this.canSeekStream = stream.CanSeek;
            if (this.canSeekStream)
                this.streamStartOffset = stream.Position;
            this.reader = new NbtBinaryReader(stream, bigEndian);
        }

        [CanBeNull]
        public string RootName { get; private set; }

        [CanBeNull]
        public string ParentName { get; private set; }

        [CanBeNull]
        public string TagName { get; private set; }

        public NbtTagType ParentTagType { get; private set; }

        public NbtTagType TagType { get; private set; }

        public bool IsListElement
        {
            get
            {
                return this.ParentTagType == NbtTagType.List;
            }
        }

        public bool HasValue
        {
            get
            {
                switch (this.TagType)
                {
                    case NbtTagType.End:
                    case NbtTagType.List:
                    case NbtTagType.Compound:
                    case NbtTagType.Unknown:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public bool HasName
        {
            get
            {
                return this.TagName != null;
            }
        }

        public bool IsAtStreamEnd
        {
            get
            {
                return this.state == NbtParseState.AtStreamEnd;
            }
        }

        public bool IsCompound
        {
            get
            {
                return this.TagType == NbtTagType.Compound;
            }
        }

        public bool IsList
        {
            get
            {
                return this.TagType == NbtTagType.List;
            }
        }

        public bool HasLength
        {
            get
            {
                switch (this.TagType)
                {
                    case NbtTagType.ByteArray:
                    case NbtTagType.List:
                    case NbtTagType.IntArray:
                        return true;
                    default:
                        return false;
                }
            }
        }

        [NotNull]
        public Stream BaseStream
        {
            get
            {
                return this.reader.BaseStream;
            }
        }

        public int TagStartOffset { get; private set; }

        public int TagsRead { get; private set; }

        public int Depth { get; private set; }

        public NbtTagType ListType { get; private set; }

        public int TagLength { get; private set; }

        public int ParentTagLength { get; private set; }

        public int ListIndex { get; private set; }

        public bool IsInErrorState
        {
            get
            {
                return this.state == NbtParseState.Error;
            }
        }

        public bool ReadToFollowing()
        {
            switch (this.state)
            {
                case NbtParseState.AtStreamBeginning:
                    this.state = NbtParseState.Error;
                    if (this.reader.ReadTagType() != NbtTagType.Compound)
                    {
                        this.state = NbtParseState.Error;
                        throw new NbtFormatException("Given NBT stream does not start with a TAG_Compound");
                    }
                    this.Depth = 1;
                    this.TagType = NbtTagType.Compound;
                    this.ReadTagHeader(true);
                    this.RootName = this.TagName;
                    return true;
                case NbtParseState.AtCompoundBeginning:
                    this.GoDown();
                    this.state = NbtParseState.InCompound;
                    goto case NbtParseState.InCompound;
                case NbtParseState.InCompound:
                    if (this.atValue)
                        this.SkipValue();
                    if (this.canSeekStream)
                        this.TagStartOffset = (int)(this.reader.BaseStream.Position - this.streamStartOffset);
                    NbtParseState state = this.state;
                    this.state = NbtParseState.Error;
                    this.TagType = this.reader.ReadTagType();
                    this.state = state;
                    if (this.TagType == NbtTagType.End)
                    {
                        this.TagName = (string)null;
                        ++this.TagsRead;
                        this.state = NbtParseState.AtCompoundEnd;
                        if (!this.SkipEndTags)
                            return true;
                        --this.TagsRead;
                        goto case NbtParseState.AtCompoundEnd;
                    }
                    else
                    {
                        this.ReadTagHeader(true);
                        return true;
                    }
                case NbtParseState.AtCompoundEnd:
                    this.GoUp();
                    if (this.ParentTagType == NbtTagType.List)
                    {
                        this.state = NbtParseState.InList;
                        this.TagType = NbtTagType.Compound;
                        goto case NbtParseState.InList;
                    }
                    else if (this.ParentTagType == NbtTagType.Compound)
                    {
                        this.state = NbtParseState.InCompound;
                        goto case NbtParseState.InCompound;
                    }
                    else
                    {
                        if (this.ParentTagType == NbtTagType.Unknown)
                        {
                            this.state = NbtParseState.AtStreamEnd;
                            return false;
                        }
                        this.state = NbtParseState.Error;
                        throw new NbtFormatException("Parent tag is neither a Compound nor a List.");
                    }
                case NbtParseState.AtListBeginning:
                    this.GoDown();
                    this.ListIndex = -1;
                    this.TagType = this.ListType;
                    this.state = NbtParseState.InList;
                    goto case NbtParseState.InList;
                case NbtParseState.InList:
                    while (true)
                    {
                        if (this.atValue)
                            this.SkipValue();
                        ++this.ListIndex;
                        if (this.ListIndex >= this.ParentTagLength)
                        {
                            this.GoUp();
                            if (this.ParentTagType == NbtTagType.List)
                            {
                                this.state = NbtParseState.InList;
                                this.TagType = NbtTagType.List;
                            }
                            else
                                break;
                        }
                        else
                            goto label_23;
                    }
                    if (this.ParentTagType == NbtTagType.Compound)
                    {
                        this.state = NbtParseState.InCompound;
                        goto case NbtParseState.InCompound;
                    }
                    else
                    {
                        this.state = NbtParseState.Error;
                        throw new NbtFormatException("Parent tag is neither a Compound nor a List.");
                    }
                label_23:
                    if (this.canSeekStream)
                        this.TagStartOffset = (int)(this.reader.BaseStream.Position - this.streamStartOffset);
                    this.ReadTagHeader(false);
                    return true;
                case NbtParseState.AtStreamEnd:
                    return false;
                default:
                    throw new InvalidReaderStateException("NbtReader is in an erroneous state!");
            }
        }

        private void ReadTagHeader(bool readName)
        {
            ++this.TagsRead;
            this.TagName = readName ? this.reader.ReadString() : (string)null;
            this.valueCache = (object)null;
            this.TagLength = 0;
            this.atValue = false;
            this.ListType = NbtTagType.Unknown;
            switch (this.TagType)
            {
                case NbtTagType.Byte:
                case NbtTagType.Short:
                case NbtTagType.Int:
                case NbtTagType.Long:
                case NbtTagType.Float:
                case NbtTagType.Double:
                case NbtTagType.String:
                    this.atValue = true;
                    break;
                case NbtTagType.ByteArray:
                case NbtTagType.IntArray:
                    this.TagLength = this.reader.ReadInt32();
                    this.atValue = true;
                    break;
                case NbtTagType.List:
                    this.state = NbtParseState.Error;
                    this.ListType = this.reader.ReadTagType();
                    this.TagLength = this.reader.ReadInt32();
                    if (this.TagLength < 0)
                        throw new NbtFormatException("Negative tag length given: " + (object)this.TagLength);
                    this.state = NbtParseState.AtListBeginning;
                    break;
                case NbtTagType.Compound:
                    this.state = NbtParseState.AtCompoundBeginning;
                    break;
                default:
                    this.state = NbtParseState.Error;
                    throw new NbtFormatException("Trying to read tag of unknown type.");
            }
        }

        private void GoDown()
        {
            if (this.nodes == null)
                this.nodes = new Stack<NbtReaderNode>();
            this.nodes.Push(new NbtReaderNode()
            {
                ListIndex = this.ListIndex,
                ParentTagLength = this.ParentTagLength,
                ParentName = this.ParentName,
                ParentTagType = this.ParentTagType,
                ListType = this.ListType
            });
            this.ParentName = this.TagName;
            this.ParentTagType = this.TagType;
            this.ParentTagLength = this.TagLength;
            this.ListIndex = 0;
            this.TagLength = 0;
            ++this.Depth;
        }

        private void GoUp()
        {
            NbtReaderNode nbtReaderNode = this.nodes.Pop();
            this.ParentName = nbtReaderNode.ParentName;
            this.ParentTagType = nbtReaderNode.ParentTagType;
            this.ParentTagLength = nbtReaderNode.ParentTagLength;
            this.ListIndex = nbtReaderNode.ListIndex;
            this.ListType = nbtReaderNode.ListType;
            this.TagLength = 0;
            --this.Depth;
        }

        private void SkipValue()
        {
            switch (this.TagType)
            {
                case NbtTagType.Byte:
                    int num1 = (int)this.reader.ReadByte();
                    break;
                case NbtTagType.Short:
                    int num2 = (int)this.reader.ReadInt16();
                    break;
                case NbtTagType.Int:
                case NbtTagType.Float:
                    this.reader.ReadInt32();
                    break;
                case NbtTagType.Long:
                case NbtTagType.Double:
                    this.reader.ReadInt64();
                    break;
                case NbtTagType.ByteArray:
                    this.reader.Skip(this.TagLength);
                    break;
                case NbtTagType.String:
                    this.reader.SkipString();
                    break;
                case NbtTagType.IntArray:
                    this.reader.Skip(4 * this.TagLength);
                    break;
                default:
                    throw new InvalidOperationException("Trying to read value of a non-value tag.");
            }
            this.atValue = false;
            this.valueCache = (object)null;
        }

        public bool ReadToFollowing([CanBeNull] string tagName)
        {
            while (this.ReadToFollowing())
            {
                if (this.TagName == tagName)
                    return true;
            }
            return false;
        }

        public bool ReadToDescendant([CanBeNull] string tagName)
        {
            if (this.state == NbtParseState.Error)
                throw new InvalidReaderStateException("NbtReader is in an erroneous state!");
            if (this.state == NbtParseState.AtStreamEnd)
                return false;
            int depth = this.Depth;
            while (this.ReadToFollowing() && this.Depth > depth)
            {
                if (this.TagName == tagName)
                    return true;
            }
            return false;
        }

        public bool ReadToNextSibling()
        {
            if (this.state == NbtParseState.Error)
                throw new InvalidReaderStateException("NbtReader is in an erroneous state!");
            if (this.state == NbtParseState.AtStreamEnd)
                return false;
            int depth = this.Depth;
            while (this.ReadToFollowing())
            {
                if (this.Depth == depth)
                    return true;
                if (this.Depth < depth)
                    return false;
            }
            return false;
        }

        public bool ReadToNextSibling([CanBeNull] string tagName)
        {
            while (this.ReadToNextSibling())
            {
                if (this.TagName == tagName)
                    return true;
            }
            return false;
        }

        public int Skip()
        {
            if (this.state == NbtParseState.Error)
                throw new InvalidReaderStateException("NbtReader is in an erroneous state!");
            if (this.state == NbtParseState.AtStreamEnd)
                return 0;
            int depth = this.Depth;
            int num = 0;
            while (this.ReadToFollowing() && this.Depth >= depth)
                ++num;
            return num;
        }

        [NotNull]
        public NbtTag ReadAsTag()
        {
            switch (this.state)
            {
                case NbtParseState.AtStreamBeginning:
                case NbtParseState.AtCompoundEnd:
                    this.ReadToFollowing();
                    break;
                case NbtParseState.AtStreamEnd:
                    throw new EndOfStreamException();
                case NbtParseState.Error:
                    throw new InvalidReaderStateException("NbtReader is in an erroneous state!");
            }
            NbtTag parent;
            if (this.TagType == NbtTagType.Compound)
                parent = (NbtTag)new NbtCompound(this.TagName);
            else if (this.TagType == NbtTagType.List)
            {
                parent = (NbtTag)new NbtList(this.TagName, this.ListType);
            }
            else
            {
                if (!this.atValue)
                    throw new InvalidOperationException("Value aready read, or no value to read.");
                NbtTag nbtTag = this.ReadValueAsTag();
                this.ReadToFollowing();
                return nbtTag;
            }
            int depth1 = this.Depth;
            int depth2 = this.Depth;
            while (true)
            {
                do
                {
                    this.ReadToFollowing();
                    for (; this.Depth <= depth2 && parent.Parent != null; --depth2)
                        parent = parent.Parent;
                    if (this.Depth > depth1)
                    {
                        if (this.TagType == NbtTagType.Compound)
                        {
                            NbtTag thisTag = (NbtTag)new NbtCompound(this.TagName);
                            this.AddToParent(thisTag, parent);
                            parent = thisTag;
                            depth2 = this.Depth;
                        }
                        else if (this.TagType == NbtTagType.List)
                        {
                            NbtTag thisTag = (NbtTag)new NbtList(this.TagName, this.ListType);
                            this.AddToParent(thisTag, parent);
                            parent = thisTag;
                            depth2 = this.Depth;
                        }
                    }
                    else
                        goto label_22;
                }
                while (this.TagType == NbtTagType.End);
                this.AddToParent(this.ReadValueAsTag(), parent);
            }
        label_22:
            return parent;
        }

        private void AddToParent([NotNull] NbtTag thisTag, [NotNull] NbtTag parent)
        {
            switch (parent)
            {
                case NbtList nbtList:
                    nbtList.Add(thisTag);
                    break;
                case NbtCompound nbtCompound:
                    nbtCompound.Add(thisTag);
                    break;
                default:
                    throw new NbtFormatException("Parent tag is neither a Compound nor a List.");
            }
        }

        [NotNull]
        private NbtTag ReadValueAsTag()
        {
            if (!this.atValue)
                throw new InvalidOperationException("Value aready read, or no value to read.");
            this.atValue = false;
            switch (this.TagType)
            {
                case NbtTagType.Byte:
                    return (NbtTag)new NbtByte(this.TagName, this.reader.ReadByte());
                case NbtTagType.Short:
                    return (NbtTag)new NbtShort(this.TagName, this.reader.ReadInt16());
                case NbtTagType.Int:
                    return (NbtTag)new NbtInt(this.TagName, this.reader.ReadInt32());
                case NbtTagType.Long:
                    return (NbtTag)new NbtLong(this.TagName, this.reader.ReadInt64());
                case NbtTagType.Float:
                    return (NbtTag)new NbtFloat(this.TagName, this.reader.ReadSingle());
                case NbtTagType.Double:
                    return (NbtTag)new NbtDouble(this.TagName, this.reader.ReadDouble());
                case NbtTagType.ByteArray:
                    byte[] numArray1 = this.reader.ReadBytes(this.TagLength);
                    if (numArray1.Length < this.TagLength)
                        throw new EndOfStreamException();
                    return (NbtTag)new NbtByteArray(this.TagName, numArray1);
                case NbtTagType.String:
                    return (NbtTag)new NbtString(this.TagName, this.reader.ReadString());
                case NbtTagType.IntArray:
                    int[] numArray2 = new int[this.TagLength];
                    for (int index = 0; index < this.TagLength; ++index)
                        numArray2[index] = this.reader.ReadInt32();
                    return (NbtTag)new NbtIntArray(this.TagName, numArray2);
                default:
                    throw new InvalidOperationException("Trying to read value of a non-value tag.");
            }
        }

        public T ReadValueAs<T>()
        {
            return (T)this.ReadValue();
        }

        [NotNull]
        public object ReadValue()
        {
            if (this.state == NbtParseState.AtStreamEnd)
                throw new EndOfStreamException();
            if (!this.atValue)
            {
                if (!this.cacheTagValues)
                    throw new InvalidOperationException("Value aready read, or no value to read.");
                if (this.valueCache == null)
                    throw new InvalidOperationException("No value to read.");
                return this.valueCache;
            }
            this.valueCache = (object)null;
            this.atValue = false;
            object obj;
            switch (this.TagType)
            {
                case NbtTagType.Byte:
                    obj = (object)this.reader.ReadByte();
                    break;
                case NbtTagType.Short:
                    obj = (object)this.reader.ReadInt16();
                    break;
                case NbtTagType.Int:
                    obj = (object)this.reader.ReadInt32();
                    break;
                case NbtTagType.Long:
                    obj = (object)this.reader.ReadInt64();
                    break;
                case NbtTagType.Float:
                    obj = (object)this.reader.ReadSingle();
                    break;
                case NbtTagType.Double:
                    obj = (object)this.reader.ReadDouble();
                    break;
                case NbtTagType.ByteArray:
                    byte[] numArray1 = this.reader.ReadBytes(this.TagLength);
                    if (numArray1.Length < this.TagLength)
                        throw new EndOfStreamException();
                    obj = (object)numArray1;
                    break;
                case NbtTagType.String:
                    obj = (object)this.reader.ReadString();
                    break;
                case NbtTagType.IntArray:
                    int[] numArray2 = new int[this.TagLength];
                    for (int index = 0; index < this.TagLength; ++index)
                        numArray2[index] = this.reader.ReadInt32();
                    obj = (object)numArray2;
                    break;
                default:
                    throw new InvalidOperationException("Trying to read value of a non-value tag.");
            }
            this.valueCache = this.cacheTagValues ? obj : (object)null;
            return obj;
        }

        public bool SkipEndTags { get; set; }

        public bool CacheTagValues
        {
            get
            {
                return this.cacheTagValues;
            }
            set
            {
                this.cacheTagValues = value;
                if (this.cacheTagValues)
                    return;
                this.valueCache = (object)null;
            }
        }

        public override string ToString()
        {
            return this.ToString(false, NbtTag.DefaultIndentString);
        }

        [NotNull]
        public string ToString(bool includeValue)
        {
            return this.ToString(includeValue, NbtTag.DefaultIndentString);
        }

        [NotNull]
        public string ToString(bool includeValue, [NotNull] string indentString)
        {
            if (indentString == null)
                throw new ArgumentNullException(nameof(indentString));
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < this.Depth; ++index)
                stringBuilder.Append(indentString);
            stringBuilder.Append('#').Append(this.TagsRead).Append(". ").Append((object)this.TagType);
            if (this.IsList)
                stringBuilder.Append('<').Append((object)this.ListType).Append('>');
            if (this.HasLength)
                stringBuilder.Append('[').Append(this.TagLength).Append(']');
            stringBuilder.Append(' ').Append(this.TagName);
            if (includeValue && (this.atValue || this.HasValue && this.cacheTagValues) && (this.TagType != NbtTagType.IntArray && this.TagType != NbtTagType.ByteArray))
                stringBuilder.Append(" = ").Append(this.ReadValue());
            return stringBuilder.ToString();
        }
    }
}
