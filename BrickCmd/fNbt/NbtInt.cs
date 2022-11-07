using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x02000019 RID: 25
	public sealed class NbtInt : NbtTag
	{
		// Token: 0x1700004C RID: 76
		// (get) Token: 0x06000144 RID: 324 RVA: 0x000065D9 File Offset: 0x000047D9
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.Int;
			}
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x06000145 RID: 325 RVA: 0x000065DC File Offset: 0x000047DC
		// (set) Token: 0x06000146 RID: 326 RVA: 0x000065E4 File Offset: 0x000047E4
		public int Value { get; set; }

		// Token: 0x06000147 RID: 327 RVA: 0x000065ED File Offset: 0x000047ED
		public NbtInt()
		{
		}

		// Token: 0x06000148 RID: 328 RVA: 0x000065F5 File Offset: 0x000047F5
		public NbtInt(int value) : this(null, value)
		{
		}

		// Token: 0x06000149 RID: 329 RVA: 0x000065FF File Offset: 0x000047FF
		public NbtInt([CanBeNull] string tagName) : this(tagName, 0)
		{
		}

		// Token: 0x0600014A RID: 330 RVA: 0x00006609 File Offset: 0x00004809
		public NbtInt([CanBeNull] string tagName, int value)
		{
			this.name = tagName;
			this.Value = value;
		}

		// Token: 0x0600014B RID: 331 RVA: 0x0000661F File Offset: 0x0000481F
		public NbtInt([NotNull] NbtInt other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			this.name = other.name;
			this.Value = other.Value;
		}

		// Token: 0x0600014C RID: 332 RVA: 0x0000664D File Offset: 0x0000484D
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

		// Token: 0x0600014D RID: 333 RVA: 0x0000667B File Offset: 0x0000487B
		internal override void SkipTag(NbtBinaryReader readStream)
		{
			readStream.ReadInt32();
		}

		// Token: 0x0600014E RID: 334 RVA: 0x00006684 File Offset: 0x00004884
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

		// Token: 0x0600014F RID: 335 RVA: 0x000066B8 File Offset: 0x000048B8
		internal override void WriteData(NbtBinaryWriter writeStream)
		{
			writeStream.Write(this.Value);
		}

		// Token: 0x06000150 RID: 336 RVA: 0x000066C6 File Offset: 0x000048C6
		public override object Clone()
		{
			return new NbtInt(this);
		}

		// Token: 0x06000151 RID: 337 RVA: 0x000066D0 File Offset: 0x000048D0
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
