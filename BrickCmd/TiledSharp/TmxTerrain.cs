﻿using System.Xml.Linq;

namespace TiledSharp
{
    public class TmxTerrain : ITmxElement
    {
        public string Name { get; private set; }
        public int Tile { get; private set; }
        public PropertyDict Properties { get; private set; }
        public TmxTerrain(XElement xTerrain)
        {
            Name = (string)xTerrain.Attribute("name");
            Tile = (int)xTerrain.Attribute("tile");
            Properties = new PropertyDict(xTerrain.Element("properties"));
        }
    }
}
