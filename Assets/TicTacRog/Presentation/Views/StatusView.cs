using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacRog.Presentation.Views
{
    public sealed class StatusView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Button _resetButton;

        public TextMeshProUGUI StatusText => _statusText;
        public Button ResetButton => _resetButton;
    }
}