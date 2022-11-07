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
        public TmxTerrain TopLeft => TerrainEdges[0];
        public TmxTerrain TopRight => TerrainEdges[1];
        public TmxTerrain BottomLeft => TerrainEdges[2];
        public TmxTerrain BottomRight => TerrainEdges[3];
        public TmxTilesetTile(XElement xTile, TmxList<TmxTerrain> Terrains, string tmxDir = "")
        {
            Id = (int)xTile.Attribute("id");
            if (Terrains.Count > 0)
            {
                TerrainEdges = new List<TmxTerrain>(4);
                string[] array = ((string)xTile.Attribute("terrain")).Split(new char[]
                {
                    ','
                });
                foreach (string s in array)
                {
                    bool flag = int.TryParse(s, out int index);
                    TmxTerrain item;
                    if (flag)
                    {
                        item = Terrains[index];
                    }
                    else
                    {
                        item = null;
                    }
                    TerrainEdges.Add(item);
                }
            }
            double? num = (double?)xTile.Attribute("probability");
            Probability = ((num != null) ? num.GetValueOrDefault() : 1.0);
            XElement xelement = xTile.Element("image");
            if (xelement != null)
            {
                Image = new TmxImage(xelement, tmxDir);
            }
            Properties = new PropertyDict(xTile.Element("properties"));
        }
    }
}
