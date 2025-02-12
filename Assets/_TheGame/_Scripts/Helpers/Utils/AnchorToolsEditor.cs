using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace _TheGame._Scripts.Helpers.Utils
{
    public class AnchorToolsEditor : EditorWindow
    {
        [MenuItem("Tools/Anchor Tools")]
        private static void Init()
        {
            GetWindow<AnchorToolsEditor>(title: "Anchor Tools");
        }

        private void OnGUI()
        {
            var viewWidth = EditorGUIUtility.currentViewWidth;
            var buttonWidth = viewWidth * 0.9f;

            if (GUILayout.Button("Stick Anchors to Rect", GUILayout.Width(buttonWidth), GUILayout.Height(50)))
            {
                UpdateAnchors();
            }
        }

        private static Rect _anchorRect;
        private static Vector2 _anchorVector;
        private static RectTransform _currentRectTransform;
        private static RectTransform _parentRectTransform;

        private static void UpdateAnchors()
        {
            foreach (var go in Selection.gameObjects)
            {
                _currentRectTransform = go.GetComponent<RectTransform>();
                if (_currentRectTransform != null && _currentRectTransform.parent != null)
                {
                    _parentRectTransform = _currentRectTransform.parent.GetComponent<RectTransform>();
                    Stick();
                }
                else
                {
                    _parentRectTransform = null;
                }
            }
        }

        private static void Stick()
        {
            CalculateCurrentWh();
            CalculateCurrentXY();

            CalculateCurrentXY();
            AnchorsToCorners();
        }

        private static void CalculateCurrentXY()
        {
            var pivotX = _anchorRect.width * _currentRectTransform.pivot.x;
            var pivotY = _anchorRect.height * (1 - _currentRectTransform.pivot.y);
            var newXY = new Vector2(_currentRectTransform.anchorMin.x * _parentRectTransform.rect.width + _currentRectTransform.offsetMin.x + pivotX - _parentRectTransform.rect.width * _anchorVector.x,
                -(1 - _currentRectTransform.anchorMax.y) * _parentRectTransform.rect.height + _currentRectTransform.offsetMax.y - pivotY + _parentRectTransform.rect.height * (1 - _anchorVector.y));
            _anchorRect.x = newXY.x;
            _anchorRect.y = newXY.y;
        }

        private static void CalculateCurrentWh()
        {
            var rect = _currentRectTransform.rect;
            _anchorRect.width = rect.width;
            _anchorRect.height = rect.height;
        }

        private static void AnchorsToCorners()
        {
            Undo.RecordObject(_currentRectTransform, "Stick Anchors");

            var pivotX = _anchorRect.width * _currentRectTransform.pivot.x;
            var pivotY = _anchorRect.height * (1 - _currentRectTransform.pivot.y);
            _currentRectTransform.anchorMin = new Vector2(0f, 1f);
            _currentRectTransform.anchorMax = new Vector2(0f, 1f);
            _currentRectTransform.offsetMin = new Vector2(_anchorRect.x / _currentRectTransform.localScale.x, _anchorRect.y / _currentRectTransform.localScale.y - _anchorRect.height);
            _currentRectTransform.offsetMax = new Vector2(_anchorRect.x / _currentRectTransform.localScale.x + _anchorRect.width, _anchorRect.y / _currentRectTransform.localScale.y);
            _currentRectTransform.anchorMin = new Vector2(_currentRectTransform.anchorMin.x + _anchorVector.x + (_currentRectTransform.offsetMin.x - pivotX) / _parentRectTransform.rect.width * _currentRectTransform.localScale.x,
                _currentRectTransform.anchorMin.y - (1 - _anchorVector.y) + (_currentRectTransform.offsetMin.y + pivotY) / _parentRectTransform.rect.height * _currentRectTransform.localScale.y);
            _currentRectTransform.anchorMax = new Vector2(_currentRectTransform.anchorMax.x + _anchorVector.x + (_currentRectTransform.offsetMax.x - pivotX) / _parentRectTransform.rect.width * _currentRectTransform.localScale.x,
                _currentRectTransform.anchorMax.y - (1 - _anchorVector.y) + (_currentRectTransform.offsetMax.y + pivotY) / _parentRectTransform.rect.height * _currentRectTransform.localScale.y);
            _currentRectTransform.offsetMin = new Vector2((0 - _currentRectTransform.pivot.x) * _anchorRect.width * (1 - _currentRectTransform.localScale.x), (0 - _currentRectTransform.pivot.y) * _anchorRect.height * (1 - _currentRectTransform.localScale.y));
            _currentRectTransform.offsetMax = new Vector2((1 - _currentRectTransform.pivot.x) * _anchorRect.width * (1 - _currentRectTransform.localScale.x), (1 - _currentRectTransform.pivot.y) * _anchorRect.height * (1 - _currentRectTransform.localScale.y));
        }
    }
}
#endif