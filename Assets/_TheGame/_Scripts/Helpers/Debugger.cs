using System.Text.RegularExpressions;
using UnityEngine;

namespace _TheGame._Scripts.Helpers
{
    public abstract class Debugger
    {
        private static readonly Color textColor = Color.green;
        private static readonly Color variableColor = Color.red;
        

        public static void Log(string message)
        {
            string coloredMessage = ColorNumbers(message);
            Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{coloredMessage}</color>");
        }

        private static string ColorNumbers(string input)
        {
            return Regex.Replace(input, @"(-?\d+(?:\.\d+)?)", match => 
                $"<color=#{ColorUtility.ToHtmlStringRGB(variableColor)}>{match.Value}</color>");
        }
    }
}
