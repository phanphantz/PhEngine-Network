using PhEngine.JSON;

namespace PhEngine.Network
{
    public abstract class NetworkConfigCenter<T> : NetworkObjectCenter<T> where T : NetworkConfig
    {
        public override JSONObject CreateGetRequestBody() => null;
    }
}