using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x0200001D RID: 29
	public sealed class NbtShort : NbtTag
	{
		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060001A1 RID: 417 RVA: 0x00007575 File Offset: 0x00005775
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.Short;
			}
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060001A2 RID: 418 RVA: 0x00007578 File Offset: 0x00005778
		// (set) Token: 0x060001A3 RID: 419 RVA: 0x00007580 File Offset: 0x00005780
		public short Value { get; set; }

		// Token: 0x060001A4 RID: 420 RVA: 0x00007589 File Offset: 0x00005789
		public NbtShort()
		{
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x00007591 File Offset: 0x00005791
		public NbtShort(short value) : this(null, value)
		{
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x0000759B File Offset: 0x0000579B
		public NbtShort([CanBeNull] string tagName) : this(tagName, 0)
		{
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x000075A5 File Offset: 0x000057A5
		public NbtShort([CanBeNull] string tagName, short value)
		{
			this.name = tagName;
			this.Value = value;
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x000075BB File Offset: 0x000057BB
		public NbtShort([NotNull] NbtShort other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			this.name = other.name;
			this.Value = other.Value;
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x000075E9 File Offset: 0x000057E9
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

		// Token: 0x060001AA RID: 426 RVA: 0x00007617 File Offset: 0x00005817
		internal override void SkipTag(NbtBinaryReader readStream)
		{
			readStream.ReadInt16();
		}

		// Token: 0x060001AB RID: 427 RVA: 0x00007620 File Offset: 0x00005820
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

		// Token: 0x060001AC RID: 428 RVA: 0x00007654 File Offset: 0x00005854
		internal override void WriteData(NbtBinaryWriter writeStream)
		{
			writeStream.Write(this.Value);
		}

		// Token: 0x060001AD RID: 429 RVA: 0x00007662 File Offset: 0x00005862
		public override object Clone()
		{
			return new NbtShort(this);
		}

		// Token: 0x060001AE RID: 430 RVA: 0x0000766C File Offset: 0x0000586C
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
