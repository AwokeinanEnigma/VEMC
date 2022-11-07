﻿using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	public sealed class NbtShort : NbtTag
	{
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.Short;
			}
		}
		public short Value { get; set; }
		public NbtShort()
		{
		}
		public NbtShort(short value) : this(null, value)
		{
		}
		public NbtShort([CanBeNull] string tagName) : this(tagName, 0)
		{
		}
		public NbtShort([CanBeNull] string tagName, short value)
		{
			this.name = tagName;
			this.Value = value;
		}
		public NbtShort([NotNull] NbtShort other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			this.name = other.name;
			this.Value = other.Value;
		}
		internal override bool ReadTag(NbtBinaryReader readStream)
		{
			if (readStream.Selector != null && !readStream.Selector(this))
			{
				readStream.ReadInt16();
				return false;
			}
			this.Value = readStream.ReadInt16();
			return true;
		}
		internal override void SkipTag(NbtBinaryReader readStream)
		{
			readStream.ReadInt16();
		}
		internal override void WriteTag(NbtBinaryWriter writeStream)
		{
			writeStream.Write(NbtTagType.Short);
			if (base.Name == null)
			{
				throw new NbtFormatException("Name is null");
			}
			writeStream.Write(base.Name);
			writeStream.Write(this.Value);
		}
		internal override void WriteData(NbtBinaryWriter writeStream)
		{
			writeStream.Write(this.Value);
		}
		public override object Clone()
		{
			return new NbtShort(this);
		}
		internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
		{
			for (int i = 0; i < indentLevel; i++)
			{
				sb.Append(indentString);
			}
			sb.Append("TAG_Short");
			if (!string.IsNullOrEmpty(base.Name))
			{
				sb.AppendFormat("(\"{0}\")", base.Name);
			}
			sb.Append(": ");
			sb.Append(this.Value);
		}
	}
}
