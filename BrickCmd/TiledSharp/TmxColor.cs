using System;
using System.Globalization;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x02000071 RID: 113
	public class TmxColor
	{
		// Token: 0x06000406 RID: 1030 RVA: 0x00019F30 File Offset: 0x00018130
		public TmxColor(XAttribute xColor)
		{
			if (xColor == null)
			{
				return;
			}
			string text = ((string)xColor).TrimStart("#".ToCharArray());
			this.R = int.Parse(text.Substring(0, 2), NumberStyles.HexNumber);
			this.G = int.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
			this.B = int.Parse(text.Substring(4, 2), NumberStyles.HexNumber);
		}

		// Token: 0x0400029A RID: 666
		public int R;

		// Token: 0x0400029B RID: 667
		public int G;

		// Token: 0x0400029C RID: 668
		public int B;
	}
}
