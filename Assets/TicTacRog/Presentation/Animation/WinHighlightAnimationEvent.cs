using System.Collections;
using TicTacRog.Presentation.Views;

namespace TicTacRog.Presentation.Animation
{
    public sealed class WinHighlightAnimationEvent : IAnimationEvent
    {
        private readonly CellView _cellView;

        public WinHighlightAnimationEvent(CellView cellView)
        {
            _cellView = cellView;
        }

        public IEnumerator PlayAnimation()
        {
            yield return _cellView.PlayWinHighlight();
        }
        
        public void StopAnimation()
        {
            _cellView.StopCurrentAnimation();
        }
    }
}

