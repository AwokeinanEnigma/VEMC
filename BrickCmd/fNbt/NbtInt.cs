using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	public sealed class NbtInt : NbtTag
	{
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.Int;
			}
		}
		public int Value { get; set; }
		public NbtInt()
		{
		}
		public NbtInt(int value) : this(null, value)
		{
		}
		public NbtInt([CanBeNull] string tagName) : this(tagName, 0)
		{
		}
		public NbtInt([CanBeNull] string tagName, int value)
		{
			this.name = tagName;
			this.Value = value;
		}
		public NbtInt([NotNull] NbtInt other)
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
				readStream.ReadInt32();
				return false;
			}
			this.Value = readStream.ReadInt32();
			return true;
		}
		internal override void SkipTag(NbtBinaryReader readStream)
		{
			readStream.ReadInt32();
		}
		internal override void WriteTag(NbtBinaryWriter writeStream)
		{
			writeStream.Write(NbtTagType.Int);
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
			return new NbtInt(this);
		}
		internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
		{
			for (int i = 0; i < indentLevel; i++)
			{
				sb.Append(indentString);
			}
			sb.Append("TAG_Int");
			if (!string.IsNullOrEmpty(base.Name))
			{
				sb.AppendFormat("(\"{0}\")", base.Name);
			}
			sb.Append(": ");
			sb.Append(this.Value);
		}
	}
}
