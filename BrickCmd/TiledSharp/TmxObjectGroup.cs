using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x0200006A RID: 106
	public class TmxObjectGroup : ITmxElement
	{
		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x060003C9 RID: 969 RVA: 0x000192C4 File Offset: 0x000174C4
		// (set) Token: 0x060003CA RID: 970 RVA: 0x000192CC File Offset: 0x000174CC
		public string Name { get; private set; }

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x060003CB RID: 971 RVA: 0x000192D5 File Offset: 0x000174D5
		// (set) Token: 0x060003CC RID: 972 RVA: 0x000192DD File Offset: 0x000174DD
		public TmxColor Color { get; private set; }

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x060003CD RID: 973 RVA: 0x000192E6 File Offset: 0x000174E6
		// (set) Token: 0x060003CE RID: 974 RVA: 0x000192EE File Offset: 0x000174EE
		public double Opacity { get; private set; }

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x060003CF RID: 975 RVA: 0x000192F7 File Offset: 0x000174F7
		// (set) Token: 0x060003D0 RID: 976 RVA: 0x000192FF File Offset: 0x000174FF
		public bool Visible { get; private set; }

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x060003D1 RID: 977 RVA: 0x00019308 File Offset: 0x00017508
		// (set) Token: 0x060003D2 RID: 978 RVA: 0x00019310 File Offset: 0x00017510
		public TmxList<TmxObjectGroup.TmxObject> Objects { get; private set; }

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x060003D3 RID: 979 RVA: 0x00019319 File Offset: 0x00017519
		// (set) Token: 0x060003D4 RID: 980 RVA: 0x00019321 File Offset: 0x00017521
		public PropertyDict Properties { get; private set; }

		// Token: 0x060003D5 RID: 981 RVA: 0x0001932C File Offset: 0x0001752C
		public TmxObjectGroup(XElement xObjectGroup)
		{
			this.Name = (string)xObjectGroup.Attribute("name");
			this.Color = new TmxColor(xObjectGroup.Attribute("color"));
			double? num = (double?)xObjectGroup.Attribute("opacity");
			this.Opacity = ((num != null) ? num.GetValueOrDefault() : 1.0);
			this.Visible = (((bool?)xObjectGroup.Attribute("visible")) ?? true);
			this.Objects = new TmxList<TmxObjectGroup.TmxObject>();
			foreach (XElement xObject in xObjectGroup.Elements("object"))
			{
				this.Objects.Add(new TmxObjectGroup.TmxObject(xObject));
			}
			this.Properties = new PropertyDict(xObjectGroup.Element("properties"));
		}

		// Token: 0x0200006B RID: 107
		public class TmxObject : ITmxElement
		{
			// Token: 0x170000D8 RID: 216
			// (get) Token: 0x060003D6 RID: 982 RVA: 0x00019458 File Offset: 0x00017658
			// (set) Token: 0x060003D7 RID: 983 RVA: 0x00019460 File Offset: 0x00017660
			public string Name { get; private set; }

			// Token: 0x170000D9 RID: 217
			// (get) Token: 0x060003D8 RID: 984 RVA: 0x00019469 File Offset: 0x00017669
			// (set) Token: 0x060003D9 RID: 985 RVA: 0x00019471 File Offset: 0x00017671
			public TmxObjectGroup.TmxObjectType ObjectType { get; private set; }

			// Token: 0x170000DA RID: 218
			// (get) Token: 0x060003DA RID: 986 RVA: 0x0001947A File Offset: 0x0001767A
			// (set) Token: 0x060003DB RID: 987 RVA: 0x00019482 File Offset: 0x00017682
			public string Type { get; private set; }

			// Token: 0x170000DB RID: 219
			// (get) Token: 0x060003DC RID: 988 RVA: 0x0001948B File Offset: 0x0001768B
			// (set) Token: 0x060003DD RID: 989 RVA: 0x00019493 File Offset: 0x00017693
			public int X { get; private set; }

			// Token: 0x170000DC RID: 220
			// (get) Token: 0x060003DE RID: 990 RVA: 0x0001949C File Offset: 0x0001769C
			// (set) Token: 0x060003DF RID: 991 RVA: 0x000194A4 File Offset: 0x000176A4
			public int Y { get; private set; }

			// Token: 0x170000DD RID: 221
			// (get) Token: 0x060003E0 RID: 992 RVA: 0x000194AD File Offset: 0x000176AD
			// (set) Token: 0x060003E1 RID: 993 RVA: 0x000194B5 File Offset: 0x000176B5
			public int Width { get; private set; }

			// Token: 0x170000DE RID: 222
			// (get) Token: 0x060003E2 RID: 994 RVA: 0x000194BE File Offset: 0x000176BE
			// (set) Token: 0x060003E3 RID: 995 RVA: 0x000194C6 File Offset: 0x000176C6
			public int Height { get; private set; }

			// Token: 0x170000DF RID: 223
			// (get) Token: 0x060003E4 RID: 996 RVA: 0x000194CF File Offset: 0x000176CF
			// (set) Token: 0x060003E5 RID: 997 RVA: 0x000194D7 File Offset: 0x000176D7
			public double Rotation { get; private set; }

			// Token: 0x170000E0 RID: 224
			// (get) Token: 0x060003E6 RID: 998 RVA: 0x000194E0 File Offset: 0x000176E0
			// (set) Token: 0x060003E7 RID: 999 RVA: 0x000194E8 File Offset: 0x000176E8
			public TmxLayerTile Tile { get; private set; }

			// Token: 0x170000E1 RID: 225
			// (get) Token: 0x060003E8 RID: 1000 RVA: 0x000194F1 File Offset: 0x000176F1
			// (set) Token: 0x060003E9 RID: 1001 RVA: 0x000194F9 File Offset: 0x000176F9
			public bool Visible { get; private set; }

			// Token: 0x170000E2 RID: 226
			// (get) Token: 0x060003EA RID: 1002 RVA: 0x00019502 File Offset: 0x00017702
			// (set) Token: 0x060003EB RID: 1003 RVA: 0x0001950A File Offset: 0x0001770A
			public List<Tuple<int, int>> Points { get; private set; }

			// Token: 0x170000E3 RID: 227
			// (get) Token: 0x060003EC RID: 1004 RVA: 0x00019513 File Offset: 0x00017713
			// (set) Token: 0x060003ED RID: 1005 RVA: 0x0001951B File Offset: 0x0001771B
			public PropertyDict Properties { get; private set; }

			// Token: 0x060003EE RID: 1006 RVA: 0x00019524 File Offset: 0x00017724
			public TmxObject(XElement xObject)
			{
				this.Name = (((string)xObject.Attribute("name")) ?? "");
				this.Type = (string)xObject.Attribute("type");
				this.X = (int)((float)xObject.Attribute("x"));
				this.Y = (int)((float)xObject.Attribute("y"));
				this.Visible = (((bool?)xObject.Attribute("visible")) ?? true);
				float? num = (float?)xObject.Attribute("width");
				this.Width = (int)((num != null) ? num.GetValueOrDefault() : 0f);
				float? num2 = (float?)xObject.Attribute("height");
				this.Height = (int)((num2 != null) ? num2.GetValueOrDefault() : 0f);
				double? num3 = (double?)xObject.Attribute("rotation");
				this.Rotation = ((num3 != null) ? num3.GetValueOrDefault() : 0.0);
				XAttribute xattribute = xObject.Attribute("gid");
				XElement xelement = xObject.Element("ellipse");
				XElement xelement2 = xObject.Element("polygon");
				XElement xelement3 = xObject.Element("polyline");
				if (xattribute != null)
				{
					this.Tile = new TmxLayerTile((uint)xattribute, this.X, this.Y);
					this.ObjectType = TmxObjectGroup.TmxObjectType.Tile;
				}
				else if (xelement != null)
				{
					this.ObjectType = TmxObjectGroup.TmxObjectType.Ellipse;
				}
				else if (xelement2 != null)
				{
					this.Points = this.ParsePoints(xelement2);
					this.ObjectType = TmxObjectGroup.TmxObjectType.Polygon;
				}
				else if (xelement3 != null)
				{
					this.Points = this.ParsePoints(xelement3);
					this.ObjectType = TmxObjectGroup.TmxObjectType.Polyline;
				}
				else
				{
					this.ObjectType = TmxObjectGroup.TmxObjectType.Basic;
				}
				this.Properties = new PropertyDict(xObject.Element("properties"));
			}

			// Token: 0x060003EF RID: 1007 RVA: 0x00019758 File Offset: 0x00017958
			public List<Tuple<int, int>> ParsePoints(XElement xPoints)
			{
				List<Tuple<int, int>> list = new List<Tuple<int, int>>();
				string text = (string)xPoints.Attribute("points");
				string[] array = text.Split(new char[]
				{
					' '
				});
				foreach (string text2 in array)
				{
					string[] array3 = text2.Split(new char[]
					{
						','
					});
					float num = float.Parse(array3[0]);
					float num2 = float.Parse(array3[1]);
					int item = (int)Math.Round((double)num);
					int item2 = (int)Math.Round((double)num2);
					list.Add(Tuple.Create<int, int>(item, item2));
				}
				return list;
			}
		}

		// Token: 0x0200006C RID: 108
		public enum TmxObjectType : byte
		{
			// Token: 0x0400028E RID: 654
			Basic,
			// Token: 0x0400028F RID: 655
			Tile,
			// Token: 0x04000290 RID: 656
			Ellipse,
			// Token: 0x04000291 RID: 657
			Polygon,
			// Token: 0x04000292 RID: 658
			Polyline
		}
	}
}
