using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;

namespace TicTacRog.Infrastructure.Repositories
{
    public sealed class InMemoryBoardRepository : IBoardRepository
    {
        private GameState _current;

        public GameState GetCurrent()
        {
            return _current;
        }

        public void Save(GameState state)
        {
            _current = state;
        }
    }
}