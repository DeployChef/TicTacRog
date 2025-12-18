using System;
using System.Linq;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;

namespace TicTacRog.Infrastructure.Bot
{
    public sealed class RandomBotPlayer : IBotPlayer
    {
        private readonly IBoardRepository _repository;
        private readonly MakeMoveUseCase _makeMoveUseCase;

        public RandomBotPlayer(
            IBoardRepository repository,
            MakeMoveUseCase makeMoveUseCase)
        {
            _repository = repository;
            _makeMoveUseCase = makeMoveUseCase;
        }

        public bool TryMakeMove(GameState state)
        {
            if (state.Status != GameStatus.InProgress)
                return false;

            if (state.CurrentPlayer != Mark.Nought)
                return false;

            var board = state.Board;
            var empty = new System.Collections.Generic.List<CellIndex>();

            for (int x = 0; x < board.Size; x++)
            for (int y = 0; y < board.Size; y++)
            {
                var idx = new CellIndex(x, y);
                if (board.GetMark(idx) == Mark.None)
                    empty.Add(idx);
            }

            if (empty.Count == 0)
                return false;

            int seed = CalculateSeedFromHistory(state.History);
            var random = new Random(seed);
            var choice = empty[random.Next(empty.Count)];
            var result = _makeMoveUseCase.Execute(choice);
            return result.IsSuccess;
        }

        private int CalculateSeedFromHistory(MoveHistory history)
        {
            if (history.Count == 0)
                return 0;

            int hash = 17;
            foreach (var move in history.Moves)
            {
                hash = hash * 31 + move.Cell.Row;
                hash = hash * 31 + move.Cell.Column;
                hash = hash * 31 + (int)move.Player;
            }
            return hash;
        }
    }
}

