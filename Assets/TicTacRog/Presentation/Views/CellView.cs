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
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _background;

        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.4f;
        [SerializeField] private Ease _scaleEase = Ease.OutBack;
        [SerializeField] private float _punchScale = AnimationConstants.PunchScale;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _highlightColor = Color.yellow;

        private CellIndex _index;
        private System.Action<CellIndex> _onClicked;
        private Mark _currentMark = Mark.None;
        private Sequence _currentAnimation;

        public CellIndex Index => _index;
        public Button Button => _button;

        public void Init(CellIndex index, System.Action<CellIndex> onClicked)
        {
            _index = index;
            _onClicked = onClicked;
            
            if (_button == null)
            {
                Debug.LogError("[CellView] Button component is not assigned!");
                return;
            }
            
            _button.onClick.AddListener(HandleClick);
            ResetToNormalState();
        }

        public void SetMarkImmediate(Mark mark)
        {
            KillAllAnimations();
            
            _currentMark = mark;
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
            if (_label != null) _label.DOKill();
            if (_background != null) _background.DOKill();
        }
        
        private void ResetToNormalState()
        {
            transform.localScale = Vector3.one;
            SetBackgroundColor(_normalColor);
            SetLabelAlpha(1f);
            SetCanvasGroupAlpha(1f);
        }

        private void SetBackgroundColor(Color color)
        {
            if (_background != null)
            {
                _background.color = color;
            }
        }

        private void SetLabelAlpha(float alpha)
        {
            if (_label != null)
            {
                var color = _label.color;
                color.a = alpha;
                _label.color = color;
            }
        }

        private void SetCanvasGroupAlpha(float alpha)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = alpha;
            }
        }

        public void SetMark(Mark mark)
        {
            _currentMark = mark;
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
                transform.DOScale(_punchScale, _animationDuration * AnimationConstants.ScaleUpDurationRatio)
                    .SetEase(_scaleEase)
            );
            
            _currentAnimation.Append(
                transform.DOScale(1f, _animationDuration * AnimationConstants.ScaleDownDurationRatio)
                    .SetEase(Ease.OutQuad)
            );
        }

        private void BuildLabelFadeAnimation()
        {
            if (_label == null) return;

            var color = _label.color;
            color.a = 0f;
            _label.color = color;
            
            _currentAnimation.Join(
                _label.DOFade(1f, _animationDuration * AnimationConstants.FadeInDurationRatio)
                    .SetEase(Ease.OutQuad)
            );
        }

        private void BuildBackgroundHighlightAnimation()
        {
            if (_background == null) return;

            var highlightDuration = _animationDuration * AnimationConstants.HighlightDurationRatio;
            
            _currentAnimation.Insert(0,
                _background.DOColor(_highlightColor, highlightDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            _currentAnimation.Append(
                _background.DOColor(_normalColor, highlightDuration)
                    .SetEase(Ease.InQuad)
            );
        }

        public IEnumerator PlayWinHighlight()
        {
            if (_background == null) yield break;

            var sequence = DOTween.Sequence();
            
            for (int i = 0; i < AnimationConstants.WinHighlightCycles; i++)
            {
                sequence.Append(_background.DOColor(_highlightColor, AnimationConstants.WinHighlightDuration));
                sequence.Append(_background.DOColor(_normalColor, AnimationConstants.WinHighlightDuration));
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
            if (_label == null) return;
            
            _label.text = _currentMark switch
            {
                Mark.Cross => GameTextConstants.MarkCross,
                Mark.Nought => GameTextConstants.MarkNought,
                _ => GameTextConstants.MarkEmpty
            };
        }

        private void OnDestroy()
        {
            _currentAnimation?.Kill();
            transform.DOKill();
            if (_label != null) _label.DOKill();
            if (_background != null) _background.DOKill();
            
            _button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            _onClicked?.Invoke(_index);
        }
    }
}