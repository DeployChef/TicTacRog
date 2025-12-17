namespace TicTacRog.Infrastructure.Events.Messages
{
    public readonly struct GameFinishedMessage
    {
        public GameState State { get; }
        public GameFinishedMessage(GameState state) => State = state;
    }
}
