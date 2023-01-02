using PhEngine.JSON;

namespace PhEngine.Network
{
    public abstract class NetworkConfig : JSONConvertibleObject
    {
        public abstract string ConfigId { get; }
    }
}