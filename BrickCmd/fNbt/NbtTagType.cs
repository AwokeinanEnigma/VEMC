using System;

namespace fNbt
{
	public enum NbtTagType : byte
	{
		Unknown = 255,
		End = 0,
		Byte,
		Short,
		Int,
		Long,
		Float,
		Double,
		ByteArray,
		String,
		List,
		Compound,
		IntArray
	}
}
