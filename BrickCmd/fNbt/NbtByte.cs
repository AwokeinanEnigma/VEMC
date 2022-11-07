using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x02000014 RID: 20
	public sealed class NbtByte : NbtTag
	{
		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000E5 RID: 229 RVA: 0x00005681 File Offset: 0x00003881
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.Byte;
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000E6 RID: 230 RVA: 0x00005684 File Offset: 0x00003884
		// (set) Token: 0x060000E7 RID: 231 RVA: 0x0000568C File Offset: 0x0000388C
		public byte Value { get; set; }

		// Token: 0x060000E8 RID: 232 RVA: 0x00005695 File Offset: 0x00003895
		public NbtByte()
		{
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x0000569D File Offset: 0x0000389D
		public NbtByte(byte value) : this(null, value)
		{
		}

		// Token: 0x060000EA RID: 234 RVA: 0x000056A7 File Offset: 0x000038A7
		public NbtByte([CanBeNull] string tagName) : this(tagName, 0)
		{
		}

		// Token: 0x060000EB RID: 235 RVA: 0x000056B1 File Offset: 0x000038B1
		public NbtByte([CanBeNull] string tagName, byte value)
		{
			this.name = tagName;
			this.Value = value;
		}

		// Token: 0x060000EC RID: 236 RVA: 0x000056C7 File Offset: 0x000038C7
		public NbtByte([NotNull] NbtByte other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			this.name = other.name;
			this.Value = other.Value;
		}

		// Token: 0x060000ED RID: 237 RVA: 0x000056F5 File Offset: 0x000038F5
		internal override bool ReadTag(NbtBinaryReader readStream)
		{
			if (readStream.Selector != null && !readStream.Selector(this))
			{
				readStream.ReadByte();
				return false;
			}
			this.Value = readStream.ReadByte();
			return true;
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00005723 File Offset: 0x00003923
		internal override void SkipTag(NbtBinaryReader readStream)
		{
			readStream.ReadByte();
		}

		// Token: 0x060000EF RID: 239 RVA: 0x0000572C File Offset: 0x0000392C
		internal override void WriteTag(NbtBinaryWriter writeStream)
		{
			writeStream.Write(NbtTagType.Byte);
			if (base.Name == null)
			{
				throw new NbtFormatException("Name is null");
			}
			writeStream.Write(base.Name);
			writeStream.Write(this.Value);
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x00005760 File Offset: 0x00003960
		internal override void WriteData(NbtBinaryWriter writeStream)
		{
			writeStream.Write(this.Value);
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x0000576E File Offset: 0x0000396E
		public override object Clone()
		{
			return new NbtByte(this);
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00005778 File Offset: 0x00003978
		internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
		{
			for (int i = 0; i < indentLevel; i++)
			{
				sb.Append(indentString);
			}
			sb.Append("TAG_Byte");
			if (!string.IsNullOrEmpty(base.Name))
			{
				sb.AppendFormat("(\"{0}\")", base.Name);
			}
			sb.Append(": ");
			sb.Append(this.Value);
		}
	}
}
