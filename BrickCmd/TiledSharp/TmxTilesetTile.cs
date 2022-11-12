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
        public string Type { get; private set; }

        public PropertyDict Properties { get; private set; }
        public TmxImage Image { get; private set; }
        public TmxList<TmxObjectGroup> ObjectGroups { get; private set; }

        // Human-readable aliases to the Terrain markers
        public TmxTerrain TopLeft
        {
            get { return TerrainEdges[0]; }
        }

        public TmxTerrain TopRight
        {
            get { return TerrainEdges[1]; }
        }

        public TmxTerrain BottomLeft
        {
            get { return TerrainEdges[2]; }
        }
        public TmxTerrain BottomRight
        {
            get { return TerrainEdges[3]; }
        }

        public TmxTilesetTile(XElement xTile, TmxList<TmxTerrain> Terrains,
                       string tmxDir = "")
        {
            Id = (int)xTile.Attribute("id");

            TerrainEdges = new List<TmxTerrain>();

            int result;
            TmxTerrain edge;

            var strTerrain = (string)xTile.Attribute("terrain") ?? ",,,";
            foreach (var v in strTerrain.Split(','))
            {
                var success = int.TryParse(v, out result);
                if (success)
                    edge = Terrains[result];
                else
                    edge = null;
                TerrainEdges.Add(edge);

                // TODO: Assert that TerrainEdges length is 4
            }


            Probability = (double?)xTile.Attribute("probability") ?? 1.0;
            if (xTile.Attribute("class") != null)
                Type = (string)xTile.Attribute("class") ?? string.Empty;
            else
                Type = (string)xTile.Attribute("type") ?? string.Empty;
            Image = new TmxImage(xTile.Element("image"), tmxDir);

            Properties = new PropertyDict(xTile.Element("properties"));
        }
    }
}
