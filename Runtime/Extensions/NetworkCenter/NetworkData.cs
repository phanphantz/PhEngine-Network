using PhEngine.JSON;

namespace PhEngine.Network
{
    public abstract class NetworkData : JSONConvertibleObject
    {
        public abstract string ConfigId { get; }
        public abstract string UserId { get; }
    }
}