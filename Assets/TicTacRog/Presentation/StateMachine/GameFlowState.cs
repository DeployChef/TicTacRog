namespace TicTacRog.Presentation.StateMachine
{
    /// <summary>
    /// Состояния игрового процесса (для UI)
    /// </summary>
    public enum GameFlowState
    {
        /// <summary>
        /// Ожидание хода игрока
        /// </summary>
        WaitingForPlayerInput,
        
        /// <summary>
        /// Проигрывается анимация хода игрока
        /// </summary>
        AnimatingPlayerMove,
        
        /// <summary>
        /// Бот "думает"
        /// </summary>
        BotThinking,
        
        /// <summary>
        /// Проигрывается анимация хода бота
        /// </summary>
        AnimatingBotMove,
        
        /// <summary>
        /// Игра завершена
        /// </summary>
        GameFinished
    }
}

