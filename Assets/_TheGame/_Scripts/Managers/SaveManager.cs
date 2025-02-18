using _TheGame._Scripts.Constants;
using UnityEngine;

namespace _TheGame._Scripts.Managers
{
    public class SaveManager : Singleton<SaveManager>
    {
        public void SetLastLevelIndex(int index)
        {
            PlayerPrefs.SetInt(StringConstants.LastSceneIndex, index);
            PlayerPrefs.Save();
        }

        public int GetLastLevelIndex()
        {
            return PlayerPrefs.GetInt(StringConstants.LastSceneIndex, -1);
        }
    }
}