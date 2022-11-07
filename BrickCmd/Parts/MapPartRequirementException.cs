using System;

namespace VEMC.Parts
{
	// Token: 0x02000032 RID: 50
	internal class MapPartRequirementException : Exception
	{
		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000213 RID: 531 RVA: 0x00009010 File Offset: 0x00007210
		public override string Message
		{
			get
			{
				return this.message;
			}
		}

		// Token: 0x06000214 RID: 532 RVA: 0x00009018 File Offset: 0x00007218
		public MapPartRequirementException(string name, string key)
		{
			this.message = string.Format("Error processing {0}: \"{1}\" is required", name, key);
		}

		// Token: 0x040000A1 RID: 161
		private string message;
	}
}
