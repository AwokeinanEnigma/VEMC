// Decompiled with JetBrains decompiler

using System.Collections.Generic;
using TiledSharp;

namespace VEMC
{
    internal class TileAnimIdSetter
    {
        private readonly TmxTileset tileset;
        private IDictionary<int, ushort> animationMap;

        public TileAnimIdSetter(TmxTileset tileset)
        {
            this.tileset = tileset;
            BuildAnimationMap();
        }

        private void BuildAnimationMap()
        {
            animationMap = new Dictionary<int, ushort>();
            ushort num1 = 0;
            foreach (TmxTilesetTile tile in tileset.Tiles.Values)
            {
                if (tile.Properties.ContainsKey("animid"))
                {
                    int num2 = (int)tile.Properties.TryGetDecimal("animid");
                    int length = (int)tile.Properties.TryGetDecimal("frames");
                    int num3 = (int)tile.Properties.TryGetDecimal("vFrameSkip");
                    int num4 = (int)tile.Properties.TryGetDecimal("hFrameSkip");
                    double num5 = (float)tile.Properties.TryGetDecimal("speed");
                    int[] numArray = new int[length];
                    int num6 = tileset.Image.Width.Value / tileset.TileWidth;
                    int num7 = tile.Id / num6;
                    bool flag = true;
                    for (int index = 0; index < length; ++index)
                    {
                        int num8 = tileset.FirstGid + tile.Id + index * num4;
                        int num9 = num8 / num6;
                        int num10 = num8 + (num9 - num7) * (num6 * num3);
                        numArray[index] = num10;
                        flag = ((flag ? 1 : 0) & (index == 0 ? 1 : (numArray[index - 1] == numArray[index] ? 1 : 0))) != 0;
                    }
                    if (!flag)
                    {
                        for (int index = 0; index < numArray.Length; ++index)
                        {
                            animationMap.Add(numArray[index], num1);
                        }

                        ++num1;
                    }
                }
            }
        }

        public void SetAnimIds(uint[] tileInts)
        {
            for (int index = 0; index < tileInts.Length; ++index)
            {
                if (tileInts[index] > 0U)
                {
                    animationMap.TryGetValue((int)tileInts[index], out ushort num);
                    tileInts[index] = (uint)num << 19 | tileInts[index];
                }
            }
        }
    }
}
