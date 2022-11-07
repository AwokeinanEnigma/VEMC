using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	public sealed class NbtCompound : NbtTag, ICollection<NbtTag>, IEnumerable<NbtTag>, ICollection, IEnumerable
	{
		public override NbtTagType TagType
		{
			get
			{
				return NbtTagType.Compound;
			}
		}
		public NbtCompound()
		{
		}
		public NbtCompound([CanBeNull] string tagName)
		{
			this.name = tagName;
		}
		public NbtCompound([NotNull] IEnumerable<NbtTag> tags) : this(null, tags)
		{
		}
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
		[Pure]
		public bool Contains([NotNull] string tagName)
		{
			if (tagName == null)
			{
				throw new ArgumentNullException("tagName");
			}
			return this.tags.ContainsKey(tagName);
		}
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
		[NotNull]
		public IEnumerable<string> Names
		{
			get
			{
				return this.tags.Keys;
			}
		}
		[NotNull]
		public IEnumerable<NbtTag> Tags
		{
			get
			{
				return this.tags.Values;
			}
		}
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
		internal override void WriteData(NbtBinaryWriter writeStream)
		{
			foreach (NbtTag nbtTag in this.tags.Values)
			{
				nbtTag.WriteTag(writeStream);
			}
			writeStream.Write(NbtTagType.End);
		}
		public IEnumerator<NbtTag> GetEnumerator()
		{
			return this.tags.Values.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.tags.Values.GetEnumerator();
		}
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
		public void Clear()
		{
			foreach (NbtTag nbtTag in this.tags.Values)
			{
				nbtTag.Parent = null;
			}
			this.tags.Clear();
		}
		[Pure]
		public bool Contains([NotNull] NbtTag tag)
		{
			if (tag == null)
			{
				throw new ArgumentNullException("tag");
			}
			return this.tags.ContainsValue(tag);
		}
		public void CopyTo(NbtTag[] array, int arrayIndex)
		{
			this.tags.Values.CopyTo(array, arrayIndex);
		}
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
		public int Count
		{
			get
			{
				return this.tags.Count;
			}
		}
		bool ICollection<NbtTag>.IsReadOnly
		{
			get
			{
				return false;
			}
		}
		void ICollection.CopyTo(Array array, int index)
		{
			this.CopyTo((NbtTag[])array, index);
		}
		object ICollection.SyncRoot
		{
			get
			{
				return ((ICollection)this.tags).SyncRoot;
			}
		}
		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}
		public override object Clone()
		{
			return new NbtCompound(this);
		}
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
		private readonly Dictionary<string, NbtTag> tags = new Dictionary<string, NbtTag>();
	}
}
