using System.Collections;

namespace TicTacRog.Presentation.Animation
{
    public interface IAnimationEvent
    {
        IEnumerator PlayAnimation();
        void StopAnimation();
    }
}

