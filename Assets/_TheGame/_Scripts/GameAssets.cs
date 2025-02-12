using _TheGame._Scripts.Helpers;
using UnityEngine;

namespace _TheGame._Scripts
{
    public class GameAssets : MonoBehaviour
    {
        private static GameAssets instance;
        public static GameAssets Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Instantiate(Resources.Load<GameAssets>("GameAssets"));
                }
                return instance;
            }
        }

        public SoundAudioClip[] soundAudioClipArray;

        [System.Serializable]
        public class SoundAudioClip
        {
            public Enums.Sound sound;
            public AudioClip audioClip;
        }
    }
}
