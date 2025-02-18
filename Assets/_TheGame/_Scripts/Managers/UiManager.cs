using System.Collections;
using _TheGame._Scripts.References;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _TheGame._Scripts.Managers
{
    public class UiManager : Singleton<UiManager>
    {
        public void ReplayButtonOnClick()
        {
            SceneManager.LoadScene(1);
        }

        public void BackButtonOnClick()
        {
            SceneManager.LoadScene(0);
        }

        public void SetMovesText(int moveCount)
        {
            ComponentReferences.Instance.movesText.text = "Moves : " + moveCount;
        }

        public void SetLevelText(string levelCount)
        {
            ComponentReferences.Instance.levelText.text = "Level : " + levelCount;
        }

        public void GameFail()
        {
            StartCoroutine(DelayFailFunc());
        }
        
        public void GameWin()
        {
            ObjectReferences.Instance.congratsPanel.SetActive(true);
            StartCoroutine(DelayFunc());
        }

        private IEnumerator DelayFailFunc()
        {
            yield return new WaitForSeconds(0.6f);
            ObjectReferences.Instance.failPanel.SetActive(true);
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(1);
        }
        
        private IEnumerator DelayFunc()
        {
            yield return new WaitForSeconds(2f);
            var currentLevelIndex = SaveManager.Instance.GetLastLevelIndex();
            if (currentLevelIndex == 3)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                SaveManager.Instance.SetLastLevelIndex(currentLevelIndex + 1);
                SceneManager.LoadScene(1);
            }
        }
    }
}
