using System;

namespace VEMC.Parts
{
	// Token: 0x02000034 RID: 52
	internal class MapPartParameterException : Exception
	{
		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000217 RID: 535 RVA: 0x0000905A File Offset: 0x0000725A
		public override string Message
		{
			get
			{
				return this.message;
			}
		}

		// Token: 0x06000218 RID: 536 RVA: 0x00009062 File Offset: 0x00007262
		public MapPartParameterException(string name, string parameter, Type type)
		{
			this.message = string.Format("Error processing {0}: \"{1}\" ({2}) is invalid", name, parameter, type.Name);
		}

		// Token: 0x040000A3 RID: 163
		private string message;
	}
}
