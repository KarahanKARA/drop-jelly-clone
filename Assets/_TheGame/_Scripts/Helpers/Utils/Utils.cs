using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _TheGame._Scripts.Helpers.Utils
{
    public static class Utils
    {
        public static void SetInteractableOfButtons<T>(T obj,bool statue)
        {
            switch (obj)
            {
                case List<Button> buttonList:
                {
                    foreach (Button button in buttonList)
                    {
                        button.interactable = statue;
                    }

                    break;
                }
                case Button button:
                    button.interactable = statue;
                    break;
            }
        }

        public static IEnumerator SetActiveObjectWithDelay(GameObject obj,float delay)
        {
            yield return new WaitForSeconds(delay);
            obj.SetActive(true);
        }
        
        public static string ConvertToMinutesAndSeconds(double totalSeconds)
        {
            var minutes = (int)(totalSeconds / 60);
            var seconds = (int)(totalSeconds % 60);

            var timeString = $"{minutes:D2}:{seconds:D2}";

            return timeString;
        }
        
        public static string ConvertToHoursMinutesAndSeconds(double totalSeconds)
        {
            var hours = (int)(totalSeconds / 3600);
            var minutes = (int)((totalSeconds % 3600) / 60);
            var seconds = (int)(totalSeconds % 60);

            var timeString = $"{hours:D2}:{minutes:D2}:{seconds:D2}";

            return timeString;
        }
    }
}