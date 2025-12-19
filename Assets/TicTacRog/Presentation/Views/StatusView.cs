using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacRog.Presentation.Views
{
    public sealed class StatusView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button resetButton;

        public TextMeshProUGUI StatusText => statusText;
        public Button ResetButton => resetButton;
    }
}