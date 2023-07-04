using System.Linq;
using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/WebRequestFormDatabase", fileName = "WebRequestFormDatabase", order = 0)]
    public class WebRequestFormDatabase : ScriptableObject
    {
        [SerializeField] WebRequestFormConfig[] forms;

        public WebRequestForm GetByName(string formName)
        {
            var config = forms.FirstOrDefault(t => t.name == formName);
            if (config == null)
                Debug.LogError("Cannot find the Web Request Form with name: " + formName);

            return config.Form;
        }
    }
}