using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace TiledSharp
{
	public class TmxTilesetTile
	{
		public int Id { get; private set; }
		public List<TmxTerrain> TerrainEdges { get; private set; }
		public double Probability { get; private set; }
		public TmxImage Image { get; private set; }
		public PropertyDict Properties { get; private set; }
		public TmxTerrain TopLeft
		{
			get
			{
				return this.TerrainEdges[0];
			}
		}
		public TmxTerrain TopRight
		{
			get
			{
				return this.TerrainEdges[1];
			}
		}
		public TmxTerrain BottomLeft
		{
			get
			{
				return this.TerrainEdges[2];
			}
		}
		public TmxTerrain BottomRight
		{
			get
			{
				return this.TerrainEdges[3];
			}
		}
		public TmxTilesetTile(XElement xTile, TmxList<TmxTerrain> Terrains, string tmxDir = "")
		{
			this.Id = (int)xTile.Attribute("id");
			if (Terrains.Count > 0)
			{
				this.TerrainEdges = new List<TmxTerrain>(4);
				string[] array = ((string)xTile.Attribute("terrain")).Split(new char[]
				{
					','
				});
				foreach (string s in array)
				{
					int index;
					bool flag = int.TryParse(s, out index);
					TmxTerrain item;
					if (flag)
					{
						item = Terrains[index];
					}
					else
					{
						item = null;
					}
					this.TerrainEdges.Add(item);
				}
			}
			double? num = (double?)xTile.Attribute("probability");
			this.Probability = ((num != null) ? num.GetValueOrDefault() : 1.0);
			XElement xelement = xTile.Element("image");
			if (xelement != null)
			{
				this.Image = new TmxImage(xelement, tmxDir);
			}
			this.Properties = new PropertyDict(xTile.Element("properties"));
		}
	}
}
