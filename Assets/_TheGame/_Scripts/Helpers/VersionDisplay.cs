using TMPro;
using UnityEngine;

namespace _TheGame._Scripts.Helpers
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VersionDisplay : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<TextMeshProUGUI>().text = "v"+Application.version;
        }
    }
}
