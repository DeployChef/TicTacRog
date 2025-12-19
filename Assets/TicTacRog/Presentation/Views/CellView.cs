using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TicTacRog.Core.Domain;
using TMPro;
using DG.Tweening;

namespace TicTacRog.Presentation.Views
{
    public sealed class CellView : MonoBehaviour, IAnimatable
    {
        [Header("Components")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image background;

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.4f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;
        [SerializeField] private float punchScale = AnimationConstants.PunchScale;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;

        private CellIndex _index;
        private System.Action<CellIndex> _onClicked;
        private Symbol _currentSymbol;
        private Sequence _currentAnimation;

        public CellIndex Index => _index;
        public Button Button => button;

        public void Init(CellIndex index, System.Action<CellIndex> onClicked)
        {
            _index = index;
            _onClicked = onClicked;
            
            if (!button)
            {
                Debug.LogError("[CellView] Button component is not assigned!");
                return;
            }
            
            button.onClick.AddListener(HandleClick);
            ResetToNormalState();
        }

        public void SetSymbolImmediate(Symbol symbol)
        {
            KillAllAnimations();
            
            _currentSymbol = symbol;
            UpdateLabel();
            
            ResetToNormalState();
        }
        
        public void StopCurrentAnimation()
        {
            KillAllAnimations();
        }
        
        private void KillAllAnimations()
        {
            _currentAnimation?.Kill();
            transform.DOKill();
            if (label) label.DOKill();
            if (background) background.DOKill();
        }
        
        private void ResetToNormalState()
        {
            transform.localScale = Vector3.one;
            SetBackgroundColor(normalColor);
            SetLabelAlpha(1f);
            SetCanvasGroupAlpha(1f);
        }

        private void SetBackgroundColor(Color color)
        {
            if (background)
            {
                background.color = color;
            }
        }

        private void SetLabelAlpha(float alpha)
        {
            if (label)
            {
                var color = label.color;
                color.a = alpha;
                label.color = color;
            }
        }

        private void SetCanvasGroupAlpha(float alpha)
        {
            if (canvasGroup)
            {
                canvasGroup.alpha = alpha;
            }
        }

        public void SetSymbol(Symbol symbol)
        {
            _currentSymbol = symbol;
            UpdateLabel();
        }

        public IEnumerator PlayAnimation()
        {
            _currentAnimation?.Kill();
            _currentAnimation = DOTween.Sequence();

            BuildScaleAnimation();
            BuildLabelFadeAnimation();
            BuildBackgroundHighlightAnimation();

            yield return _currentAnimation.WaitForCompletion();
        }

        private void BuildScaleAnimation()
        {
            transform.localScale = Vector3.zero;
            
            _currentAnimation.Append(
                transform.DOScale(punchScale, animationDuration * AnimationConstants.ScaleUpDurationRatio)
                    .SetEase(scaleEase)
            );
            
            _currentAnimation.Append(
                transform.DOScale(1f, animationDuration * AnimationConstants.ScaleDownDurationRatio)
                    .SetEase(Ease.OutQuad)
            );
        }

        private void BuildLabelFadeAnimation()
        {
            if (!label) return;

            var color = label.color;
            color.a = 0f;
            label.color = color;
            
            _currentAnimation.Join(
                label.DOFade(1f, animationDuration * AnimationConstants.FadeInDurationRatio)
                    .SetEase(Ease.OutQuad)
            );
        }

        private void BuildBackgroundHighlightAnimation()
        {
            if (!background) return;

            var highlightDuration = animationDuration * AnimationConstants.HighlightDurationRatio;
            
            _currentAnimation.Insert(0,
                background.DOColor(highlightColor, highlightDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            _currentAnimation.Append(
                background.DOColor(normalColor, highlightDuration)
                    .SetEase(Ease.InQuad)
            );
        }

        public IEnumerator PlayWinHighlight()
        {
            if (!background) yield break;

            var sequence = DOTween.Sequence();
            
            for (int i = 0; i < AnimationConstants.WinHighlightCycles; i++)
            {
                sequence.Append(background.DOColor(highlightColor, AnimationConstants.WinHighlightDuration));
                sequence.Append(background.DOColor(normalColor, AnimationConstants.WinHighlightDuration));
            }

            yield return sequence.WaitForCompletion();
        }

        public void PlayErrorShake()
        {
            transform.DOKill(complete: true);
            
            transform.DOShakePosition(
                AnimationConstants.ErrorShakeDuration,
                strength: AnimationConstants.ErrorShakeStrength,
                vibrato: AnimationConstants.ErrorShakeVibrato,
                randomness: AnimationConstants.ErrorShakeRandomness,
                snapping: false,
                fadeOut: true)
                .SetEase(Ease.OutQuad);
        }

        private void UpdateLabel()
        {
            if (!label) return;
            
            if (_currentSymbol == null)
            {
                label.text = GameTextConstants.MarkEmpty;
                return;
            }
            
            label.text = _currentSymbol.Type switch
            {
                SymbolType.Cross => GameTextConstants.MarkCross,
                SymbolType.Nought => GameTextConstants.MarkNought,
                _ => GameTextConstants.MarkEmpty
            };
        }

        private void OnDestroy()
        {
            _currentAnimation?.Kill();
            transform.DOKill();
            if (label) label.DOKill();
            if (background) background.DOKill();
            
            button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            _onClicked?.Invoke(_index);
        }
    }
}