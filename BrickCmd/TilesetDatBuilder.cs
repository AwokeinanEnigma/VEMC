// Decompiled with JetBrains decompiler

using fNbt;
using System.Collections.Generic;
using System.Drawing;
using TiledSharp;

namespace VEMC
{
    internal static class TilesetDatBuilder
    {
        public static NbtFile Build(TmxTileset tmxTileset, OptimizedTileset optTileset)
        {
            NbtFile nbtFile = new NbtFile();
            NbtCompound rootTag = nbtFile.RootTag;
            rootTag.Name = tmxTileset.Name;
            int width = optTileset.Width;
            rootTag.Add(new NbtInt("w", width));
            rootTag.Add(new NbtByteArray("img", optTileset.IndexedImage));
            rootTag.Add(TilesetDatBuilder.CreatePaletteTag(optTileset.Palette));
            rootTag.Add(TilesetDatBuilder.CreateAnimationTag(tmxTileset, optTileset));
            return nbtFile;
        }

        private static NbtList CreatePaletteTag(Color[] colors)
        {
            int[] numArray = new int[colors.Length];
            for (int index = 0; index < numArray.Length; ++index)
            {
                numArray[index] = colors[index].ToArgb();
            }

            return new NbtList("pal", new List<NbtIntArray>()
      {
        new NbtIntArray(numArray)
      }, NbtTagType.IntArray);
        }

        private static NbtCompound CreateAnimationTag(
          TmxTileset tmxTileset,
          OptimizedTileset optTileset)
        {
            NbtCompound nbtCompound = new NbtCompound("spr");
            int num1 = 0;
            foreach (TmxTilesetTile tile in tmxTileset.Tiles)
            {
                if (tile.Properties.ContainsKey("animid"))
                {
                    int num2 = (int)tile.Properties.TryGetDecimal("animid");
                    int length = (int)tile.Properties.TryGetDecimal("frames");
                    int num3 = (int)tile.Properties.TryGetDecimal("vFrameSkip");
                    int num4 = (int)tile.Properties.TryGetDecimal("hFrameSkip");
                    float num5 = (float)tile.Properties.TryGetDecimal("speed");
                    int[] numArray = new int[length];
                    int num6 = tmxTileset.Image.Width / tmxTileset.TileWidth;
                    int num7 = tile.Id / num6;
                    bool flag = true;
                    for (int index = 0; index < length; ++index)
                    {
                        int num8 = tmxTileset.FirstGid + tile.Id + index * num4;
                        int num9 = num8 / num6;
                        int old = num8 + (num9 - num7) * (num6 * num3);
                        numArray[index] = optTileset.Translate(old);
                        flag = ((flag ? 1 : 0) & (index == 0 ? 1 : (numArray[index - 1] == numArray[index] ? 1 : 0))) != 0;
                    }
                    if (!flag)
                    {
                        tile.Properties.Add("actualid", num1.ToString());
                        nbtCompound.Add(new NbtCompound(num1.ToString())
            {
               new NbtIntArray("d", numArray),
               new NbtList("spd",  new List<NbtFloat>()
              {
                new NbtFloat(num5)
              })
            });
                        ++num1;
                    }
                }
            }
            return nbtCompound;
        }
    }
}
