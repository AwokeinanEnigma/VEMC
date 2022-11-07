using Ionic.Zlib;
using System;
using System.IO;
using System.Xml.Linq;

namespace TiledSharp
{
    public class TmxBase64Data
    {
        public Stream Data { get; private set; }
        public TmxBase64Data(XElement xData)
        {
            if ((string)xData.Attribute("encoding") != "base64")
            {
                throw new Exception("TmxBase64Data: Only Base64-encoded data is supported.");
            }
            byte[] buffer = Convert.FromBase64String(xData.Value);
            Data = new MemoryStream(buffer, false);
            string text = (string)xData.Attribute("compression");
            if (text == "gzip")
            {
                Data = new System.IO.Compression.GZipStream(Data, System.IO.Compression.CompressionMode.Decompress, false);
                return;
            }
            if (text == "zlib")
            {
                Data = new ZlibStream(Data, Ionic.Zlib.CompressionMode.Decompress, false);
                return;
            }
            if (text != null)
            {
                throw new Exception("TmxBase64Data: Unknown compression.");
            }
        }
    }
}
