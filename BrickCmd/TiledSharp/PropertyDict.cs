using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace TiledSharp
{
	// Token: 0x0200006F RID: 111
	public class PropertyDict : Dictionary<string, string>
	{
		// Token: 0x060003F8 RID: 1016 RVA: 0x00019D6C File Offset: 0x00017F6C
		public PropertyDict(XElement xmlProp)
		{
			if (xmlProp == null)
			{
				return;
			}
			foreach (XElement xelement in xmlProp.Elements("property"))
			{
				string value = xelement.Attribute("name").Value;
				string value2 = xelement.Attribute("value").Value;
				base.Add(value, value2);
			}
		}
	}
}
