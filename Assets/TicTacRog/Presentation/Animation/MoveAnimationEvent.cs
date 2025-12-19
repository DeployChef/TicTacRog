using System.Collections;
using UnityEngine;
using TicTacRog.Core.Domain;
using TicTacRog.Presentation.Views;

namespace TicTacRog.Presentation.Animation
{
    public sealed class MoveAnimationEvent : IAnimationEvent
    {
        private readonly CellView _cellView;
        private readonly Symbol _symbol;
        private readonly CellIndex _cellIndex;

        public CellIndex CellIndex => _cellIndex;
        public Symbol Symbol => _symbol;

        public MoveAnimationEvent(CellView cellView, Symbol symbol, CellIndex cellIndex)
        {
            _cellView = cellView;
            _symbol = symbol;
            _cellIndex = cellIndex;
        }

        public IEnumerator PlayAnimation()
        {
            _cellView.SetSymbol(_symbol);
            yield return _cellView.PlayAnimation();
        }
        
        public void StopAnimation()
        {
            _cellView.StopCurrentAnimation();
        }
    }
}

