using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x0200001C RID: 28
	public sealed class NbtLong : NbtTag
	{
		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000193 RID: 403 RVA: 0x00007418 File Offset: 0x00005618
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.Long;
			}
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000194 RID: 404 RVA: 0x0000741B File Offset: 0x0000561B
		// (set) Token: 0x06000195 RID: 405 RVA: 0x00007423 File Offset: 0x00005623
		public long Value { get; set; }

		// Token: 0x06000196 RID: 406 RVA: 0x0000742C File Offset: 0x0000562C
		public NbtLong()
		{
		}

		// Token: 0x06000197 RID: 407 RVA: 0x00007434 File Offset: 0x00005634
		public NbtLong(long value) : this(null, value)
		{
		}

		// Token: 0x06000198 RID: 408 RVA: 0x0000743E File Offset: 0x0000563E
		public NbtLong(string tagName) : this(tagName, 0L)
		{
		}

		// Token: 0x06000199 RID: 409 RVA: 0x00007449 File Offset: 0x00005649
		public NbtLong(string tagName, long value)
		{
			this.name = tagName;
			this.Value = value;
		}

		// Token: 0x0600019A RID: 410 RVA: 0x0000745F File Offset: 0x0000565F
		public NbtLong([NotNull] NbtLong other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			this.name = other.name;
			this.Value = other.Value;
		}

		// Token: 0x0600019B RID: 411 RVA: 0x0000748D File Offset: 0x0000568D
		internal override bool ReadTag(NbtBinaryReader readStream)
		{
			if (readStream.Selector != null && !readStream.Selector(this))
			{
				readStream.ReadInt64();
				return false;
			}
			this.Value = readStream.ReadInt64();
			return true;
		}

		// Token: 0x0600019C RID: 412 RVA: 0x000074BB File Offset: 0x000056BB
		internal override void SkipTag(NbtBinaryReader readStream)
		{
			readStream.ReadInt64();
		}

		// Token: 0x0600019D RID: 413 RVA: 0x000074C4 File Offset: 0x000056C4
		internal override void WriteTag(NbtBinaryWriter writeStream)
		{
			writeStream.Write(NbtTagType.Long);
			if (base.Name == null)
			{
				throw new NbtFormatException("Name is null");
			}
			writeStream.Write(base.Name);
			writeStream.Write(this.Value);
		}

		// Token: 0x0600019E RID: 414 RVA: 0x000074F8 File Offset: 0x000056F8
		internal override void WriteData(NbtBinaryWriter writeStream)
		{
			writeStream.Write(this.Value);
		}

		// Token: 0x0600019F RID: 415 RVA: 0x00007506 File Offset: 0x00005706
		public override object Clone()
		{
			return new NbtLong(this);
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x00007510 File Offset: 0x00005710
		internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
		{
			for (int i = 0; i < indentLevel; i++)
			{
				sb.Append(indentString);
			}
			sb.Append("TAG_Long");
			if (!string.IsNullOrEmpty(base.Name))
			{
				sb.AppendFormat("(\"{0}\")", base.Name);
			}
			sb.Append(": ");
			sb.Append(this.Value);
		}
	}
}
