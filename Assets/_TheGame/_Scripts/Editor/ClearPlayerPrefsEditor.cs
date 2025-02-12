using System.IO;
using _TheGame._Scripts.Constants;
using UnityEditor;
using UnityEngine;

namespace _TheGame._Scripts.Editor
{
    public class ClearPlayerPrefsEditor : EditorWindow
    {
        [MenuItem("Tools/Clear PlayerPrefs")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ClearPlayerPrefsEditor), false, "Clear PlayerPrefs");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Clear All PlayerPrefs"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("All PlayerPrefs cleared!");
            }

            if (GUILayout.Button("Delete Save File"))
            {
                DeleteSaveFile();
            }
        }

        private void DeleteSaveFile()
        {
            string savePath = Path.Combine(Application.persistentDataPath, StringConstants.SaveFileName);
            try
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    Debug.Log($"Save file deleted successfully from: {savePath}");
                }
                else
                {
                    Debug.Log($"No save file found at: {savePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error deleting save file: {e.Message}");
            }
        }
    }
}
