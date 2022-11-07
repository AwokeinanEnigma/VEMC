using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace TiledSharp
{
    public class TmxDocument
    {
        public string TmxDirectory { get; private set; }
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
                TmxDirectory = "";
            }
            else
            {
                result = XDocument.Load(filepath);
                TmxDirectory = Path.GetDirectoryName(filepath);
            }
            return result;
        }
    }
}
