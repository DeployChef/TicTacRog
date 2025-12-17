using TicTacRog.Core.Domain;

namespace TicTacRog.Core.UseCases
{
    public interface IBotPlayer
    {
        bool TryMakeMove(GameState state);
    }
}
