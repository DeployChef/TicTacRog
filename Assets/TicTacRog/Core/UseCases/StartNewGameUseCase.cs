using System.Collections.Generic;
using System.Linq;
using TicTacRog.Core.Common;
using TicTacRog.Core.Domain;

namespace TicTacRog.Core.UseCases
{
    public sealed class StartNewGameUseCase
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IGameEvents _gameEvents;

        public StartNewGameUseCase(IBoardRepository boardRepository, IGameEvents gameEvents)
        {
            _boardRepository = boardRepository;
            _gameEvents = gameEvents;
        }

        public Result Execute(int boardSize, SymbolType startingPlayerType)
        {
            if (boardSize <= 0)
                return Result.Failure("Board size must be positive.");
            
            var board = new Board(boardSize);
            
            // Создаем деки: у игрока 5 крестиков, у бота 5 ноликов
            var playerDeckSymbols = Enumerable.Range(0, 3)
                .Select(_ => new Symbol(SymbolType.Cross))
                .ToList();
            var playerDeck = new Deck(playerDeckSymbols);
            
            var botDeckSymbols = Enumerable.Range(0, 5)
                .Select(_ => new Symbol(SymbolType.Nought))
                .ToList();
            var botDeck = new Deck(botDeckSymbols);
            
            // Создаем руки и берем по 3 символа из дек
            var playerHand = new Hand();
            var playerDrawn = playerDeck.Draw(3);
            playerHand.AddSymbols(playerDrawn);
            
            var botHand = new Hand();
            var botDrawn = botDeck.Draw(3);
            botHand.AddSymbols(botDrawn);
            
            var state = new GameState(board, startingPlayerType, playerDeck, playerHand, botDeck, botHand);
            _boardRepository.Save(state);
            _gameEvents.OnGameStarted(state);
            
            return Result.Success();
        }
    }
}