using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	public abstract class NbtTag : ICloneable
	{
		[CanBeNull]
		public NbtTag Parent { get; internal set; }
		public abstract NbtTagType TagType { get; }
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
		internal abstract bool ReadTag([NotNull] NbtBinaryReader readStream);
		internal abstract void SkipTag([NotNull] NbtBinaryReader readStream);
		internal abstract void WriteTag([NotNull] NbtBinaryWriter writeReader);
		internal abstract void WriteData([NotNull] NbtBinaryWriter writeReader);
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
		public override string ToString()
		{
			return this.ToString(NbtTag.DefaultIndentString);
		}
		public abstract object Clone();
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
		internal abstract void PrettyPrint([NotNull] StringBuilder sb, [NotNull] string indentString, int indentLevel);
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
		protected string name;
		private static string defaultIndentString = "  ";
	}
}
