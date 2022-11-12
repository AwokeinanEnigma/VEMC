using System.Xml.Linq;

namespace TiledSharp
{

    public class TmxTileOffset
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public TmxTileOffset(XElement xTileOffset)
        {
            if (xTileOffset == null)
            {
                X = 0;
                Y = 0;
            }
            else
            {
                X = (int)xTileOffset.Attribute("x");
                Y = (int)xTileOffset.Attribute("y");
            }
        }
    }

}
