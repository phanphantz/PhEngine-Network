using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/NetworkRuleConfig", fileName = "NetworkRuleConfig", order = 0)]
    public class NetworkRuleConfig : ScriptableObject
    {
        public ClientRequestRule clientRequestRule;
        public ServerResultRule serverResultRule;
    }
}