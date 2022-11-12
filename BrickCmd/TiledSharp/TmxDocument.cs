using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace TiledSharp
{
    public class TmxDocument
    {
        public string TmxDirectory { get; private set; }
        protected XDocument ReadXml(string filepath)
        {
            XDocument xDoc;

            var asm = Assembly.GetEntryAssembly();
            var manifest = new string[0];

            if (asm != null)
                manifest = asm.GetManifestResourceNames();

            var fileResPath = filepath.Replace(
                    Path.DirectorySeparatorChar.ToString(), ".");
            var fileRes = Array.Find(manifest, s => s.EndsWith(fileResPath));

            // If there is a resource in the assembly, load the resource
            // Otherwise, assume filepath is an explicit path
            if (fileRes != null)
            {
                using (Stream xmlStream = asm.GetManifestResourceStream(fileRes))
                {
                    using (XmlReader reader = XmlReader.Create(xmlStream))
                    {
                        xDoc = XDocument.Load(reader);
                    }
                }
                TmxDirectory = String.Empty;
            }
            else
            {
                // TODO: Check for existence of file

                xDoc = XDocument.Load(filepath);
                TmxDirectory = Path.GetDirectoryName(filepath);
            }

            return xDoc;
        }
    }
}
