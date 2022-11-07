using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x02000067 RID: 103
	public class TmxDocument
	{
		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x060003AC RID: 940 RVA: 0x00018E7F File Offset: 0x0001707F
		// (set) Token: 0x060003AD RID: 941 RVA: 0x00018E87 File Offset: 0x00017087
		public string TmxDirectory { get; private set; }

		// Token: 0x060003AE RID: 942 RVA: 0x00018EA8 File Offset: 0x000170A8
		protected XDocument ReadXml(string filepath)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			string fileResPath = filepath.Replace(Path.DirectorySeparatorChar.ToString(), ".");
			string text = Array.Find<string>(manifestResourceNames, (string s) => s.EndsWith(fileResPath));
			XDocument result;
			if (text != null)
			{
				Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(text);
				result = XDocument.Load(manifestResourceStream);
				this.TmxDirectory = "";
			}
			else
			{
				result = XDocument.Load(filepath);
				this.TmxDirectory = Path.GetDirectoryName(filepath);
			}
			return result;
		}
	}
}
