namespace TicTacRog.Presentation.Views
{
    /// <summary>
    /// Константы для анимаций клеток
    /// </summary>
    public static class AnimationConstants
    {
        // Тайминги анимации (в долях от общей длительности)
        public const float ScaleUpDurationRatio = 0.6f;
        public const float ScaleDownDurationRatio = 0.4f;
        public const float FadeInDurationRatio = 0.5f;
        public const float HighlightDurationRatio = 0.3f;
        
        // Параметры анимации
        public const float PunchScale = 1.15f;
        public const float ErrorShakeDuration = 0.3f;
        public const float ErrorShakeStrength = 10f;
        public const int ErrorShakeVibrato = 10;
        public const int ErrorShakeRandomness = 90;
        
        // Параметры анимации победы
        public const float WinHighlightDuration = 0.3f;
        public const int WinHighlightCycles = 3;
    }
}

