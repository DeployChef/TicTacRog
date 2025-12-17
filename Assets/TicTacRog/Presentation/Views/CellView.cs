using UnityEngine;
using UnityEngine.UI;
using TicTacRog.Core.Domain;
using TMPro;

namespace TicTacRog.Presentation.Views
{
    public sealed class CellView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _label;

        private CellIndex _index;
        private System.Action<CellIndex> _onClicked;

        public void Init(CellIndex index, System.Action<CellIndex> onClicked)
        {
            _index = index;
            _onClicked = onClicked;
            _button.onClick.AddListener(HandleClick);
        }

        public void SetMark(Mark mark)
        {
            _label.text = mark switch
            {
                Mark.Cross => "X",
                Mark.Nought => "O",
                _ => ""
            };
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            _onClicked?.Invoke(_index);
        }
    }
}