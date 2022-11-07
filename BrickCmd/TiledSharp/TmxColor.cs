using System.Globalization;
using System.Xml.Linq;

namespace TiledSharp
{
    public class TmxColor
    {
        public TmxColor(XAttribute xColor)
        {
            if (xColor == null)
            {
                return;
            }
            string text = ((string)xColor).TrimStart("#".ToCharArray());
            R = int.Parse(text.Substring(0, 2), NumberStyles.HexNumber);
            G = int.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
            B = int.Parse(text.Substring(4, 2), NumberStyles.HexNumber);
        }
        public int R;
        public int G;
        public int B;
    }
}
