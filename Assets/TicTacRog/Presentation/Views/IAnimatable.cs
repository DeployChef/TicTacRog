using System.Collections;

namespace TicTacRog.Presentation.Views
{
    /// <summary>
    /// Интерфейс для компонентов с анимациями
    /// </summary>
    public interface IAnimatable
    {
        IEnumerator PlayAnimation();
    }
}

