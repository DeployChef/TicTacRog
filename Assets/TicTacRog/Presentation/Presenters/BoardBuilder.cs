using System.Collections.Generic;
using UnityEngine;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Presentation.Views;

namespace TicTacRog.Presentation.Presenters
{
    public sealed class BoardBuilder
    {
        private readonly BoardView _boardView;
        private readonly IBoardRepository _repository;
        private readonly Dictionary<CellIndex, CellView> _cellViews = new();

        public IReadOnlyDictionary<CellIndex, CellView> CellViews => _cellViews;

        public BoardBuilder(BoardView boardView, IBoardRepository repository)
        {
            _boardView = boardView ?? throw new System.ArgumentNullException(nameof(boardView));
            _repository = repository ?? throw new System.ArgumentNullException(nameof(repository));
        }

        public void BuildBoard(System.Action<CellIndex> onCellClicked)
        {
            var state = _repository.GetCurrent();
            
            if (state == null)
            {
                Debug.LogError("[BoardBuilder] Cannot build board: game state is null! " +
                    "Make sure StartNewGameUseCase.Execute() is called BEFORE building board");
                return;
            }
            
            if (state.Board == null)
            {
                Debug.LogError("[BoardBuilder] Cannot build board: board is null!");
                return;
            }
            
            if (!_boardView.CellPrefab)
            {
                Debug.LogError("[BoardBuilder] Cannot build board: Cell Prefab is not assigned! " +
                    "Please assign Cell prefab to BoardView in Inspector.");
                return;
            }

            var size = state.Board.Size;
            _cellViews.Clear();

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    var index = new CellIndex(row, col);
                    var go = Object.Instantiate(_boardView.CellPrefab, _boardView.CellsRoot);
                    var view = go.GetComponent<CellView>();
                    
                    if (!view)
                    {
                        Debug.LogError($"[BoardBuilder] CellView component not found on prefab!");
                        Object.Destroy(go);
                        continue;
                    }
                    
                    view.Init(index, onCellClicked);
                    _cellViews[index] = view;
                }
            }

            Debug.Log($"[BoardBuilder] Board built: {size}x{size} = {_cellViews.Count} cells");
        }

        public void RedrawBoardImmediate(GameState state)
        {
            foreach (var (index, cellView) in _cellViews)
            {
                var symbol = state.Board.GetSymbol(index);
                cellView.SetSymbolImmediate(symbol);
            }
        }
    }
}

