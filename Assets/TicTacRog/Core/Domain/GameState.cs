namespace TicTacRog.Core.Domain
{
    public sealed class GameState
    {
        public Board Board { get; }
        public SymbolType CurrentPlayerType { get; private set; }
        public GameStatus Status { get; private set; }
        public SymbolType WinnerType { get; private set; }
        public MoveHistory History { get; }

        // Деки и руки игроков
        public Deck PlayerDeck { get; }
        public Hand PlayerHand { get; }
        public Deck BotDeck { get; }
        public Hand BotHand { get; }

        public GameState(Board board, SymbolType startingPlayerType, Deck playerDeck, Hand playerHand, Deck botDeck, Hand botHand)
        {
            Board = board;
            CurrentPlayerType = startingPlayerType;
            Status = GameStatus.InProgress;
            WinnerType = SymbolType.None;
            History = new MoveHistory();
            
            PlayerDeck = playerDeck ?? throw new System.ArgumentNullException(nameof(playerDeck));
            PlayerHand = playerHand ?? throw new System.ArgumentNullException(nameof(playerHand));
            BotDeck = botDeck ?? throw new System.ArgumentNullException(nameof(botDeck));
            BotHand = botHand ?? throw new System.ArgumentNullException(nameof(botHand));
        }

        public void SetStatus(GameStatus status, SymbolType winnerType = SymbolType.None)
        {
            Status = status;
            WinnerType = winnerType;
        }

        public void SwitchPlayer()
        {
            CurrentPlayerType = CurrentPlayerType == SymbolType.Cross ? SymbolType.Nought : SymbolType.Cross;
        }

        /// <summary>
        /// Получить текущую деку в зависимости от текущего игрока.
        /// </summary>
        public Deck GetCurrentDeck()
        {
            return CurrentPlayerType == SymbolType.Cross ? PlayerDeck : BotDeck;
        }

        /// <summary>
        /// Получить текущую руку в зависимости от текущего игрока.
        /// </summary>
        public Hand GetCurrentHand()
        {
            return CurrentPlayerType == SymbolType.Cross ? PlayerHand : BotHand;
        }
    }
}