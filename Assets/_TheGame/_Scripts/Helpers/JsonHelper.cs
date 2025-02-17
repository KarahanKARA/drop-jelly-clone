using UnityEngine;
using _TheGame._Scripts.Data;

namespace _TheGame._Scripts.Helpers
{
    public static class JsonHelper
    {
        public static BlockDataModel.GameMoveData LoadMoveData()
        {
            var jsonText = Resources.Load<TextAsset>("moves").text;
            return JsonUtility.FromJson<BlockDataModel.GameMoveData>(jsonText);
        }
    }
}