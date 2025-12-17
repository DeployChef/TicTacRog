namespace TicTacRog.Infrastructure.Events
{
    public readonly struct GameFinishedEvent
    {
        public GameState State { get; }
        public GameFinishedEvent(GameState state) => State = state;
    }
}
