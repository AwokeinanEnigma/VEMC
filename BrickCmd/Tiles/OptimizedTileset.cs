using System.Collections.Generic;
using System.Drawing;

namespace VEMC
{
    public class OptimizedTileset
    {
        public int Width { get; set; }
        public Dictionary<int, int> TranslationTable { get; set; }
        public Color[] Palette { get; set; }
        public byte[] IndexedImage { get; set; }
        public OptimizedTileset()
        {
            TranslationTable = new Dictionary<int, int>();
        }
        public int Translate(int old)
        {

            return TranslationTable[old];
        }
    }
}
