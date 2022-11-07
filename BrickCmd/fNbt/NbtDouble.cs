using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x02000017 RID: 23
	public sealed class NbtDouble : NbtTag
	{
		// Token: 0x17000048 RID: 72
		// (get) Token: 0x06000128 RID: 296 RVA: 0x00006314 File Offset: 0x00004514
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.Double;
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000129 RID: 297 RVA: 0x00006317 File Offset: 0x00004517
		// (set) Token: 0x0600012A RID: 298 RVA: 0x0000631F File Offset: 0x0000451F
		public double Value { get; set; }

		// Token: 0x0600012B RID: 299 RVA: 0x00006328 File Offset: 0x00004528
		public NbtDouble()
		{
		}

		// Token: 0x0600012C RID: 300 RVA: 0x00006330 File Offset: 0x00004530
		public NbtDouble(double value) : this(null, value)
		{
		}

		// Token: 0x0600012D RID: 301 RVA: 0x0000633A File Offset: 0x0000453A
		public NbtDouble([CanBeNull] string tagName) : this(tagName, 0.0)
		{
		}

		// Token: 0x0600012E RID: 302 RVA: 0x0000634C File Offset: 0x0000454C
		public NbtDouble([CanBeNull] string tagName, double value)
		{
			this.name = tagName;
			this.Value = value;
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00006362 File Offset: 0x00004562
		public NbtDouble([NotNull] NbtDouble other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			this.name = other.name;
			this.Value = other.Value;
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00006390 File Offset: 0x00004590
		internal override bool ReadTag(NbtBinaryReader readStream)
		{
			if (readStream.Selector != null && !readStream.Selector(this))
			{
				readStream.ReadDouble();
				return false;
			}
			this.Value = readStream.ReadDouble();
			return true;
		}

		// Token: 0x06000131 RID: 305 RVA: 0x000063BE File Offset: 0x000045BE
		internal override void SkipTag(NbtBinaryReader readStream)
		{
			readStream.ReadDouble();
		}

		// Token: 0x06000132 RID: 306 RVA: 0x000063C7 File Offset: 0x000045C7
		internal override void WriteTag(NbtBinaryWriter writeStream)
		{
			writeStream.Write(NbtTagType.Double);
			if (base.Name == null)
			{
				throw new NbtFormatException("Name is null");
			}
			writeStream.Write(base.Name);
			writeStream.Write(this.Value);
		}

		// Token: 0x06000133 RID: 307 RVA: 0x000063FB File Offset: 0x000045FB
		internal override void WriteData(NbtBinaryWriter writeStream)
		{
			writeStream.Write(this.Value);
		}

		// Token: 0x06000134 RID: 308 RVA: 0x00006409 File Offset: 0x00004609
		public override object Clone()
		{
			return new NbtDouble(this);
		}

		// Token: 0x06000135 RID: 309 RVA: 0x00006414 File Offset: 0x00004614
		internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
		{
			for (int i = 0; i < indentLevel; i++)
			{
				sb.Append(indentString);
			}
			sb.Append("TAG_Double");
			if (!string.IsNullOrEmpty(base.Name))
			{
				sb.AppendFormat("(\"{0}\")", base.Name);
			}
			sb.Append(": ");
			sb.Append(this.Value);
		}
	}
}
