namespace TicTacRog.Core.Domain
{
    public sealed class GameState
    {
        public Board Board { get; }
        public Mark CurrentPlayer { get; private set; }
        public GameStatus Status { get; private set; }
        public Mark Winner { get; private set; }
        public MoveHistory History { get; }

        public GameState(Board board, Mark startingPlayer)
        {
            Board = board;
            CurrentPlayer = startingPlayer;
            Status = GameStatus.InProgress;
            Winner = Mark.None;
            History = new MoveHistory();
        }

        public void SetStatus(GameStatus status, Mark winner = Mark.None)
        {
            Status = status;
            Winner = winner;
        }

        public void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == Mark.Cross ? Mark.Nought : Mark.Cross;
        }
    }
}