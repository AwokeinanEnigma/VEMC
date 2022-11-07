using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace TiledSharp
{
	public class TmxObjectGroup : ITmxElement
	{
		public string Name { get; private set; }
		public TmxColor Color { get; private set; }
		public double Opacity { get; private set; }
		public bool Visible { get; private set; }
		public TmxList<TmxObjectGroup.TmxObject> Objects { get; private set; }
		public PropertyDict Properties { get; private set; }
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
		public class TmxObject : ITmxElement
		{
			public string Name { get; private set; }
			public TmxObjectGroup.TmxObjectType ObjectType { get; private set; }
			public string Type { get; private set; }
			public int X { get; private set; }
			public int Y { get; private set; }
			public int Width { get; private set; }
			public int Height { get; private set; }
			public double Rotation { get; private set; }
			public TmxLayerTile Tile { get; private set; }
			public bool Visible { get; private set; }
			public List<Tuple<int, int>> Points { get; private set; }
			public PropertyDict Properties { get; private set; }
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
		public enum TmxObjectType : byte
		{
			Basic,
			Tile,
			Ellipse,
			Polygon,
			Polyline
		}
	}
}
