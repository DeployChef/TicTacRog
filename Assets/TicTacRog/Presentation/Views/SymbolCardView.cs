using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TicTacRog.Core.Domain;

namespace TicTacRog.Presentation.Views
{
    /// <summary>
    /// Карточка символа в руке с поддержкой drag & drop.
    /// </summary>
    public sealed class SymbolCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Components")]
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI symbolText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Drag Settings")]
        [SerializeField] private float dragAlpha = 0.8f;
        [SerializeField] private float normalAlpha = 1f;

        private Symbol _symbol;
        private Transform _originalParent;
        private Canvas _canvas;
        private RectTransform _canvasRectTransform;
        private RectTransform _rectTransform;
        private Vector2 _originalPosition;
        private Vector2 _originalLocalPosition;
        private Vector2 _dragOffset;
        private System.Action<Symbol, CellIndex?> _onDragEnd;

        public Symbol Symbol => _symbol;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null)
            {
                _canvasRectTransform = _canvas.GetComponent<RectTransform>();
            }
        }

        public void Init(Symbol symbol, System.Action<Symbol, CellIndex?> onDragEnd)
        {
            _symbol = symbol;
            _onDragEnd = onDragEnd;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (symbolText == null) return;

            if (_symbol == null)
            {
                symbolText.text = "";
                return;
            }

            symbolText.text = _symbol.Type switch
            {
                SymbolType.Cross => GameTextConstants.MarkCross,
                SymbolType.Nought => GameTextConstants.MarkNought,
                _ => ""
            };
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_symbol == null || _rectTransform == null) return;

            _originalParent = transform.parent;
            _originalPosition = _rectTransform.anchoredPosition;
            
            // Получаем Canvas если еще не получили
            if (_canvas == null)
            {
                _canvas = GetComponentInParent<Canvas>();
                if (_canvas != null)
                {
                    _canvasRectTransform = _canvas.GetComponent<RectTransform>();
                }
            }
            
            if (_canvasRectTransform == null) return;
            
            // Сохраняем мировую позицию карточки
            Vector3 worldPos = _rectTransform.position;
            
            // Перемещаем на верхний уровень Canvas для корректного отображения поверх всего
            transform.SetParent(_canvasRectTransform, false);
            transform.SetAsLastSibling();
            
            // Восстанавливаем мировую позицию
            _rectTransform.position = worldPos;
            
            // Теперь вычисляем offset в координатах Canvas
            Vector2 localPointerPosition;
            Camera cam = _canvas.renderMode != RenderMode.ScreenSpaceOverlay ? _canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform,
                eventData.position,
                cam,
                out localPointerPosition))
            {
                _dragOffset = _rectTransform.anchoredPosition - localPointerPosition;
            }
            else
            {
                _dragOffset = Vector2.zero;
            }
            
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_symbol == null || _rectTransform == null || _canvasRectTransform == null) return;

            // Преобразуем позицию экрана в локальные координаты Canvas
            Vector2 localPointerPosition;
            Camera cam = _canvas.renderMode != RenderMode.ScreenSpaceOverlay ? _canvas.worldCamera : null;
            
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform,
                eventData.position,
                cam,
                out localPointerPosition))
            {
                // Применяем offset, чтобы карточка следовала за мышкой
                _rectTransform.anchoredPosition = localPointerPosition + _dragOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_symbol == null) return;

            canvasGroup.alpha = normalAlpha;
            canvasGroup.blocksRaycasts = true;

            // Проверяем, был ли drop на валидную цель
            // Используем pointerEnter вместо pointerCurrentRaycast, так как blocksRaycasts был false
            GameObject dropTarget = eventData.pointerEnter;
            
            // Если pointerEnter null, пробуем найти через raycast
            if (dropTarget == null)
            {
                var results = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);
                
                foreach (var result in results)
                {
                    if (result.gameObject != gameObject) // Игнорируем саму карточку
                    {
                        dropTarget = result.gameObject;
                        break;
                    }
                }
            }

            CellIndex? cellIndex = null;
            if (dropTarget != null)
            {
                var cellView = dropTarget.GetComponent<CellView>();
                if (cellView != null)
                {
                    cellIndex = cellView.Index;
                }
            }

            // Вызываем callback
            _onDragEnd?.Invoke(_symbol, cellIndex);

            // Если drop не на ячейку - возвращаем карточку на место
            if (!cellIndex.HasValue)
            {
                transform.SetParent(_originalParent, false);
                _rectTransform.anchoredPosition = _originalPosition;
            }
        }
    }
}
