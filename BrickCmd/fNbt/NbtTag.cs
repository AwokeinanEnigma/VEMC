using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x02000013 RID: 19
	public abstract class NbtTag : ICloneable
	{
		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060000C4 RID: 196 RVA: 0x0000500D File Offset: 0x0000320D
		// (set) Token: 0x060000C5 RID: 197 RVA: 0x00005015 File Offset: 0x00003215
		[CanBeNull]
		public NbtTag Parent { get; internal set; }

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060000C6 RID: 198
		public abstract NbtTagType TagType { get; }

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060000C7 RID: 199 RVA: 0x00005020 File Offset: 0x00003220
		public bool HasValue
		{
			get
			{
				NbtTagType tagType = this.TagType;
				if (tagType != NbtTagType.End)
				{
					switch (tagType)
					{
					case NbtTagType.List:
					case NbtTagType.Compound:
						break;
					default:
						if (tagType != NbtTagType.Unknown)
						{
							return true;
						}
						break;
					}
				}
				return false;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060000C8 RID: 200 RVA: 0x00005054 File Offset: 0x00003254
		// (set) Token: 0x060000C9 RID: 201 RVA: 0x0000505C File Offset: 0x0000325C
		[CanBeNull]
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				if (this.name == value)
				{
					return;
				}
				NbtCompound nbtCompound = this.Parent as NbtCompound;
				if (nbtCompound != null)
				{
					if (value == null)
					{
						throw new ArgumentNullException("value", "Name of tags inside an NbtCompound may not be null.");
					}
					if (this.name != null)
					{
						nbtCompound.RenameTag(this.name, value);
					}
				}
				this.name = value;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060000CA RID: 202 RVA: 0x000050B8 File Offset: 0x000032B8
		[NotNull]
		public string Path
		{
			get
			{
				if (this.Parent == null)
				{
					return this.Name ?? "";
				}
				NbtList nbtList = this.Parent as NbtList;
				if (nbtList != null)
				{
					return string.Concat(new object[]
					{
						nbtList.Path,
						'[',
						nbtList.IndexOf(this),
						']'
					});
				}
				return this.Parent.Path + '.' + this.Name;
			}
		}

		// Token: 0x060000CB RID: 203
		internal abstract bool ReadTag([NotNull] NbtBinaryReader readStream);

		// Token: 0x060000CC RID: 204
		internal abstract void SkipTag([NotNull] NbtBinaryReader readStream);

		// Token: 0x060000CD RID: 205
		internal abstract void WriteTag([NotNull] NbtBinaryWriter writeReader);

		// Token: 0x060000CE RID: 206
		internal abstract void WriteData([NotNull] NbtBinaryWriter writeReader);

		// Token: 0x1700002F RID: 47
		public virtual NbtTag this[string tagName]
		{
			get
			{
				throw new InvalidOperationException("String indexers only work on NbtCompound tags.");
			}
			set
			{
				throw new InvalidOperationException("String indexers only work on NbtCompound tags.");
			}
		}

		// Token: 0x17000030 RID: 48
		public virtual NbtTag this[int tagIndex]
		{
			get
			{
				throw new InvalidOperationException("Integer indexers only work on NbtList tags.");
			}
			set
			{
				throw new InvalidOperationException("Integer indexers only work on NbtList tags.");
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060000D3 RID: 211 RVA: 0x00005173 File Offset: 0x00003373
		public byte ByteValue
		{
			get
			{
				if (this.TagType == NbtTagType.Byte)
				{
					return ((NbtByte)this).Value;
				}
				throw new InvalidCastException("Cannot get ByteValue from " + NbtTag.GetCanonicalTagName(this.TagType));
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060000D4 RID: 212 RVA: 0x000051A4 File Offset: 0x000033A4
		public short ShortValue
		{
			get
			{
				switch (this.TagType)
				{
				case NbtTagType.Byte:
					return (short)((NbtByte)this).Value;
				case NbtTagType.Short:
					return ((NbtShort)this).Value;
				default:
					throw new InvalidCastException("Cannot get ShortValue from " + NbtTag.GetCanonicalTagName(this.TagType));
				}
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060000D5 RID: 213 RVA: 0x000051FC File Offset: 0x000033FC
		public int IntValue
		{
			get
			{
				switch (this.TagType)
				{
				case NbtTagType.Byte:
					return (int)((NbtByte)this).Value;
				case NbtTagType.Short:
					return (int)((NbtShort)this).Value;
				case NbtTagType.Int:
					return ((NbtInt)this).Value;
				default:
					throw new InvalidCastException("Cannot get IntValue from " + NbtTag.GetCanonicalTagName(this.TagType));
				}
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060000D6 RID: 214 RVA: 0x00005264 File Offset: 0x00003464
		public long LongValue
		{
			get
			{
				switch (this.TagType)
				{
				case NbtTagType.Byte:
					return (long)((ulong)((NbtByte)this).Value);
				case NbtTagType.Short:
					return (long)((NbtShort)this).Value;
				case NbtTagType.Int:
					return (long)((NbtInt)this).Value;
				case NbtTagType.Long:
					return ((NbtLong)this).Value;
				default:
					throw new InvalidCastException("Cannot get LongValue from " + NbtTag.GetCanonicalTagName(this.TagType));
				}
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060000D7 RID: 215 RVA: 0x000052E0 File Offset: 0x000034E0
		public float FloatValue
		{
			get
			{
				switch (this.TagType)
				{
				case NbtTagType.Byte:
					return (float)((NbtByte)this).Value;
				case NbtTagType.Short:
					return (float)((NbtShort)this).Value;
				case NbtTagType.Int:
					return (float)((NbtInt)this).Value;
				case NbtTagType.Long:
					return (float)((NbtLong)this).Value;
				case NbtTagType.Float:
					return ((NbtFloat)this).Value;
				case NbtTagType.Double:
					return (float)((NbtDouble)this).Value;
				default:
					throw new InvalidCastException("Cannot get FloatValue from " + NbtTag.GetCanonicalTagName(this.TagType));
				}
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060000D8 RID: 216 RVA: 0x00005380 File Offset: 0x00003580
		public double DoubleValue
		{
			get
			{
				switch (this.TagType)
				{
				case NbtTagType.Byte:
					return (double)((NbtByte)this).Value;
				case NbtTagType.Short:
					return (double)((NbtShort)this).Value;
				case NbtTagType.Int:
					return (double)((NbtInt)this).Value;
				case NbtTagType.Long:
					return (double)((NbtLong)this).Value;
				case NbtTagType.Float:
					return (double)((NbtFloat)this).Value;
				case NbtTagType.Double:
					return ((NbtDouble)this).Value;
				default:
					throw new InvalidCastException("Cannot get DoubleValue from " + NbtTag.GetCanonicalTagName(this.TagType));
				}
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060000D9 RID: 217 RVA: 0x0000541D File Offset: 0x0000361D
		public byte[] ByteArrayValue
		{
			get
			{
				if (this.TagType == NbtTagType.ByteArray)
				{
					return ((NbtByteArray)this).Value;
				}
				throw new InvalidCastException("Cannot get ByteArrayValue from " + NbtTag.GetCanonicalTagName(this.TagType));
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060000DA RID: 218 RVA: 0x0000544E File Offset: 0x0000364E
		public int[] IntArrayValue
		{
			get
			{
				if (this.TagType == NbtTagType.IntArray)
				{
					return ((NbtIntArray)this).Value;
				}
				throw new InvalidCastException("Cannot get IntArrayValue from " + NbtTag.GetCanonicalTagName(this.TagType));
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060000DB RID: 219 RVA: 0x00005480 File Offset: 0x00003680
		public string StringValue
		{
			get
			{
				switch (this.TagType)
				{
				case NbtTagType.Byte:
					return ((NbtByte)this).Value.ToString(CultureInfo.InvariantCulture);
				case NbtTagType.Short:
					return ((NbtShort)this).Value.ToString(CultureInfo.InvariantCulture);
				case NbtTagType.Int:
					return ((NbtInt)this).Value.ToString(CultureInfo.InvariantCulture);
				case NbtTagType.Long:
					return ((NbtLong)this).Value.ToString(CultureInfo.InvariantCulture);
				case NbtTagType.Float:
					return ((NbtFloat)this).Value.ToString(CultureInfo.InvariantCulture);
				case NbtTagType.Double:
					return ((NbtDouble)this).Value.ToString(CultureInfo.InvariantCulture);
				case NbtTagType.String:
					return ((NbtString)this).Value;
				}
				throw new InvalidCastException("Cannot get StringValue from " + NbtTag.GetCanonicalTagName(this.TagType));
			}
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00005580 File Offset: 0x00003780
		[CanBeNull]
		public static string GetCanonicalTagName(NbtTagType type)
		{
			switch (type)
			{
			case NbtTagType.End:
				return "TAG_End";
			case NbtTagType.Byte:
				return "TAG_Byte";
			case NbtTagType.Short:
				return "TAG_Short";
			case NbtTagType.Int:
				return "TAG_Int";
			case NbtTagType.Long:
				return "TAG_Long";
			case NbtTagType.Float:
				return "TAG_Float";
			case NbtTagType.Double:
				return "TAG_Double";
			case NbtTagType.ByteArray:
				return "TAG_Byte_Array";
			case NbtTagType.String:
				return "TAG_String";
			case NbtTagType.List:
				return "TAG_List";
			case NbtTagType.Compound:
				return "TAG_Compound";
			case NbtTagType.IntArray:
				return "TAG_Int_Array";
			default:
				return null;
			}
		}

		// Token: 0x060000DD RID: 221 RVA: 0x00005610 File Offset: 0x00003810
		public override string ToString()
		{
			return this.ToString(NbtTag.DefaultIndentString);
		}

		// Token: 0x060000DE RID: 222
		public abstract object Clone();

		// Token: 0x060000DF RID: 223 RVA: 0x00005620 File Offset: 0x00003820
		[NotNull]
		public string ToString([NotNull] string indentString)
		{
			if (indentString == null)
			{
				throw new ArgumentNullException("indentString");
			}
			StringBuilder stringBuilder = new StringBuilder();
			this.PrettyPrint(stringBuilder, indentString, 0);
			return stringBuilder.ToString();
		}

		// Token: 0x060000E0 RID: 224
		internal abstract void PrettyPrint([NotNull] StringBuilder sb, [NotNull] string indentString, int indentLevel);

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000E1 RID: 225 RVA: 0x00005650 File Offset: 0x00003850
		// (set) Token: 0x060000E2 RID: 226 RVA: 0x00005657 File Offset: 0x00003857
		[NotNull]
		public static string DefaultIndentString
		{
			get
			{
				return NbtTag.defaultIndentString;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				NbtTag.defaultIndentString = value;
			}
		}

		// Token: 0x04000066 RID: 102
		protected string name;

		// Token: 0x04000067 RID: 103
		private static string defaultIndentString = "  ";
	}
}
