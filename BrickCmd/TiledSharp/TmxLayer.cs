using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace TiledSharp
{
    public class TmxLayer : ITmxElement
    {
        public string Name { get; private set; }
        public double Opacity { get; private set; }
        public bool Visible { get; private set; }
        public List<TmxLayerTile> Tiles { get; private set; }
        public PropertyDict Properties { get; private set; }
        public TmxLayer(XElement xLayer, int width, int height)
        {
            Name = (string)xLayer.Attribute("name");
            double? num = (double?)xLayer.Attribute("opacity");
            Opacity = ((num != null) ? num.GetValueOrDefault() : 1.0);
            Visible = (((bool?)xLayer.Attribute("visible")) ?? true);
            XElement xelement = xLayer.Element("data");
            string text = (string)xelement.Attribute("encoding");
            Tiles = new List<TmxLayerTile>();
            if (text == "base64")
            {
                TmxBase64Data tmxBase64Data = new TmxBase64Data(xelement);
                Stream data = tmxBase64Data.Data;
                using (BinaryReader binaryReader = new BinaryReader(data))
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            Tiles.Add(new TmxLayerTile(binaryReader.ReadUInt32(), j, i));
                        }
                    }
                    goto IL_23C;
                }
            }
            if (!(text == "csv"))
            {
                if (text == null)
                {
                    int num2 = 0;
                    using (IEnumerator<XElement> enumerator = xelement.Elements("tile").GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            XElement xelement2 = enumerator.Current;
                            uint id = (uint)xelement2.Attribute("gid");
                            int x = num2 % width;
                            int y = num2 / width;
                            Tiles.Add(new TmxLayerTile(id, x, y));
                            num2++;
                        }
                        goto IL_23C;
                    }
                }
                throw new Exception("TmxLayer: Unknown encoding.");
            }
            string value = xelement.Value;
            int num3 = 0;
            foreach (string text2 in value.Split(new char[]
            {
                ','
            }))
            {
                uint id2 = uint.Parse(text2.Trim());
                int x2 = num3 % width;
                int y2 = num3 / width;
                Tiles.Add(new TmxLayerTile(id2, x2, y2));
                num3++;
            }
        IL_23C:
            Properties = new PropertyDict(xLayer.Element("properties"));
        }
    }
}
