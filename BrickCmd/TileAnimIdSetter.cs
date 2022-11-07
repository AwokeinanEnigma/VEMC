// Decompiled with JetBrains decompiler
// Type: VEMC.TileAnimIdSetter
// Assembly: VEMC, Version=3.5.2.0, Culture=neutral, PublicKeyToken=null
// MVID: D31E32F7-EF00-4E54-8BC6-F1DFA3DF5B6D
// Assembly location: C:\Users\Tom\Documents\Mother4RestoredENIGMA_COPY\Mother4\bin\Debug\Brickroad.exe

using System.Collections.Generic;
using TiledSharp;

namespace VEMC
{
    internal class TileAnimIdSetter
    {
        private TmxTileset tileset;
        private IDictionary<int, ushort> animationMap;

        public TileAnimIdSetter(TmxTileset tileset)
        {
            this.tileset = tileset;
            this.BuildAnimationMap();
        }

        private void BuildAnimationMap()
        {
            this.animationMap = (IDictionary<int, ushort>)new Dictionary<int, ushort>();
            ushort num1 = 0;
            foreach (TmxTilesetTile tile in this.tileset.Tiles)
            {
                if (tile.Properties.ContainsKey("animid"))
                {
                    int num2 = (int)tile.Properties.TryGetDecimal("animid");
                    int length = (int)tile.Properties.TryGetDecimal("frames");
                    int num3 = (int)tile.Properties.TryGetDecimal("vFrameSkip");
                    int num4 = (int)tile.Properties.TryGetDecimal("hFrameSkip");
                    double num5 = (double)(float)tile.Properties.TryGetDecimal("speed");
                    int[] numArray = new int[length];
                    int num6 = this.tileset.Image.Width / this.tileset.TileWidth;
                    int num7 = tile.Id / num6;
                    bool flag = true;
                    for (int index = 0; index < length; ++index)
                    {
                        int num8 = this.tileset.FirstGid + tile.Id + index * num4;
                        int num9 = num8 / num6;
                        int num10 = num8 + (num9 - num7) * (num6 * num3);
                        numArray[index] = num10;
                        flag = ((flag ? 1 : 0) & (index == 0 ? 1 : (numArray[index - 1] == numArray[index] ? 1 : 0))) != 0;
                    }
                    if (!flag)
                    {
                        for (int index = 0; index < numArray.Length; ++index)
                            this.animationMap.Add(numArray[index], num1);
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
                    ushort num = 0;
                    this.animationMap.TryGetValue((int)tileInts[index], out num);
                    tileInts[index] = (uint)num << 19 | tileInts[index];
                }
            }
        }
    }
}
