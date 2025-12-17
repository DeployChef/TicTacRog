using UnityEngine;

namespace TicTacRog.Presentation.Views
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private Transform _cellsRoot;
        [SerializeField] private GameObject _cellPrefab;

        public Transform CellsRoot => _cellsRoot;
        public GameObject CellPrefab => _cellPrefab;
    }
}