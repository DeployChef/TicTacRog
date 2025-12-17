using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TicTacRog.Core.Domain;
using TMPro;
using DG.Tweening;

namespace TicTacRog.Presentation.Views
{
    /// <summary>
    /// View клетки игрового поля с DOTween анимациями
    /// </summary>
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
        [SerializeField] private float _punchScale = 1.15f;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _highlightColor = Color.yellow;

        private CellIndex _index;
        private System.Action<CellIndex> _onClicked;
        private Mark _currentMark = Mark.None;
        private Sequence _currentAnimation;

        public CellIndex Index => _index;
        public Button Button => _button;

        private void Awake()
        {
            // Убиваем все анимации при уничтожении
            DOTween.Init();
        }

        public void Init(CellIndex index, System.Action<CellIndex> onClicked)
        {
            _index = index;
            _onClicked = onClicked;
            _button.onClick.AddListener(HandleClick);
            
            // Начальное состояние
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
            transform.localScale = Vector3.one;
            
            if (_background != null)
            {
                _background.color = _normalColor;
            }
        }

        /// <summary>
        /// Устанавливает метку мгновенно (без анимации)
        /// </summary>
        public void SetMarkImmediate(Mark mark)
        {
            // Убиваем текущую анимацию если есть
            _currentAnimation?.Kill();
            
            _currentMark = mark;
            UpdateLabel();
            transform.localScale = Vector3.one;
            
            if (_label != null)
            {
                var color = _label.color;
                color.a = 1f;
                _label.color = color;
            }
        }

        /// <summary>
        /// Устанавливает метку (сама анимация в PlayAnimation)
        /// </summary>
        public void SetMark(Mark mark)
        {
            _currentMark = mark;
            UpdateLabel();
        }

        /// <summary>
        /// Проигрывает анимацию появления метки с DOTween
        /// </summary>
        public IEnumerator PlayAnimation()
        {
            // Убиваем предыдущую анимацию
            _currentAnimation?.Kill();

            // Создаем последовательность анимаций
            _currentAnimation = DOTween.Sequence();

            // 1. Появление текста (scale + fade)
            transform.localScale = Vector3.zero;
            _currentAnimation.Append(
                transform.DOScale(_punchScale, _animationDuration * 0.6f)
                    .SetEase(_scaleEase)
            );
            
            // 2. Небольшой отскок
            _currentAnimation.Append(
                transform.DOScale(1f, _animationDuration * 0.4f)
                    .SetEase(Ease.OutQuad)
            );

            // 3. Fade in текста (параллельно)
            if (_label != null)
            {
                var color = _label.color;
                color.a = 0f;
                _label.color = color;
                
                _currentAnimation.Join(
                    _label.DOFade(1f, _animationDuration * 0.5f)
                        .SetEase(Ease.OutQuad)
                );
            }

            // 4. Highlight эффект (опционально)
            if (_background != null)
            {
                _currentAnimation.Insert(0,
                    _background.DOColor(_highlightColor, _animationDuration * 0.3f)
                        .SetEase(Ease.OutQuad)
                );
                _currentAnimation.Append(
                    _background.DOColor(_normalColor, _animationDuration * 0.3f)
                        .SetEase(Ease.InQuad)
                );
            }

            // Ждем завершения
            yield return _currentAnimation.WaitForCompletion();
        }

        /// <summary>
        /// Анимация подсветки (для победной линии)
        /// </summary>
        public IEnumerator PlayWinHighlight()
        {
            if (_background == null) yield break;

            var sequence = DOTween.Sequence();
            
            // Пульсация
            for (int i = 0; i < 3; i++)
            {
                sequence.Append(_background.DOColor(_highlightColor, 0.3f));
                sequence.Append(_background.DOColor(_normalColor, 0.3f));
            }

            yield return sequence.WaitForCompletion();
        }

        /// <summary>
        /// Анимация ошибки (клик на занятую клетку)
        /// </summary>
        public void PlayErrorShake()
        {
            transform.DOShakePosition(0.3f, strength: 10f, vibrato: 10)
                .SetEase(Ease.OutQuad);
        }

        private void UpdateLabel()
        {
            if (_label == null) return;
            
            _label.text = _currentMark switch
            {
                Mark.Cross => "X",
                Mark.Nought => "O",
                _ => ""
            };
        }

        private void OnDestroy()
        {
            // Очищаем анимации
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