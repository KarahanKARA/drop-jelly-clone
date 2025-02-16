using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Systems.Visuals
{
    public class RgbColorSystem : MonoBehaviour
    {
        [SerializeField] private Material targetMaterial;
        
        private readonly float[] _hueValues = new float[] 
        {
            0f,   
            60f,  
            120f,  
            180f, 
            240f, 
            300f  
        };

        private const float Saturation = 1f;
        private const float Value = 1f;

        private void Awake()
        {
            if (targetMaterial == null)
            {
                Debug.LogError("Target material is not assigned!");
                enabled = false;
                return;
            }
            
            // İlk rengi set et (son renkten başlayarak)
            Color startColor = GetColorFromHue(_hueValues[_hueValues.Length - 1]);
            targetMaterial.color = startColor;
            
            CycleThroughColors(2f);
        }

        private void OnDestroy()
        {
            DOTween.Kill(targetMaterial);
        }

        private Color GetColorFromHue(float hue)
        {
            return Color.HSVToRGB(hue / 360f, Saturation, Value);
        }

        private void CycleThroughColors(float durationPerColor = 1f)
        {
            var colorSequence = DOTween.Sequence();
            
            var firstColor = GetColorFromHue(_hueValues[0]);
            colorSequence.Append(targetMaterial.DOColor(firstColor, durationPerColor));
            
            for (int i = 1; i < _hueValues.Length; i++)
            {
                var targetColor = GetColorFromHue(_hueValues[i]);
                colorSequence.Append(targetMaterial.DOColor(targetColor, durationPerColor));
            }
            
            colorSequence.SetLoops(-1);
        }
    }
}