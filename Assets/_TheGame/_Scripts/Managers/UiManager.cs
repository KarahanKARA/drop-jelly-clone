using UnityEngine;
using UnityEngine.SceneManagement;

namespace _TheGame._Scripts.Managers
{
    public class UiManager : MonoBehaviour
    {
        public void ReplayButtonOnClick()
        {
            SceneManager.LoadScene(1);
        }

        public void BackButtonOnClick()
        {
            SceneManager.LoadScene(0);
        }
    }
}
