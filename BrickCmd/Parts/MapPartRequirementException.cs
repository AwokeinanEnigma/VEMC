using System;

namespace VEMC.Parts
{
    internal class MapPartRequirementException : Exception
    {
        public override string Message => message;
        public MapPartRequirementException(string name, string key)
        {
            message = string.Format("Error processing {0}: \"{1}\" is required", name, key);
        }
        private readonly string message;
    }
}
