using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Presentation.Presenters;
using UnityEngine;
using VContainer.Unity;

namespace TicTacRog.Presentation.DI
{
    /// <summary>
    /// Точка входа в приложение
    /// Инициализирует игру через DI контейнер
    /// </summary>
    public sealed class GameEntryPoint : IStartable
    {
        private readonly int _boardSize;
        private readonly Mark _startingPlayer;
        private readonly StartNewGameUseCase _startNewGame;
        private readonly GamePresenter _gamePresenter;

        public GameEntryPoint(
            int boardSize,
            Mark startingPlayer,
            StartNewGameUseCase startNewGame,
            GamePresenter gamePresenter)
        {
            _boardSize = boardSize;
            _startingPlayer = startingPlayer;
            _startNewGame = startNewGame;
            _gamePresenter = gamePresenter;
        }

        public void Start()
        {
            Debug.Log($"[GameEntryPoint] Starting game: {_boardSize}x{_boardSize}, Starting player: {_startingPlayer}");
            
            // ВАЖНО: Сначала создаем игру, ПОТОМ инициализируем презентер!
            // Иначе презентер попытается прочитать несуществующее состояние
            
            // 1. Создаем новую игру (создает Board и GameState)
            _startNewGame.Execute(_boardSize, _startingPlayer);
            
            // 2. Инициализируем презентер (подписывается на события, строит UI)
            _gamePresenter.Initialize();
            
            Debug.Log("[GameEntryPoint] Game started successfully!");
        }
    }
}