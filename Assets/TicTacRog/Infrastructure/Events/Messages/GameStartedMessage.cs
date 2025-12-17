namespace TicTacRog.Infrastructure.Events.Messages
{
    public readonly struct GameStartedMessage
    {
        public GameState State { get; }
        public GameStartedMessage(GameState state) => State = state;
    }
}
