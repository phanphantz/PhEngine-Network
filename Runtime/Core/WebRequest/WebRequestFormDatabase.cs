using System.Linq;
using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/WebRequestFormDatabase", fileName = "WebRequestFormDatabase", order = 0)]
    public class WebRequestFormDatabase : ScriptableObject
    {
        [SerializeField] WebRequestForm[] forms;

        public WebRequestForm GetByName(string formName)
            => forms.FirstOrDefault(t => t.name == formName);
    }
}