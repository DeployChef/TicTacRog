using System.Collections;

namespace TicTacRog.Presentation.Animation
{
    /// <summary>
    /// Интерфейс для события, которое можно проанимировать
    /// </summary>
    public interface IAnimationEvent
    {
        /// <summary>
        /// Проигрывает анимацию события
        /// </summary>
        IEnumerator PlayAnimation();
    }
}

