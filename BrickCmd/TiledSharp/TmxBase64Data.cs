using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using Ionic.Zlib;

namespace TiledSharp
{
	// Token: 0x02000072 RID: 114
	public class TmxBase64Data
	{
		// Token: 0x170000EA RID: 234
		// (get) Token: 0x06000407 RID: 1031 RVA: 0x00019FA5 File Offset: 0x000181A5
		// (set) Token: 0x06000408 RID: 1032 RVA: 0x00019FAD File Offset: 0x000181AD
		public Stream Data { get; private set; }

		// Token: 0x06000409 RID: 1033 RVA: 0x00019FB8 File Offset: 0x000181B8
		public TmxBase64Data(XElement xData)
		{
			if ((string)xData.Attribute("encoding") != "base64")
			{
				throw new Exception("TmxBase64Data: Only Base64-encoded data is supported.");
			}
			byte[] buffer = Convert.FromBase64String(xData.Value);
			this.Data = new MemoryStream(buffer, false);
			string text = (string)xData.Attribute("compression");
			if (text == "gzip")
			{
				this.Data = new System.IO.Compression.GZipStream(this.Data, System.IO.Compression.CompressionMode.Decompress, false);
				return;
			}
			if (text == "zlib")
			{
				this.Data = new ZlibStream(this.Data, Ionic.Zlib.CompressionMode.Decompress, false);
				return;
			}
			if (text != null)
			{
				throw new Exception("TmxBase64Data: Unknown compression.");
			}
		}
	}
}
