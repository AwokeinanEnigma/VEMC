using System;

namespace VEMC.Parts
{
    internal class MapPartParameterException : Exception
    {
        public override string Message => message;
        public MapPartParameterException(string name, string parameter, Type type)
        {
            message = string.Format("Error processing {0}: \"{1}\" ({2}) is invalid", name, parameter, type.Name);
        }
        private readonly string message;
    }
}
