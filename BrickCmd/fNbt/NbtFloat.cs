using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x02000018 RID: 24
	public sealed class NbtFloat : NbtTag
	{
		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000136 RID: 310 RVA: 0x00006479 File Offset: 0x00004679
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.Float;
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000137 RID: 311 RVA: 0x0000647C File Offset: 0x0000467C
		// (set) Token: 0x06000138 RID: 312 RVA: 0x00006484 File Offset: 0x00004684
		public float Value { get; set; }

		// Token: 0x06000139 RID: 313 RVA: 0x0000648D File Offset: 0x0000468D
		public NbtFloat()
		{
		}

		// Token: 0x0600013A RID: 314 RVA: 0x00006495 File Offset: 0x00004695
		public NbtFloat(float value) : this(null, value)
		{
		}

		// Token: 0x0600013B RID: 315 RVA: 0x0000649F File Offset: 0x0000469F
		public NbtFloat([CanBeNull] string tagName) : this(tagName, 0f)
		{
		}

		// Token: 0x0600013C RID: 316 RVA: 0x000064AD File Offset: 0x000046AD
		public NbtFloat([CanBeNull] string tagName, float value)
		{
			this.name = tagName;
			this.Value = value;
		}

		// Token: 0x0600013D RID: 317 RVA: 0x000064C3 File Offset: 0x000046C3
		public NbtFloat([NotNull] NbtFloat other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			this.name = other.name;
			this.Value = other.Value;
		}

		// Token: 0x0600013E RID: 318 RVA: 0x000064F1 File Offset: 0x000046F1
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

		// Token: 0x0600013F RID: 319 RVA: 0x0000651F File Offset: 0x0000471F
		internal override void SkipTag(NbtBinaryReader readStream)
		{
			readStream.ReadSingle();
		}

		// Token: 0x06000140 RID: 320 RVA: 0x00006528 File Offset: 0x00004728
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

		// Token: 0x06000141 RID: 321 RVA: 0x0000655C File Offset: 0x0000475C
		internal override void WriteData(NbtBinaryWriter writeStream)
		{
			writeStream.Write(this.Value);
		}

		// Token: 0x06000142 RID: 322 RVA: 0x0000656A File Offset: 0x0000476A
		public override object Clone()
		{
			return new NbtFloat(this);
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00006574 File Offset: 0x00004774
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
