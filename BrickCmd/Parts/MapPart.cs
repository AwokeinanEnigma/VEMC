

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

        public NbtTag Tag => tag;

        public ICollection<NbtTag> Tags => tag as ICollection<NbtTag>;

        public MapPart(string name, bool isList)
        {
            Init(name, isList);
        }

        public MapPart(string name)
        {
            Init(name, false);
        }

        public MapPart(bool isList)
        {
            Init(null, isList);
        }

        private void Init(string name, bool isList)
        {
            this.name = name;
            tag = isList ? new NbtList(this.name, NbtTagType.Compound) : (NbtTag)new NbtCompound(this.name);
        }

        public void AddFromDictionary<T>(
          string name,
          Dictionary<string, string> dictionary,
          string key)
        {
            AddFromDictionary<T>(name, dictionary, key, default(T));
        }

        public void AddFromDictionary<T>(
          string name,
          Dictionary<string, string> dictionary,
          string key,
          T defValue)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            T obj;
            if (dictionary.TryGetValue(key, out string text))
            {
                try
                {
                    obj = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(text);
                }
                catch (Exception)
                {
                    obj = defValue;
                }
            }
            else
            {
                obj = defValue;
            }

            if (defValue == null && obj == null)
            {
                throw new MapPartRequirementException(this.name, key);
            }

            Add(name, obj);
        }

        public void Add(string name, object value)
        {
            if (tag is NbtCompound)
            {
                Type type = value.GetType();
                NbtTag newTag;
                switch (value)
                {
                    case long num:
                        newTag = new NbtLong(name, num);
                        break;
                    case int num:
                        newTag = new NbtInt(name, num);
                        break;
                    case short num:
                        newTag = new NbtShort(name, num);
                        break;
                    case byte num:
                        newTag = new NbtByte(name, num);
                        break;
                    case float num:
                        newTag = new NbtFloat(name, num);
                        break;
                    case double num:
                        newTag = new NbtDouble(name, num);
                        break;
                    case string _:
                        newTag = new NbtString(name, (string)value);
                        break;
                    case int[] _:
                        newTag = new NbtIntArray(name, (int[])value);
                        break;
                    case byte[] _:
                        newTag = new NbtByteArray(name, (byte[])value);
                        break;
                    case bool flag:
                        byte num1 = flag ? (byte)1 : (byte)0;
                        newTag = new NbtByte(name, num1);
                        break;
                    default:
                        throw new MapPartParameterException(this.name, name, type);
                }
              ((NbtCompound)tag).Add(newTag);
            }
            else if (tag is NbtList)
            {
                throw new InvalidOperationException("Only compound tags can be added to list MapParts");
            }
        }

        public void Add(NbtTag tag)
        {
            if (tag == null)
            {
                return;
            }

            if (this.tag is NbtCompound)
            {
                ((NbtCompound)this.tag).Add(tag);
            }
            else
            {
                if (!(this.tag is NbtList) || !(tag is NbtCompound))
                {
                    throw new InvalidOperationException("Only compound tags can be added to list MapParts");
                } ((NbtList)this.tag).Add(tag);
            }
        }

        public void Add(MapPart part)
        {
            if (part == null)
            {
                return;
            }

            if (tag is NbtCompound)
            {
                ((NbtCompound)tag).Add(part.tag);
            }
            else
            {
                if (!(tag is NbtList) || !(part.tag is NbtCompound))
                {
                    throw new InvalidOperationException("Only compound tags can be added to list MapParts");
                } ((NbtList)tag).Add(part.tag);
            }
        }
    }
}
