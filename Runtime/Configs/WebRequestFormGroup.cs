using System.Linq;
using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/WebRequestFormGroup", fileName = "WebRequestFormGroup", order = 0)]
    public class WebRequestFormGroup : ScriptableObject
    {
        [SerializeField] WebRequestFormConfig[] forms;

        public WebRequestFormConfig GetByName(string formName)
        {
            return forms.FirstOrDefault(t => t.name == formName);
        }
    }
}