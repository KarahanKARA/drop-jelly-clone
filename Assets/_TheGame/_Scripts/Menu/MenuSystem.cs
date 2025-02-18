using _TheGame._Scripts.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _TheGame._Scripts.Menu
{
    public class MenuSystem : MonoBehaviour
    {
        public void LevelButtonOnClick(int levelIndex)
        {
            SaveManager.Instance.SetLastLevelIndex(levelIndex);
            SceneManager.LoadScene(sceneBuildIndex: 1);
        }
    }
}
