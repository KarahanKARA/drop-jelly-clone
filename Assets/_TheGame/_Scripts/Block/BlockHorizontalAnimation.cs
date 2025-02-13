using DG.Tweening;
using UnityEngine;

namespace _TheGame._Scripts.Block
{
    public class BlockHorizontalAnimation : MonoBehaviour
    {
        [SerializeField] private float maxRotationAngle = 30f;
        [SerializeField] private float rotationDuration = 3.3f;
        [SerializeField] private float resetDelay = 0.1f; // Hareket durunca ne kadar süre sonra reset başlasın
        public AnimationCurve animationCurve;
    
        private float _lastXPosition;
        private Tween _currentRotationTween;
        private float _lastMovementTime;
    
        private void Awake()
        {
            _lastXPosition = transform.position.x;
            _lastMovementTime = Time.time;
        }

        private void Update()
        {
            // Hareket durunca reset
            if (Time.time - _lastMovementTime > resetDelay && transform.rotation != Quaternion.identity)
            {
                ResetRotation();
            }
        }

        public void UpdateHorizontalAnimation(float xPosition)
        {
            float direction = xPosition - _lastXPosition;
            _lastXPosition = xPosition;

            if (Mathf.Abs(direction) > 0.01f)
            {
                _lastMovementTime = Time.time; // Hareket zamanını güncelle
            
                float targetRotation = direction < 0 ? maxRotationAngle : -maxRotationAngle;
            
                float currentYRotation = transform.rotation.eulerAngles.y;
                if (currentYRotation > 180) currentYRotation -= 360;
            
                if (targetRotation > 0 && currentYRotation >= maxRotationAngle) return;
                if (targetRotation < 0 && currentYRotation <= -maxRotationAngle) return;

                if (_currentRotationTween != null && _currentRotationTween.IsActive())
                {
                    _currentRotationTween.Kill();
                }

                _currentRotationTween = transform.DORotate(
                    new Vector3(0, targetRotation, 0), 
                    rotationDuration
                ).SetEase(animationCurve);
            }
        }

        public void ResetRotation()
        {
            if (_currentRotationTween != null && _currentRotationTween.IsActive())
                _currentRotationTween.Kill();

            _currentRotationTween = transform.DORotate(
                Vector3.zero, 
                rotationDuration * 0.08f
            ).SetEase(Ease.OutBounce);
        }

        private void OnDisable()
        {
            if (_currentRotationTween != null)
                _currentRotationTween.Kill();
        }
    }
}