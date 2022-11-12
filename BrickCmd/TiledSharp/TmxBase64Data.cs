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
            string encoding = (string)(xData.Attribute("encoding") ?? xData.Parent?.Attribute("encoding"));
            string compression = (string)(xData.Attribute("compression") ?? xData.Parent?.Attribute("compression"));
            if (encoding != "base64")
                throw new Exception(
                    "TmxBase64Data: Only Base64-encoded data is supported.");

            var rawData = Convert.FromBase64String((string)xData.Value);
            Data = new MemoryStream(rawData, false);

            if (compression == "gzip")
            {
                Data = new GZipStream(Data, CompressionMode.Decompress);
            }
            else if (compression == "zlib")
            {
                // Strip 2-byte header and 4-byte checksum
                // TODO: Validate header here
                var bodyLength = rawData.Length - 6;
                byte[] bodyData = new byte[bodyLength];
                Array.Copy(rawData, 2, bodyData, 0, bodyLength);

                var bodyStream = new MemoryStream(bodyData, false);
                Data = new DeflateStream(bodyStream, CompressionMode.Decompress);

                // TODO: Validate checksum?
            }
            else if (compression != null)
                throw new Exception("TmxBase64Data: Unknown compression.");
        }
    }
}
