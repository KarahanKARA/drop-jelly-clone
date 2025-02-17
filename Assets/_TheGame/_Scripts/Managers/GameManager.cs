using UnityEngine;
using UnityEngine.SceneManagement;

namespace _TheGame._Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}
