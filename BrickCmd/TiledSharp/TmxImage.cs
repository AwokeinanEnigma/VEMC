using System.IO;
using System.Xml.Linq;

namespace TiledSharp
{

    public class TmxImage
    {
        public string Source { get; private set; }
        public string Format { get; private set; }
        public Stream Data { get; private set; }
        public TmxColor Trans { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TmxImage(XElement xImage, string tmxDir = "")
        {
            if (xImage == null) return;

            var xSource = xImage.Attribute("source");

            if (xSource != null)
                // Append directory if present
                Source = Path.Combine(tmxDir, (string)xSource);
            else
            {
                Format = (string)xImage.Attribute("format");
                var xData = xImage.Element("data");
                var decodedStream = new TmxBase64Data(xData);
                Data = decodedStream.Data;
            }

            Trans = new TmxColor(xImage.Attribute("trans"));
            Width = (int)xImage.Attribute("width");
            Height = (int)xImage.Attribute("height");
        }
    }
}
