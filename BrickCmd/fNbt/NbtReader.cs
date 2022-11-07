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
            {
                throw new ArgumentNullException(nameof(stream));
            }

            SkipEndTags = true;
            CacheTagValues = false;
            ParentTagType = NbtTagType.Unknown;
            TagType = NbtTagType.Unknown;
            canSeekStream = stream.CanSeek;
            if (canSeekStream)
            {
                streamStartOffset = stream.Position;
            }

            reader = new NbtBinaryReader(stream, bigEndian);
        }

        [CanBeNull]
        public string RootName { get; private set; }

        [CanBeNull]
        public string ParentName { get; private set; }

        [CanBeNull]
        public string TagName { get; private set; }

        public NbtTagType ParentTagType { get; private set; }

        public NbtTagType TagType { get; private set; }

        public bool IsListElement => ParentTagType == NbtTagType.List;

        public bool HasValue
        {
            get
            {
                switch (TagType)
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

        public bool HasName => TagName != null;

        public bool IsAtStreamEnd => state == NbtParseState.AtStreamEnd;

        public bool IsCompound => TagType == NbtTagType.Compound;

        public bool IsList => TagType == NbtTagType.List;

        public bool HasLength
        {
            get
            {
                switch (TagType)
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
        public Stream BaseStream => reader.BaseStream;

        public int TagStartOffset { get; private set; }

        public int TagsRead { get; private set; }

        public int Depth { get; private set; }

        public NbtTagType ListType { get; private set; }

        public int TagLength { get; private set; }

        public int ParentTagLength { get; private set; }

        public int ListIndex { get; private set; }

        public bool IsInErrorState => state == NbtParseState.Error;

        public bool ReadToFollowing()
        {
            switch (state)
            {
                case NbtParseState.AtStreamBeginning:
                    this.state = NbtParseState.Error;
                    if (reader.ReadTagType() != NbtTagType.Compound)
                    {
                        this.state = NbtParseState.Error;
                        throw new NbtFormatException("Given NBT stream does not start with a TAG_Compound");
                    }
                    Depth = 1;
                    TagType = NbtTagType.Compound;
                    ReadTagHeader(true);
                    RootName = TagName;
                    return true;
                case NbtParseState.AtCompoundBeginning:
                    GoDown();
                    this.state = NbtParseState.InCompound;
                    goto case NbtParseState.InCompound;
                case NbtParseState.InCompound:
                    if (atValue)
                    {
                        SkipValue();
                    }

                    if (canSeekStream)
                    {
                        TagStartOffset = (int)(reader.BaseStream.Position - streamStartOffset);
                    }

                    NbtParseState state = this.state;
                    this.state = NbtParseState.Error;
                    TagType = reader.ReadTagType();
                    this.state = state;
                    if (TagType == NbtTagType.End)
                    {
                        TagName = null;
                        ++TagsRead;
                        this.state = NbtParseState.AtCompoundEnd;
                        if (!SkipEndTags)
                        {
                            return true;
                        }

                        --TagsRead;
                        goto case NbtParseState.AtCompoundEnd;
                    }
                    else
                    {
                        ReadTagHeader(true);
                        return true;
                    }
                case NbtParseState.AtCompoundEnd:
                    GoUp();
                    if (ParentTagType == NbtTagType.List)
                    {
                        this.state = NbtParseState.InList;
                        TagType = NbtTagType.Compound;
                        goto case NbtParseState.InList;
                    }
                    else if (ParentTagType == NbtTagType.Compound)
                    {
                        this.state = NbtParseState.InCompound;
                        goto case NbtParseState.InCompound;
                    }
                    else
                    {
                        if (ParentTagType == NbtTagType.Unknown)
                        {
                            this.state = NbtParseState.AtStreamEnd;
                            return false;
                        }
                        this.state = NbtParseState.Error;
                        throw new NbtFormatException("Parent tag is neither a Compound nor a List.");
                    }
                case NbtParseState.AtListBeginning:
                    GoDown();
                    ListIndex = -1;
                    TagType = ListType;
                    this.state = NbtParseState.InList;
                    goto case NbtParseState.InList;
                case NbtParseState.InList:
                    while (true)
                    {
                        if (atValue)
                        {
                            SkipValue();
                        }

                        ++ListIndex;
                        if (ListIndex >= ParentTagLength)
                        {
                            GoUp();
                            if (ParentTagType == NbtTagType.List)
                            {
                                this.state = NbtParseState.InList;
                                TagType = NbtTagType.List;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            goto label_23;
                        }
                    }
                    if (ParentTagType == NbtTagType.Compound)
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
                    if (canSeekStream)
                    {
                        TagStartOffset = (int)(reader.BaseStream.Position - streamStartOffset);
                    }

                    ReadTagHeader(false);
                    return true;
                case NbtParseState.AtStreamEnd:
                    return false;
                default:
                    throw new InvalidReaderStateException("NbtReader is in an erroneous state!");
            }
        }

        private void ReadTagHeader(bool readName)
        {
            ++TagsRead;
            TagName = readName ? reader.ReadString() : null;
            valueCache = null;
            TagLength = 0;
            atValue = false;
            ListType = NbtTagType.Unknown;
            switch (TagType)
            {
                case NbtTagType.Byte:
                case NbtTagType.Short:
                case NbtTagType.Int:
                case NbtTagType.Long:
                case NbtTagType.Float:
                case NbtTagType.Double:
                case NbtTagType.String:
                    atValue = true;
                    break;
                case NbtTagType.ByteArray:
                case NbtTagType.IntArray:
                    TagLength = reader.ReadInt32();
                    atValue = true;
                    break;
                case NbtTagType.List:
                    state = NbtParseState.Error;
                    ListType = reader.ReadTagType();
                    TagLength = reader.ReadInt32();
                    if (TagLength < 0)
                    {
                        throw new NbtFormatException("Negative tag length given: " + TagLength);
                    }

                    state = NbtParseState.AtListBeginning;
                    break;
                case NbtTagType.Compound:
                    state = NbtParseState.AtCompoundBeginning;
                    break;
                default:
                    state = NbtParseState.Error;
                    throw new NbtFormatException("Trying to read tag of unknown type.");
            }
        }

        private void GoDown()
        {
            if (nodes == null)
            {
                nodes = new Stack<NbtReaderNode>();
            }

            nodes.Push(new NbtReaderNode()
            {
                ListIndex = ListIndex,
                ParentTagLength = ParentTagLength,
                ParentName = ParentName,
                ParentTagType = ParentTagType,
                ListType = ListType
            });
            ParentName = TagName;
            ParentTagType = TagType;
            ParentTagLength = TagLength;
            ListIndex = 0;
            TagLength = 0;
            ++Depth;
        }

        private void GoUp()
        {
            NbtReaderNode nbtReaderNode = nodes.Pop();
            ParentName = nbtReaderNode.ParentName;
            ParentTagType = nbtReaderNode.ParentTagType;
            ParentTagLength = nbtReaderNode.ParentTagLength;
            ListIndex = nbtReaderNode.ListIndex;
            ListType = nbtReaderNode.ListType;
            TagLength = 0;
            --Depth;
        }

        private void SkipValue()
        {
            switch (TagType)
            {
                case NbtTagType.Byte:
                    int num1 = reader.ReadByte();
                    break;
                case NbtTagType.Short:
                    int num2 = reader.ReadInt16();
                    break;
                case NbtTagType.Int:
                case NbtTagType.Float:
                    reader.ReadInt32();
                    break;
                case NbtTagType.Long:
                case NbtTagType.Double:
                    reader.ReadInt64();
                    break;
                case NbtTagType.ByteArray:
                    reader.Skip(TagLength);
                    break;
                case NbtTagType.String:
                    reader.SkipString();
                    break;
                case NbtTagType.IntArray:
                    reader.Skip(4 * TagLength);
                    break;
                default:
                    throw new InvalidOperationException("Trying to read value of a non-value tag.");
            }
            atValue = false;
            valueCache = null;
        }

        public bool ReadToFollowing([CanBeNull] string tagName)
        {
            while (ReadToFollowing())
            {
                if (TagName == tagName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ReadToDescendant([CanBeNull] string tagName)
        {
            if (state == NbtParseState.Error)
            {
                throw new InvalidReaderStateException("NbtReader is in an erroneous state!");
            }

            if (state == NbtParseState.AtStreamEnd)
            {
                return false;
            }

            int depth = Depth;
            while (ReadToFollowing() && Depth > depth)
            {
                if (TagName == tagName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ReadToNextSibling()
        {
            if (state == NbtParseState.Error)
            {
                throw new InvalidReaderStateException("NbtReader is in an erroneous state!");
            }

            if (state == NbtParseState.AtStreamEnd)
            {
                return false;
            }

            int depth = Depth;
            while (ReadToFollowing())
            {
                if (Depth == depth)
                {
                    return true;
                }

                if (Depth < depth)
                {
                    return false;
                }
            }
            return false;
        }

        public bool ReadToNextSibling([CanBeNull] string tagName)
        {
            while (ReadToNextSibling())
            {
                if (TagName == tagName)
                {
                    return true;
                }
            }
            return false;
        }

        public int Skip()
        {
            if (state == NbtParseState.Error)
            {
                throw new InvalidReaderStateException("NbtReader is in an erroneous state!");
            }

            if (state == NbtParseState.AtStreamEnd)
            {
                return 0;
            }

            int depth = Depth;
            int num = 0;
            while (ReadToFollowing() && Depth >= depth)
            {
                ++num;
            }

            return num;
        }

        [NotNull]
        public NbtTag ReadAsTag()
        {
            switch (state)
            {
                case NbtParseState.AtStreamBeginning:
                case NbtParseState.AtCompoundEnd:
                    ReadToFollowing();
                    break;
                case NbtParseState.AtStreamEnd:
                    throw new EndOfStreamException();
                case NbtParseState.Error:
                    throw new InvalidReaderStateException("NbtReader is in an erroneous state!");
            }
            NbtTag parent;
            if (TagType == NbtTagType.Compound)
            {
                parent = new NbtCompound(TagName);
            }
            else if (TagType == NbtTagType.List)
            {
                parent = new NbtList(TagName, ListType);
            }
            else
            {
                if (!atValue)
                {
                    throw new InvalidOperationException("Value aready read, or no value to read.");
                }

                NbtTag nbtTag = ReadValueAsTag();
                ReadToFollowing();
                return nbtTag;
            }
            int depth1 = Depth;
            int depth2 = Depth;
            while (true)
            {
                do
                {
                    ReadToFollowing();
                    for (; Depth <= depth2 && parent.Parent != null; --depth2)
                    {
                        parent = parent.Parent;
                    }

                    if (Depth > depth1)
                    {
                        if (TagType == NbtTagType.Compound)
                        {
                            NbtTag thisTag = new NbtCompound(TagName);
                            AddToParent(thisTag, parent);
                            parent = thisTag;
                            depth2 = Depth;
                        }
                        else if (TagType == NbtTagType.List)
                        {
                            NbtTag thisTag = new NbtList(TagName, ListType);
                            AddToParent(thisTag, parent);
                            parent = thisTag;
                            depth2 = Depth;
                        }
                    }
                    else
                    {
                        goto label_22;
                    }
                }
                while (TagType == NbtTagType.End);
                AddToParent(ReadValueAsTag(), parent);
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
            if (!atValue)
            {
                throw new InvalidOperationException("Value aready read, or no value to read.");
            }

            atValue = false;
            switch (TagType)
            {
                case NbtTagType.Byte:
                    return new NbtByte(TagName, reader.ReadByte());
                case NbtTagType.Short:
                    return new NbtShort(TagName, reader.ReadInt16());
                case NbtTagType.Int:
                    return new NbtInt(TagName, reader.ReadInt32());
                case NbtTagType.Long:
                    return new NbtLong(TagName, reader.ReadInt64());
                case NbtTagType.Float:
                    return new NbtFloat(TagName, reader.ReadSingle());
                case NbtTagType.Double:
                    return new NbtDouble(TagName, reader.ReadDouble());
                case NbtTagType.ByteArray:
                    byte[] numArray1 = reader.ReadBytes(TagLength);
                    if (numArray1.Length < TagLength)
                    {
                        throw new EndOfStreamException();
                    }

                    return new NbtByteArray(TagName, numArray1);
                case NbtTagType.String:
                    return new NbtString(TagName, reader.ReadString());
                case NbtTagType.IntArray:
                    int[] numArray2 = new int[TagLength];
                    for (int index = 0; index < TagLength; ++index)
                    {
                        numArray2[index] = reader.ReadInt32();
                    }

                    return new NbtIntArray(TagName, numArray2);
                default:
                    throw new InvalidOperationException("Trying to read value of a non-value tag.");
            }
        }

        public T ReadValueAs<T>()
        {
            return (T)ReadValue();
        }

        [NotNull]
        public object ReadValue()
        {
            if (state == NbtParseState.AtStreamEnd)
            {
                throw new EndOfStreamException();
            }

            if (!atValue)
            {
                if (!cacheTagValues)
                {
                    throw new InvalidOperationException("Value aready read, or no value to read.");
                }

                if (valueCache == null)
                {
                    throw new InvalidOperationException("No value to read.");
                }

                return valueCache;
            }
            valueCache = null;
            atValue = false;
            object obj;
            switch (TagType)
            {
                case NbtTagType.Byte:
                    obj = reader.ReadByte();
                    break;
                case NbtTagType.Short:
                    obj = reader.ReadInt16();
                    break;
                case NbtTagType.Int:
                    obj = reader.ReadInt32();
                    break;
                case NbtTagType.Long:
                    obj = reader.ReadInt64();
                    break;
                case NbtTagType.Float:
                    obj = reader.ReadSingle();
                    break;
                case NbtTagType.Double:
                    obj = reader.ReadDouble();
                    break;
                case NbtTagType.ByteArray:
                    byte[] numArray1 = reader.ReadBytes(TagLength);
                    if (numArray1.Length < TagLength)
                    {
                        throw new EndOfStreamException();
                    }

                    obj = numArray1;
                    break;
                case NbtTagType.String:
                    obj = reader.ReadString();
                    break;
                case NbtTagType.IntArray:
                    int[] numArray2 = new int[TagLength];
                    for (int index = 0; index < TagLength; ++index)
                    {
                        numArray2[index] = reader.ReadInt32();
                    }

                    obj = numArray2;
                    break;
                default:
                    throw new InvalidOperationException("Trying to read value of a non-value tag.");
            }
            valueCache = cacheTagValues ? obj : null;
            return obj;
        }

        public bool SkipEndTags { get; set; }

        public bool CacheTagValues
        {
            get => cacheTagValues;
            set
            {
                cacheTagValues = value;
                if (cacheTagValues)
                {
                    return;
                }

                valueCache = null;
            }
        }

        public override string ToString()
        {
            return ToString(false, NbtTag.DefaultIndentString);
        }

        [NotNull]
        public string ToString(bool includeValue)
        {
            return ToString(includeValue, NbtTag.DefaultIndentString);
        }

        [NotNull]
        public string ToString(bool includeValue, [NotNull] string indentString)
        {
            if (indentString == null)
            {
                throw new ArgumentNullException(nameof(indentString));
            }

            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < Depth; ++index)
            {
                stringBuilder.Append(indentString);
            }

            stringBuilder.Append('#').Append(TagsRead).Append(". ").Append(TagType);
            if (IsList)
            {
                stringBuilder.Append('<').Append(ListType).Append('>');
            }

            if (HasLength)
            {
                stringBuilder.Append('[').Append(TagLength).Append(']');
            }

            stringBuilder.Append(' ').Append(TagName);
            if (includeValue && (atValue || HasValue && cacheTagValues) && (TagType != NbtTagType.IntArray && TagType != NbtTagType.ByteArray))
            {
                stringBuilder.Append(" = ").Append(ReadValue());
            }

            return stringBuilder.ToString();
        }
    }
}
