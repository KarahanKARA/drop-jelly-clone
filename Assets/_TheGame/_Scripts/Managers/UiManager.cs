using System;
using System.Collections;
using _TheGame._Scripts.References;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _TheGame._Scripts.Managers
{
    /// <summary>
    /// Manages UI interactions and game state transitions with an event-based architecture
    /// </summary>
    public class UiManager : Singleton<UiManager>
    {
        [Header("Transition Settings")]
        [SerializeField] private float winDelay = 2f;
        [SerializeField] private float failDelay = 0.6f;
        [SerializeField] private float restartDelay = 2f;
        
        [Header("Animation Settings")]
        [SerializeField] private float panelFadeInDuration = 0.5f;
        [SerializeField] private float textAnimationDuration = 0.3f;
        
        // UI State events
        public static event Action OnGameWin;
        public static event Action OnGameFail;
        public static event Action OnLevelStart;
        public static event Action OnLevelRestart;
        public static event Action OnReturnToMenu;
        
        // UI Data events
        public static event Action<int> OnMovesUpdated;
        public static event Action<int> OnLevelUpdated;
        
        private Coroutine _gameEndCoroutine;
        
        private void OnEnable()
        {
            BoardFlowManager.Instance.OnBoardCleared += HandleGameWin;
            
            SubscribeUIElements();
        }

        private void OnDisable()
        {
            if (BoardFlowManager.Instance != null)
            {
                BoardFlowManager.Instance.OnBoardCleared -= HandleGameWin;
            }
            
            UnsubscribeUIElements();
        }
        
        private void Start()
        {
            OnLevelStart?.Invoke();
        }
        
        private void SubscribeUIElements()
        {
            if (ObjectReferences.Instance != null)
            {
                OnGameWin += () => ShowPanel(ObjectReferences.Instance.congratsPanel);
                OnGameFail += () => ShowPanel(ObjectReferences.Instance.failPanel);
            }
            
            if (ComponentReferences.Instance != null)
            {
                OnMovesUpdated += UpdateMovesText;
                OnLevelUpdated += UpdateLevelText;
            }
        }
        
        private void UnsubscribeUIElements()
        {
            OnGameWin = null;
            OnGameFail = null;
            OnLevelStart = null;
            OnLevelRestart = null;
            OnReturnToMenu = null;
            OnMovesUpdated = null;
            OnLevelUpdated = null;
        }
        
        /// <summary>
        /// Handles showing a UI panel with animation
        /// </summary>
        private void ShowPanel(GameObject panel)
        {
            if (panel == null) return;
            
            panel.SetActive(true);
            
            var canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeInPanel(canvasGroup));
            }
        }
        
        private IEnumerator FadeInPanel(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0;
            
            float elapsedTime = 0;
            while (elapsedTime < panelFadeInDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / panelFadeInDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            canvasGroup.alpha = 1;
        }

        /// <summary>
        /// Updates the moves text with animation
        /// </summary>
        private void UpdateMovesText(int moveCount)
        {
            if (ComponentReferences.Instance?.movesText == null) return;
            
            // Animate text update
            AnimateTextUpdate(ComponentReferences.Instance.movesText, "Moves : " + moveCount);
        }
        
        /// <summary>
        /// Updates the level text with animation
        /// </summary>
        private void UpdateLevelText(int levelNumber)
        {
            if (ComponentReferences.Instance?.levelText == null) return;
            
            AnimateTextUpdate(ComponentReferences.Instance.levelText, "Level : " + levelNumber);
        }
        
        private void AnimateTextUpdate(TMPro.TextMeshProUGUI textField, string newText)
        {
            StartCoroutine(TextUpdateAnimation(textField, newText));
        }
        
        private IEnumerator TextUpdateAnimation(TMPro.TextMeshProUGUI textField, string newText)
        {
            var elapsedTime = 0f;
            var originalScale = textField.transform.localScale;
            
            while (elapsedTime < textAnimationDuration / 2)
            {
                textField.transform.localScale = Vector3.Lerp(
                    originalScale, 
                    originalScale * 0.8f, 
                    elapsedTime / (textAnimationDuration / 2)
                );
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            textField.text = newText;
            
            elapsedTime = 0;
            while (elapsedTime < textAnimationDuration / 2)
            {
                textField.transform.localScale = Vector3.Lerp(
                    originalScale * 0.8f, 
                    originalScale, 
                    elapsedTime / (textAnimationDuration / 2)
                );
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            textField.transform.localScale = originalScale;
        }

        #region Public Methods (called by UI buttons and other systems)
        
        /// <summary>
        /// Called by the replay button
        /// </summary>
        public void ReplayButtonOnClick()
        {
            OnLevelRestart?.Invoke();
            SceneManager.LoadScene(1);
        }

        /// <summary>
        /// Called by the back button
        /// </summary>
        public void BackButtonOnClick()
        {
            OnReturnToMenu?.Invoke();
            SceneManager.LoadScene(0);
        }

        /// <summary>
        /// Updates UI to reflect current move count
        /// </summary>
        public void SetMovesText(int moveCount)
        {
            OnMovesUpdated?.Invoke(moveCount);
        }

        /// <summary>
        /// Updates UI to reflect current level
        /// </summary>
        public void SetLevelText(string levelCount)
        {
            int level;
            if (int.TryParse(levelCount, out level))
            {
                OnLevelUpdated?.Invoke(level);
            }
        }

        /// <summary>
        /// Triggers the game fail state
        /// </summary>
        public void GameFail()
        {
            if (_gameEndCoroutine != null)
            {
                StopCoroutine(_gameEndCoroutine);
            }
            
            _gameEndCoroutine = StartCoroutine(GameFailSequence());
        }
        
        /// <summary>
        /// Triggers the game win state
        /// </summary>
        public void GameWin()
        {
            if (_gameEndCoroutine != null)
            {
                StopCoroutine(_gameEndCoroutine);
            }
            
            _gameEndCoroutine = StartCoroutine(GameWinSequence());
        }
        
        #endregion

        #region Event Handlers
        
        private void HandleGameWin()
        {
            GameWin();
        }
        
        #endregion

        #region Gameplay Sequences
        
        private IEnumerator GameFailSequence()
        {
            yield return new WaitForSeconds(failDelay);
            
            OnGameFail?.Invoke();
            
            yield return new WaitForSeconds(restartDelay);
            
            SceneManager.LoadScene(1);
        }
        
        private IEnumerator GameWinSequence()
        {
            OnGameWin?.Invoke();
            
            yield return new WaitForSeconds(winDelay);
            
            var currentLevelIndex = SaveManager.Instance.GetLastLevelIndex();
            if (currentLevelIndex == 3)
            {
                OnReturnToMenu?.Invoke();
                SceneManager.LoadScene(0);
            }
            else
            {
                SaveManager.Instance.SetLastLevelIndex(currentLevelIndex + 1);
                SceneManager.LoadScene(1);
            }
        }
        
        #endregion
    }
}