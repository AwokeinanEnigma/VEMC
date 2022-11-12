using System.Globalization;
using System.Xml.Linq;

namespace TiledSharp
{
    public class TmxColor
    {
        public int R { get; private set; }
        public int G { get; private set; }
        public int B { get; private set; }

        public TmxColor(XAttribute xColor)
        {
            if (xColor == null) return;

            var colorStr = ((string)xColor).TrimStart("#".ToCharArray());

            R = int.Parse(colorStr.Substring(0, 2), NumberStyles.HexNumber);
            G = int.Parse(colorStr.Substring(2, 2), NumberStyles.HexNumber);
            B = int.Parse(colorStr.Substring(4, 2), NumberStyles.HexNumber);
        }
    }
}
