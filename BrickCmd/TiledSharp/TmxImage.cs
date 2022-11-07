using System;
using System.IO;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x02000070 RID: 112
	public class TmxImage
	{
		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x060003F9 RID: 1017 RVA: 0x00019DFC File Offset: 0x00017FFC
		// (set) Token: 0x060003FA RID: 1018 RVA: 0x00019E04 File Offset: 0x00018004
		public string Format { get; private set; }

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x060003FB RID: 1019 RVA: 0x00019E0D File Offset: 0x0001800D
		// (set) Token: 0x060003FC RID: 1020 RVA: 0x00019E15 File Offset: 0x00018015
		public string Source { get; private set; }

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x060003FD RID: 1021 RVA: 0x00019E1E File Offset: 0x0001801E
		// (set) Token: 0x060003FE RID: 1022 RVA: 0x00019E26 File Offset: 0x00018026
		public Stream Data { get; private set; }

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x060003FF RID: 1023 RVA: 0x00019E2F File Offset: 0x0001802F
		// (set) Token: 0x06000400 RID: 1024 RVA: 0x00019E37 File Offset: 0x00018037
		public TmxColor Trans { get; private set; }

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x06000401 RID: 1025 RVA: 0x00019E40 File Offset: 0x00018040
		// (set) Token: 0x06000402 RID: 1026 RVA: 0x00019E48 File Offset: 0x00018048
		public int Width { get; private set; }

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x06000403 RID: 1027 RVA: 0x00019E51 File Offset: 0x00018051
		// (set) Token: 0x06000404 RID: 1028 RVA: 0x00019E59 File Offset: 0x00018059
		public int Height { get; private set; }

		// Token: 0x06000405 RID: 1029 RVA: 0x00019E64 File Offset: 0x00018064
		public TmxImage(XElement xImage, string tmxDir = "")
		{
			XAttribute xattribute = xImage.Attribute("source");
			if (xattribute != null)
			{
				this.Source = Path.Combine(tmxDir, (string)xattribute);
			}
			else
			{
				this.Format = (string)xImage.Attribute("format");
				XElement xData = xImage.Element("data");
				TmxBase64Data tmxBase64Data = new TmxBase64Data(xData);
				this.Data = tmxBase64Data.Data;
			}
			this.Trans = new TmxColor(xImage.Attribute("trans"));
			this.Width = (int)xImage.Attribute("width");
			this.Height = (int)xImage.Attribute("height");
		}
	}
}
