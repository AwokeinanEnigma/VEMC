using System;

namespace VEMC.Parts
{
	// Token: 0x02000033 RID: 51
	internal class MapPartConversionException : Exception
	{
		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000215 RID: 533 RVA: 0x00009032 File Offset: 0x00007232
		public override string Message
		{
			get
			{
				return this.message;
			}
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0000903A File Offset: 0x0000723A
		public MapPartConversionException(string name, string key, Type expectedType)
		{
			this.message = string.Format("Error processing {0}: Cannot convert \"{1}\" to a {2}", name, key, expectedType.Name);
		}

		// Token: 0x040000A2 RID: 162
		private string message;
	}
}
