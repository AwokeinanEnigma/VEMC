using System;
using System.Collections.Generic;
using System.Drawing;

namespace VEMC
{
	internal class OptimizedTileset
	{
		public int Width { get; set; }
		public Dictionary<int, int> TranslationTable { get; set; }
		public Color[] Palette { get; set; }
		public byte[] IndexedImage { get; set; }
		public OptimizedTileset()
		{
			this.TranslationTable = new Dictionary<int, int>();
		}
		public int Translate(int old)
		{

			return this.TranslationTable[old];
		}
	}
}
