using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _TheGame._Scripts.Helpers
{
    public static class Extensions
    {
        // Enum --------------------------------
        public static T GetRandomEnumValue<T>(this T enumType) where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
        
        
        // GameObject ----------------------------------
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }
        
        public static void DestroyAfterDelay(this GameObject gameObject, float delay)
        {
            Object.Destroy(gameObject, delay);
        }
        
        public static void SetActiveDelayed(this GameObject gameObject, bool active, float delay)
        {
            MonoBehaviour coroutineRunner = gameObject.GetComponent<MonoBehaviour>();
            if (coroutineRunner == null)
            {
                Debug.LogError("GameObject must have a MonoBehaviour to use SetActiveAfterDelay!");
                return;
            }

            coroutineRunner.StartCoroutine(SetActiveAfterDelayCoroutine(gameObject, active, delay));
        }

        // Transform ----------------------------------

        public static void ResetTransform(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public static void LookAt2D(this Transform transform, Vector3 target)
        {
            Vector3 dir = target - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        public static void MatchPosition(this Transform transform, Transform target)
        {
            transform.position = target.position;
        }

        public static void MatchRotation(this Transform transform, Transform target)
        {
            transform.rotation = target.rotation;
        }

        public static void MatchScale(this Transform transform, Transform target)
        {
            transform.localScale = target.localScale;
        }

        public static void ResetLocalPosition(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
        }

        // List ----------------------------------

        public static void EnableAllButtons(this List<Button> buttons)
        {
            foreach (var button in buttons)
            {
                button.interactable = true;
            }
        }

        public static void DisableAllButtons(this List<Button> buttons)
        {
            foreach (var button in buttons)
            {
                button.interactable = false;
            }
        }
        
        public static void DestroyElements(this List<GameObject> list)
        {
            if (list.Count == 0)
            {
                Debug.LogWarning("List is empty. Returning default value.");
            }
            foreach (var t in list)
            {
                Object.Destroy(t);
            }
            list.Clear();
        }
        
        public static T RandomElement<T>(this List<T> list)
        {
            if (list.Count == 0)
            {
                Debug.LogWarning("List is empty. Returning default value.");
                return default;
            }

            int randomIndex = Random.Range(0, list.Count);
            return list[randomIndex];
        }
        
        public static void ReverseInPlace<T>(this List<T> list)
        {
            var count = list.Count;
            for (var i = 0; i < count / 2; i++)
            {
                (list[i], list[count - 1 - i]) = (list[count - 1 - i], list[i]);
            }
        }

        public static void Shuffle<T>(this List<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
        
        public static void SetActiveAllElements<T>(this List<T> list, bool value)
        {
            foreach (var element in list)
            {
                switch (element)
                {
                    case GameObject gameObject:
                        gameObject.SetActive(value);
                        break;
                    case Component component:
                        component.gameObject.SetActive(value);
                        break;
                }
            }
        }
        
        // Camera ----------------------------------

        public static Bounds OrthographicBounds(this Camera camera)
        {
            float screenAspect = (float)Screen.width / (float)Screen.height;
            float cameraHeight = camera.orthographicSize * 2;
            Bounds bounds = new Bounds(camera.transform.position,
                new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
            return bounds;
        }
        public static bool IsObjectVisibleOnScreen(this Camera camera, GameObject obj)
        {
            Vector3 screenPoint = camera.WorldToViewportPoint(obj.transform.position);

            return screenPoint.x is >= 0f and <= 1f && screenPoint.y is >= 0f and <= 1f && screenPoint.z > 0f;
        }

        // Graphics
        public static void ChangeOpacity<T>(this T obj, float opacity) where T : Graphic
        {
            Color color = obj.color;
            color.a = opacity;
            obj.color = color;
        }

        public static string FormatNumber(this long number)
        {
            if (number < 1000)
            {
                return number.ToString();
            }
            var formattedNumber = Math.Floor((double)number / 1000 * 100) / 100;
            return formattedNumber.ToString("0.00") + "K";
        }
        
        
        // Local ----------------------------------
        private static System.Collections.IEnumerator SetActiveAfterDelayCoroutine(GameObject gameObject, bool active, float delay)
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(active);
        }
    }
}