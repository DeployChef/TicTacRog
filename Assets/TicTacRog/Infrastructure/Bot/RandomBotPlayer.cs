using System;
using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;

namespace TicTacRog.Infrastructure.Bot
{
    /// <summary>
    /// Бот, который делает случайный ход среди доступных клеток.
    /// </summary>
    public sealed class RandomBotPlayer : IBotPlayer
    {
        private readonly IBoardRepository _repository;
        private readonly MakeMoveUseCase _makeMoveUseCase;
        private readonly Random _random = new Random();

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

            if (state.CurrentPlayer != Mark.Nought) // бот играет ноликами
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

            var choice = empty[_random.Next(empty.Count)];
            var result = _makeMoveUseCase.Execute(choice);
            return result.IsSuccess;
        }
    }
}

