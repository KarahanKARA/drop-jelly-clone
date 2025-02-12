using UnityEngine;

namespace _TheGame._Scripts.Helpers
{
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea = new Rect(0, 0, 0, 0);
        private Vector2 _bannerAdSize;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _bannerAdSize = GetBannerAdSize();
        }

        private void Update()
        {
            var safeArea = Screen.safeArea;

            safeArea.height -= _bannerAdSize.y; 

            if (safeArea != _lastSafeArea)
            {
                ApplySafeArea(safeArea);
            }
        }

        private Vector2 GetBannerAdSize()
        {
            return Vector2.zero;
            //TODO> GET BANNER AD SIZE
        }

        private void ApplySafeArea(Rect safeArea)
        {
            _lastSafeArea = safeArea;

            var anchorMinX = safeArea.xMin / Screen.width;
            var anchorMinY = safeArea.yMin / Screen.height;
            var anchorMaxX = safeArea.xMax / Screen.width;
            var anchorMaxY = (safeArea.yMax - _bannerAdSize.y) / Screen.height;

            _rectTransform.anchorMin = new Vector2(anchorMinX, anchorMinY);
            _rectTransform.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        }
        
    }
}