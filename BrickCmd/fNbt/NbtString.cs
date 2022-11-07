using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x0200001E RID: 30
	public sealed class NbtString : NbtTag
	{
		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060001AF RID: 431 RVA: 0x000076D1 File Offset: 0x000058D1
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.String;
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060001B0 RID: 432 RVA: 0x000076D4 File Offset: 0x000058D4
		// (set) Token: 0x060001B1 RID: 433 RVA: 0x000076DC File Offset: 0x000058DC
		[NotNull]
		public string Value
		{
			get
			{
				return this.stringVal;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				this.stringVal = value;
			}
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x000076F3 File Offset: 0x000058F3
		public NbtString()
		{
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x00007706 File Offset: 0x00005906
		public NbtString([NotNull] string value) : this(null, value)
		{
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x00007710 File Offset: 0x00005910
		public NbtString([CanBeNull] string tagName, [NotNull] string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			this.name = tagName;
			this.Value = value;
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x0000773F File Offset: 0x0000593F
		public NbtString([NotNull] NbtString other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			this.name = other.name;
			this.Value = other.Value;
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x00007778 File Offset: 0x00005978
		internal override bool ReadTag(NbtBinaryReader readStream)
		{
			if (readStream.Selector != null && !readStream.Selector(this))
			{
				readStream.SkipString();
				return false;
			}
			this.Value = readStream.ReadString();
			return true;
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x000077A5 File Offset: 0x000059A5
		internal override void SkipTag(NbtBinaryReader readStream)
		{
			readStream.SkipString();
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x000077AD File Offset: 0x000059AD
		internal override void WriteTag(NbtBinaryWriter writeStream)
		{
			writeStream.Write(NbtTagType.String);
			if (base.Name == null)
			{
				throw new NbtFormatException("Name is null");
			}
			writeStream.Write(base.Name);
			writeStream.Write(this.Value);
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x000077E1 File Offset: 0x000059E1
		internal override void WriteData(NbtBinaryWriter writeStream)
		{
			writeStream.Write(this.Value);
		}

		// Token: 0x060001BA RID: 442 RVA: 0x000077EF File Offset: 0x000059EF
		public override object Clone()
		{
			return new NbtString(this);
		}

		// Token: 0x060001BB RID: 443 RVA: 0x000077F8 File Offset: 0x000059F8
		internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
		{
			for (int i = 0; i < indentLevel; i++)
			{
				sb.Append(indentString);
			}
			sb.Append("TAG_String");
			if (!string.IsNullOrEmpty(base.Name))
			{
				sb.AppendFormat("(\"{0}\")", base.Name);
			}
			sb.Append(": \"");
			sb.Append(this.Value);
			sb.Append('"');
		}

		// Token: 0x04000076 RID: 118
		[NotNull]
		private string stringVal = "";
	}
}
