using System.Collections;
using UnityEngine;
using TicTacRog.Core.Domain;
using TicTacRog.Presentation.Views;

namespace TicTacRog.Presentation.Animation
{
    /// <summary>
    /// Событие анимации хода
    /// </summary>
    public sealed class MoveAnimationEvent : IAnimationEvent
    {
        private readonly CellView _cellView;
        private readonly Mark _mark;
        private readonly CellIndex _cellIndex;

        public CellIndex CellIndex => _cellIndex;
        public Mark Mark => _mark;

        public MoveAnimationEvent(CellView cellView, Mark mark, CellIndex cellIndex)
        {
            _cellView = cellView;
            _mark = mark;
            _cellIndex = cellIndex;
        }

        public IEnumerator PlayAnimation()
        {
            // Устанавливаем метку
            _cellView.SetMark(_mark);
            
            // Проигрываем анимацию
            yield return _cellView.PlayAnimation();
        }
    }
}

