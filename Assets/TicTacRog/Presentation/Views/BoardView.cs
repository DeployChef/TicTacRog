using UnityEngine;

namespace TicTacRog.Presentation.Views
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private Transform cellsRoot;
        [SerializeField] private GameObject cellPrefab;

        public Transform CellsRoot => cellsRoot;
        public GameObject CellPrefab => cellPrefab;
    }
}