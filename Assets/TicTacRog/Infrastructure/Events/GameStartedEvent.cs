namespace TicTacRog.Infrastructure.Events
{
    public readonly struct GameStartedEvent
    {
        public GameState State { get; }
        public GameStartedEvent(GameState state) => State = state;
    }
}
