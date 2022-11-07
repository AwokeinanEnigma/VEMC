using System;

namespace JetBrains.Annotations
{
	// Token: 0x02000004 RID: 4
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Delegate)]
	public sealed class CanBeNullAttribute : Attribute
	{
	}
}
