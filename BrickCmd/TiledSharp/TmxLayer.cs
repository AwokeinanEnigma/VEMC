using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x02000065 RID: 101
	public class TmxLayer : ITmxElement
	{
		// Token: 0x170000BA RID: 186
		// (get) Token: 0x06000394 RID: 916 RVA: 0x00018ABC File Offset: 0x00016CBC
		// (set) Token: 0x06000395 RID: 917 RVA: 0x00018AC4 File Offset: 0x00016CC4
		public string Name { get; private set; }

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x06000396 RID: 918 RVA: 0x00018ACD File Offset: 0x00016CCD
		// (set) Token: 0x06000397 RID: 919 RVA: 0x00018AD5 File Offset: 0x00016CD5
		public double Opacity { get; private set; }

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x06000398 RID: 920 RVA: 0x00018ADE File Offset: 0x00016CDE
		// (set) Token: 0x06000399 RID: 921 RVA: 0x00018AE6 File Offset: 0x00016CE6
		public bool Visible { get; private set; }

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x0600039A RID: 922 RVA: 0x00018AEF File Offset: 0x00016CEF
		// (set) Token: 0x0600039B RID: 923 RVA: 0x00018AF7 File Offset: 0x00016CF7
		public List<TmxLayerTile> Tiles { get; private set; }

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x0600039C RID: 924 RVA: 0x00018B00 File Offset: 0x00016D00
		// (set) Token: 0x0600039D RID: 925 RVA: 0x00018B08 File Offset: 0x00016D08
		public PropertyDict Properties { get; private set; }

		// Token: 0x0600039E RID: 926 RVA: 0x00018B14 File Offset: 0x00016D14
		public TmxLayer(XElement xLayer, int width, int height)
		{
			this.Name = (string)xLayer.Attribute("name");
			double? num = (double?)xLayer.Attribute("opacity");
			this.Opacity = ((num != null) ? num.GetValueOrDefault() : 1.0);
			this.Visible = (((bool?)xLayer.Attribute("visible")) ?? true);
			XElement xelement = xLayer.Element("data");
			string text = (string)xelement.Attribute("encoding");
			this.Tiles = new List<TmxLayerTile>();
			if (text == "base64")
			{
				TmxBase64Data tmxBase64Data = new TmxBase64Data(xelement);
				Stream data = tmxBase64Data.Data;
				using (BinaryReader binaryReader = new BinaryReader(data))
				{
					for (int i = 0; i < height; i++)
					{
						for (int j = 0; j < width; j++)
						{
							this.Tiles.Add(new TmxLayerTile(binaryReader.ReadUInt32(), j, i));
						}
					}
					goto IL_23C;
				}
			}
			if (!(text == "csv"))
			{
				if (text == null)
				{
					int num2 = 0;
					using (IEnumerator<XElement> enumerator = xelement.Elements("tile").GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							XElement xelement2 = enumerator.Current;
							uint id = (uint)xelement2.Attribute("gid");
							int x = num2 % width;
							int y = num2 / width;
							this.Tiles.Add(new TmxLayerTile(id, x, y));
							num2++;
						}
						goto IL_23C;
					}
				}
				throw new Exception("TmxLayer: Unknown encoding.");
			}
			string value = xelement.Value;
			int num3 = 0;
			foreach (string text2 in value.Split(new char[]
			{
				','
			}))
			{
				uint id2 = uint.Parse(text2.Trim());
				int x2 = num3 % width;
				int y2 = num3 / width;
				this.Tiles.Add(new TmxLayerTile(id2, x2, y2));
				num3++;
			}
			IL_23C:
			this.Properties = new PropertyDict(xLayer.Element("properties"));
		}
	}
}
