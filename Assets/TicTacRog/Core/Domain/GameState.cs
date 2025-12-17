namespace TicTacRog.Core.Domain
{
    public sealed class GameState
    {
        public Board Board { get; }
        public Mark CurrentPlayer { get; private set; }
        public GameStatus Status { get; private set; }

        public GameState(Board board, Mark startingPlayer)
        {
            Board = board;
            CurrentPlayer = startingPlayer;
            Status = GameStatus.InProgress;
        }

        public void SetStatus(GameStatus status)
        {
            Status = status;
        }

        public void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == Mark.Cross ? Mark.Nought : Mark.Cross;
        }
    }
}