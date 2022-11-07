using System;

namespace VEMC.Parts
{
    internal class MapPartConversionException : Exception
    {
        public override string Message => message;
        public MapPartConversionException(string name, string key, Type expectedType)
        {
            message = string.Format("Error processing {0}: Cannot convert \"{1}\" to a {2}", name, key, expectedType.Name);
        }
        private readonly string message;
    }
}
