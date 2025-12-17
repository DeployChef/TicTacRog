using TicTacRog.Core.Domain;

namespace TicTacRog.Core.UseCases
{
    public interface IBoardRepository
    {
        GameState GetCurrent();
        void Save(GameState state);
    }
}