using System.IO;
using System.Xml.Linq;

namespace TiledSharp
{
    public class TmxImage
    {
        public string Format { get; private set; }
        public string Source { get; private set; }
        public Stream Data { get; private set; }
        public TmxColor Trans { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public TmxImage(XElement xImage, string tmxDir = "")
        {
            XAttribute xattribute = xImage.Attribute("source");
            if (xattribute != null)
            {
                Source = Path.Combine(tmxDir, (string)xattribute);
            }
            else
            {
                Format = (string)xImage.Attribute("format");
                XElement xData = xImage.Element("data");
                TmxBase64Data tmxBase64Data = new TmxBase64Data(xData);
                Data = tmxBase64Data.Data;
            }
            Trans = new TmxColor(xImage.Attribute("trans"));
            Width = (int)xImage.Attribute("width");
            Height = (int)xImage.Attribute("height");
        }
    }
}
