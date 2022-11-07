using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x02000016 RID: 22
	public sealed class NbtCompound : NbtTag, ICollection<NbtTag>, IEnumerable<NbtTag>, ICollection, IEnumerable
	{
		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000104 RID: 260 RVA: 0x00005A06 File Offset: 0x00003C06
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.Compound;
			}
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00005A0A File Offset: 0x00003C0A
		public NbtCompound()
		{
		}

		// Token: 0x06000106 RID: 262 RVA: 0x00005A1D File Offset: 0x00003C1D
		public NbtCompound([CanBeNull] string tagName)
		{
			this.name = tagName;
		}

		// Token: 0x06000107 RID: 263 RVA: 0x00005A37 File Offset: 0x00003C37
		public NbtCompound([NotNull] IEnumerable<NbtTag> tags) : this(null, tags)
		{
		}

		// Token: 0x06000108 RID: 264 RVA: 0x00005A44 File Offset: 0x00003C44
		public NbtCompound([CanBeNull] string tagName, [NotNull] IEnumerable<NbtTag> tags)
		{
			if (tags == null)
			{
				throw new ArgumentNullException("tags");
			}
			this.name = tagName;
			foreach (NbtTag newTag in tags)
			{
				this.Add(newTag);
			}
		}

		// Token: 0x06000109 RID: 265 RVA: 0x00005AB4 File Offset: 0x00003CB4
		public NbtCompound([NotNull] NbtCompound other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			this.name = other.name;
			foreach (NbtTag nbtTag in other.tags.Values)
			{
				this.Add((NbtTag)nbtTag.Clone());
			}
		}

		// Token: 0x17000041 RID: 65
		public override NbtTag this[[NotNull] string tagName]
		{
			[CanBeNull]
			get
			{
				return this.Get<NbtTag>(tagName);
			}
			set
			{
				if (tagName == null)
				{
					throw new ArgumentNullException("tagName");
				}
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (value.Name != tagName)
				{
					throw new ArgumentException("Given tag name must match tag's actual name.");
				}
				if (value.Parent != null)
				{
					throw new ArgumentException("A tag may only be added to one compound/list at a time.");
				}
				if (value == this)
				{
					throw new ArgumentException("Cannot add tag to itself");
				}
				this.tags[tagName] = value;
				value.Parent = this;
			}
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00005BC8 File Offset: 0x00003DC8
		[CanBeNull]
		public T Get<T>([NotNull] string tagName) where T : NbtTag
		{
			if (tagName == null)
			{
				throw new ArgumentNullException("tagName");
			}
			NbtTag nbtTag;
			if (this.tags.TryGetValue(tagName, out nbtTag))
			{
				return (T)((object)nbtTag);
			}
			return default(T);
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00005C04 File Offset: 0x00003E04
		[CanBeNull]
		public NbtTag Get([NotNull] string tagName)
		{
			if (tagName == null)
			{
				throw new ArgumentNullException("tagName");
			}
			NbtTag result;
			if (this.tags.TryGetValue(tagName, out result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00005C34 File Offset: 0x00003E34
		public bool TryGet<T>([NotNull] string tagName, out T result) where T : NbtTag
		{
			if (tagName == null)
			{
				throw new ArgumentNullException("tagName");
			}
			NbtTag nbtTag;
			if (this.tags.TryGetValue(tagName, out nbtTag))
			{
				result = (T)((object)nbtTag);
				return true;
			}
			result = default(T);
			return false;
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00005C78 File Offset: 0x00003E78
		public bool TryGet([NotNull] string tagName, out NbtTag result)
		{
			if (tagName == null)
			{
				throw new ArgumentNullException("tagName");
			}
			NbtTag nbtTag;
			if (this.tags.TryGetValue(tagName, out nbtTag))
			{
				result = nbtTag;
				return true;
			}
			result = null;
			return false;
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00005CAC File Offset: 0x00003EAC
		public void AddRange([NotNull] IEnumerable<NbtTag> newTags)
		{
			if (newTags == null)
			{
				throw new ArgumentNullException("newTags");
			}
			foreach (NbtTag newTag in newTags)
			{
				this.Add(newTag);
			}
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00005D04 File Offset: 0x00003F04
		[Pure]
		public bool Contains([NotNull] string tagName)
		{
			if (tagName == null)
			{
				throw new ArgumentNullException("tagName");
			}
			return this.tags.ContainsKey(tagName);
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00005D20 File Offset: 0x00003F20
		public bool Remove([NotNull] string tagName)
		{
			if (tagName == null)
			{
				throw new ArgumentNullException("tagName");
			}
			NbtTag nbtTag;
			if (!this.tags.TryGetValue(tagName, out nbtTag))
			{
				return false;
			}
			this.tags.Remove(tagName);
			nbtTag.Parent = null;
			return true;
		}

		// Token: 0x06000113 RID: 275 RVA: 0x00005D64 File Offset: 0x00003F64
		internal void RenameTag([NotNull] string oldName, [NotNull] string newName)
		{
			NbtTag value;
			if (this.tags.TryGetValue(newName, out value))
			{
				throw new ArgumentException("Cannot rename: a tag with the name already exists in this compound.");
			}
			if (!this.tags.TryGetValue(oldName, out value))
			{
				throw new ArgumentException("Cannot rename: no tag found to rename.");
			}
			this.tags.Remove(oldName);
			this.tags.Add(newName, value);
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000114 RID: 276 RVA: 0x00005DC1 File Offset: 0x00003FC1
		[NotNull]
		public IEnumerable<string> Names
		{
			get
			{
				return this.tags.Keys;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000115 RID: 277 RVA: 0x00005DCE File Offset: 0x00003FCE
		[NotNull]
		public IEnumerable<NbtTag> Tags
		{
			get
			{
				return this.tags.Values;
			}
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00005DDC File Offset: 0x00003FDC
		internal override bool ReadTag(NbtBinaryReader readStream)
		{
			if (base.Parent != null && readStream.Selector != null && !readStream.Selector(this))
			{
				this.SkipTag(readStream);
				return false;
			}
			NbtTagType nbtTagType;
			for (;;)
			{
				nbtTagType = readStream.ReadTagType();
				NbtTag nbtTag;
				switch (nbtTagType)
				{
				case NbtTagType.End:
					return true;
				case NbtTagType.Byte:
					nbtTag = new NbtByte();
					goto IL_D8;
				case NbtTagType.Short:
					nbtTag = new NbtShort();
					goto IL_D8;
				case NbtTagType.Int:
					nbtTag = new NbtInt();
					goto IL_D8;
				case NbtTagType.Long:
					nbtTag = new NbtLong();
					goto IL_D8;
				case NbtTagType.Float:
					nbtTag = new NbtFloat();
					goto IL_D8;
				case NbtTagType.Double:
					nbtTag = new NbtDouble();
					goto IL_D8;
				case NbtTagType.ByteArray:
					nbtTag = new NbtByteArray();
					goto IL_D8;
				case NbtTagType.String:
					nbtTag = new NbtString();
					goto IL_D8;
				case NbtTagType.List:
					nbtTag = new NbtList();
					goto IL_D8;
				case NbtTagType.Compound:
					nbtTag = new NbtCompound();
					goto IL_D8;
				case NbtTagType.IntArray:
					nbtTag = new NbtIntArray();
					goto IL_D8;
				}
				break;
				IL_D8:
				nbtTag.Parent = this;
				nbtTag.Name = readStream.ReadString();
				if (nbtTag.ReadTag(readStream))
				{
					this.tags.Add(nbtTag.Name, nbtTag);
				}
			}
			throw new NbtFormatException("Unsupported tag type found in NBT_Compound: " + nbtTagType);
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00005EF8 File Offset: 0x000040F8
		internal override void SkipTag(NbtBinaryReader readStream)
		{
			NbtTagType nbtTagType;
			for (;;)
			{
				nbtTagType = readStream.ReadTagType();
				NbtTag nbtTag;
				switch (nbtTagType)
				{
				case NbtTagType.End:
					return;
				case NbtTagType.Byte:
					nbtTag = new NbtByte();
					goto IL_B0;
				case NbtTagType.Short:
					nbtTag = new NbtShort();
					goto IL_B0;
				case NbtTagType.Int:
					nbtTag = new NbtInt();
					goto IL_B0;
				case NbtTagType.Long:
					nbtTag = new NbtLong();
					goto IL_B0;
				case NbtTagType.Float:
					nbtTag = new NbtFloat();
					goto IL_B0;
				case NbtTagType.Double:
					nbtTag = new NbtDouble();
					goto IL_B0;
				case NbtTagType.ByteArray:
					nbtTag = new NbtByteArray();
					goto IL_B0;
				case NbtTagType.String:
					nbtTag = new NbtString();
					goto IL_B0;
				case NbtTagType.List:
					nbtTag = new NbtList();
					goto IL_B0;
				case NbtTagType.Compound:
					nbtTag = new NbtCompound();
					goto IL_B0;
				case NbtTagType.IntArray:
					nbtTag = new NbtIntArray();
					goto IL_B0;
				}
				break;
				IL_B0:
				readStream.SkipString();
				nbtTag.SkipTag(readStream);
			}
			throw new NbtFormatException("Unsupported tag type found in NBT_Compound: " + nbtTagType);
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00005FC6 File Offset: 0x000041C6
		internal override void WriteTag(NbtBinaryWriter writeStream)
		{
			writeStream.Write(NbtTagType.Compound);
			if (base.Name == null)
			{
				throw new NbtFormatException("Name is null");
			}
			writeStream.Write(base.Name);
			this.WriteData(writeStream);
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00005FF8 File Offset: 0x000041F8
		internal override void WriteData(NbtBinaryWriter writeStream)
		{
			foreach (NbtTag nbtTag in this.tags.Values)
			{
				nbtTag.WriteTag(writeStream);
			}
			writeStream.Write(NbtTagType.End);
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00006058 File Offset: 0x00004258
		public IEnumerator<NbtTag> GetEnumerator()
		{
			return this.tags.Values.GetEnumerator();
		}

		// Token: 0x0600011B RID: 283 RVA: 0x0000606F File Offset: 0x0000426F
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.tags.Values.GetEnumerator();
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00006088 File Offset: 0x00004288
		public void Add([NotNull] NbtTag newTag)
		{
			if (newTag == null)
			{
				throw new ArgumentNullException("newTag");
			}
			if (newTag == this)
			{
				throw new ArgumentException("Cannot add tag to self");
			}
			if (newTag.Name == null)
			{
				throw new ArgumentException("Only named tags are allowed in compound tags.");
			}
			if (newTag.Parent != null)
			{
				throw new ArgumentException("A tag may only be added to one compound/list at a time.");
			}
			this.tags.Add(newTag.Name, newTag);
			newTag.Parent = this;
		}

		// Token: 0x0600011D RID: 285 RVA: 0x000060F4 File Offset: 0x000042F4
		public void Clear()
		{
			foreach (NbtTag nbtTag in this.tags.Values)
			{
				nbtTag.Parent = null;
			}
			this.tags.Clear();
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00006158 File Offset: 0x00004358
		[Pure]
		public bool Contains([NotNull] NbtTag tag)
		{
			if (tag == null)
			{
				throw new ArgumentNullException("tag");
			}
			return this.tags.ContainsValue(tag);
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00006174 File Offset: 0x00004374
		public void CopyTo(NbtTag[] array, int arrayIndex)
		{
			this.tags.Values.CopyTo(array, arrayIndex);
		}

		// Token: 0x06000120 RID: 288 RVA: 0x00006188 File Offset: 0x00004388
		public bool Remove([NotNull] NbtTag tag)
		{
			if (tag == null)
			{
				throw new ArgumentNullException("tag");
			}
			if (tag.Name == null)
			{
				throw new ArgumentException("Trying to remove an unnamed tag.");
			}
			NbtTag nbtTag;
			if (this.tags.TryGetValue(tag.Name, out nbtTag) && nbtTag == tag && this.tags.Remove(tag.Name))
			{
				tag.Parent = null;
				return true;
			}
			return false;
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000121 RID: 289 RVA: 0x000061EC File Offset: 0x000043EC
		public int Count
		{
			get
			{
				return this.tags.Count;
			}
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x06000122 RID: 290 RVA: 0x000061F9 File Offset: 0x000043F9
		bool ICollection<NbtTag>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000123 RID: 291 RVA: 0x000061FC File Offset: 0x000043FC
		void ICollection.CopyTo(Array array, int index)
		{
			this.CopyTo((NbtTag[])array, index);
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000124 RID: 292 RVA: 0x0000620B File Offset: 0x0000440B
		object ICollection.SyncRoot
		{
			get
			{
				return ((ICollection)this.tags).SyncRoot;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000125 RID: 293 RVA: 0x00006218 File Offset: 0x00004418
		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000126 RID: 294 RVA: 0x0000621B File Offset: 0x0000441B
		public override object Clone()
		{
			return new NbtCompound(this);
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00006224 File Offset: 0x00004424
		internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
		{
			for (int i = 0; i < indentLevel; i++)
			{
				sb.Append(indentString);
			}
			sb.Append("TAG_Compound");
			if (!string.IsNullOrEmpty(base.Name))
			{
				sb.AppendFormat("(\"{0}\")", base.Name);
			}
			sb.AppendFormat(": {0} entries {{", this.tags.Count);
			if (this.Count > 0)
			{
				sb.Append('\n');
				foreach (NbtTag nbtTag in this.tags.Values)
				{
					nbtTag.PrettyPrint(sb, indentString, indentLevel + 1);
					sb.Append('\n');
				}
				for (int j = 0; j < indentLevel; j++)
				{
					sb.Append(indentString);
				}
			}
			sb.Append('}');
		}

		// Token: 0x0400006C RID: 108
		private readonly Dictionary<string, NbtTag> tags = new Dictionary<string, NbtTag>();
	}
}
