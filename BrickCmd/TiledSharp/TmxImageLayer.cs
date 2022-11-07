using System;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x02000037 RID: 55
	public class TmxImageLayer : ITmxElement
	{
		// Token: 0x17000071 RID: 113
		// (get) Token: 0x0600021B RID: 539 RVA: 0x0000908A File Offset: 0x0000728A
		// (set) Token: 0x0600021C RID: 540 RVA: 0x00009092 File Offset: 0x00007292
		public string Name { get; private set; }

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600021D RID: 541 RVA: 0x0000909B File Offset: 0x0000729B
		// (set) Token: 0x0600021E RID: 542 RVA: 0x000090A3 File Offset: 0x000072A3
		public int Width { get; private set; }

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x0600021F RID: 543 RVA: 0x000090AC File Offset: 0x000072AC
		// (set) Token: 0x06000220 RID: 544 RVA: 0x000090B4 File Offset: 0x000072B4
		public int Height { get; private set; }

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000221 RID: 545 RVA: 0x000090BD File Offset: 0x000072BD
		// (set) Token: 0x06000222 RID: 546 RVA: 0x000090C5 File Offset: 0x000072C5
		public bool Visible { get; private set; }

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000223 RID: 547 RVA: 0x000090CE File Offset: 0x000072CE
		// (set) Token: 0x06000224 RID: 548 RVA: 0x000090D6 File Offset: 0x000072D6
		public double Opacity { get; private set; }

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000225 RID: 549 RVA: 0x000090DF File Offset: 0x000072DF
		// (set) Token: 0x06000226 RID: 550 RVA: 0x000090E7 File Offset: 0x000072E7
		public TmxImage Image { get; private set; }

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000227 RID: 551 RVA: 0x000090F0 File Offset: 0x000072F0
		// (set) Token: 0x06000228 RID: 552 RVA: 0x000090F8 File Offset: 0x000072F8
		public PropertyDict Properties { get; private set; }

		// Token: 0x06000229 RID: 553 RVA: 0x00009104 File Offset: 0x00007304
		public TmxImageLayer(XElement xImageLayer, string tmxDir = "")
		{
			this.Name = (string)xImageLayer.Attribute("name");
			this.Width = (int)xImageLayer.Attribute("width");
			this.Height = (int)xImageLayer.Attribute("height");
			this.Visible = (((bool?)xImageLayer.Attribute("visible")) ?? true);
			double? num = (double?)xImageLayer.Attribute("opacity");
			this.Opacity = ((num != null) ? num.GetValueOrDefault() : 1.0);
			this.Image = new TmxImage(xImageLayer.Element("image"), tmxDir);
			this.Properties = new PropertyDict(xImageLayer.Element("properties"));
		}
	}
}
