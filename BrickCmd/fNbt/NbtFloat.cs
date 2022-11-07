using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	public sealed class NbtFloat : NbtTag
	{
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.Float;
			}
		}
		public float Value { get; set; }
		public NbtFloat()
		{
		}
		public NbtFloat(float value) : this(null, value)
		{
		}
		public NbtFloat([CanBeNull] string tagName) : this(tagName, 0f)
		{
		}
		public NbtFloat([CanBeNull] string tagName, float value)
		{
			this.name = tagName;
			this.Value = value;
		}
		public NbtFloat([NotNull] NbtFloat other)
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
				readStream.ReadSingle();
				return false;
			}
			this.Value = readStream.ReadSingle();
			return true;
		}
		internal override void SkipTag(NbtBinaryReader readStream)
		{
			readStream.ReadSingle();
		}
		internal override void WriteTag(NbtBinaryWriter writeStream)
		{
			writeStream.Write(NbtTagType.Float);
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
			return new NbtFloat(this);
		}
		internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
		{
			for (int i = 0; i < indentLevel; i++)
			{
				sb.Append(indentString);
			}
			sb.Append("TAG_Float");
			if (!string.IsNullOrEmpty(base.Name))
			{
				sb.AppendFormat("(\"{0}\")", base.Name);
			}
			sb.Append(": ");
			sb.Append(this.Value);
		}
	}
}
