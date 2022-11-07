// Decompiled with JetBrains decompiler

using fNbt;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace VEMC.Parts
{
    internal class MapPart
    {
        protected NbtTag tag;
        protected string name;

        public NbtTag Tag
        {
            get
            {
                return this.tag;
            }
        }

        public ICollection<NbtTag> Tags
        {
            get
            {
                return this.tag as ICollection<NbtTag>;
            }
        }

        public MapPart(string name, bool isList)
        {
            this.Init(name, isList);
        }

        public MapPart(string name)
        {
            this.Init(name, false);
        }

        public MapPart(bool isList)
        {
            this.Init((string)null, isList);
        }

        private void Init(string name, bool isList)
        {
            this.name = name;
            this.tag = isList ? (NbtTag)new NbtList(this.name, NbtTagType.Compound) : (NbtTag)new NbtCompound(this.name);
        }

        public void AddFromDictionary<T>(
          string name,
          Dictionary<string, string> dictionary,
          string key)
        {
            this.AddFromDictionary<T>(name, dictionary, key, default(T));
        }

        public void AddFromDictionary<T>(
          string name,
          Dictionary<string, string> dictionary,
          string key,
          T defValue)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            string text = (string)null;
            T obj;
            if (dictionary.TryGetValue(key, out text))
            {
                try
                {
                    obj = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(text);
                }
                catch (Exception ex)
                {
                    obj = defValue;
                }
            }
            else
                obj = defValue;
            if ((object)defValue == null && (object)obj == null)
                throw new MapPartRequirementException(this.name, key);
            this.Add(name, (object)obj);
        }

        public void Add(string name, object value)
        {
            if (this.tag is NbtCompound)
            {
                Type type = value.GetType();
                NbtTag newTag;
                switch (value)
                {
                    case long num:
                        newTag = (NbtTag)new NbtLong(name, num);
                        break;
                    case int num:
                        newTag = (NbtTag)new NbtInt(name, num);
                        break;
                    case short num:
                        newTag = (NbtTag)new NbtShort(name, num);
                        break;
                    case byte num:
                        newTag = (NbtTag)new NbtByte(name, num);
                        break;
                    case float num:
                        newTag = (NbtTag)new NbtFloat(name, num);
                        break;
                    case double num:
                        newTag = (NbtTag)new NbtDouble(name, num);
                        break;
                    case string _:
                        newTag = (NbtTag)new NbtString(name, (string)value);
                        break;
                    case int[] _:
                        newTag = (NbtTag)new NbtIntArray(name, (int[])value);
                        break;
                    case byte[] _:
                        newTag = (NbtTag)new NbtByteArray(name, (byte[])value);
                        break;
                    case bool flag:
                        byte num1 = flag ? (byte)1 : (byte)0;
                        newTag = (NbtTag)new NbtByte(name, num1);
                        break;
                    default:
                        throw new MapPartParameterException(this.name, name, type);
                }
              ((NbtCompound)this.tag).Add(newTag);
            }
            else if (this.tag is NbtList)
                throw new InvalidOperationException("Only compound tags can be added to list MapParts");
        }

        public void Add(NbtTag tag)
        {
            if (tag == null)
                return;
            if (this.tag is NbtCompound)
            {
                ((NbtCompound)this.tag).Add(tag);
            }
            else
            {
                if (!(this.tag is NbtList) || !(tag is NbtCompound))
                    throw new InvalidOperationException("Only compound tags can be added to list MapParts");
                ((NbtList)this.tag).Add(tag);
            }
        }

        public void Add(MapPart part)
        {
            if (part == null)
                return;
            if (this.tag is NbtCompound)
            {
                ((NbtCompound)this.tag).Add(part.tag);
            }
            else
            {
                if (!(this.tag is NbtList) || !(part.tag is NbtCompound))
                    throw new InvalidOperationException("Only compound tags can be added to list MapParts");
                ((NbtList)this.tag).Add(part.tag);
            }
        }
    }
}
