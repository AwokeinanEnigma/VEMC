﻿namespace fNbt
{
    internal sealed class NbtReaderNode
    {
        public string ParentName;
        public NbtTagType ParentTagType;
        public NbtTagType ListType;
        public int ParentTagLength;
        public int ListIndex;
    }
}
