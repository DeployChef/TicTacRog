using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;
using TicTacRog.Presentation.Presenters;
using TicTacRog.Presentation;
using UnityEngine;
using VContainer.Unity;
using DG.Tweening;

namespace TicTacRog.Presentation.DI
{
    public sealed class GameEntryPoint : IStartable
    {
        private readonly int _boardSize;
        private readonly SymbolType _startingPlayerType;
        private readonly StartNewGameUseCase _startNewGame;
        private readonly GamePresenter _gamePresenter;
        private readonly BotController _botController;

        public GameEntryPoint(
            int boardSize,
            SymbolType startingPlayerType,
            StartNewGameUseCase startNewGame,
            GamePresenter gamePresenter,
            BotController botController)
        {
            _boardSize = boardSize;
            _startingPlayerType = startingPlayerType;
            _startNewGame = startNewGame;
            _gamePresenter = gamePresenter;
            _botController = botController;
        }

        public void Start()
        {
            Debug.Log($"[GameEntryPoint] Starting game: {_boardSize}x{_boardSize}, Starting player: {_startingPlayerType}");
            
            InitializeDoTween();
            
            var result = _startNewGame.Execute(_boardSize, _startingPlayerType);
            
            if (!result.IsSuccess)
            {
                Debug.LogError($"[GameEntryPoint] Failed to start game: {result.ErrorMessage}");
                return;
            }
            
            _gamePresenter.Initialize();
            _botController.Initialize();
            
            Debug.Log("[GameEntryPoint] Game started successfully!");
        }

        private static void InitializeDoTween()
        {
            DOTween.Init();
            Debug.Log("[GameEntryPoint] DOTween initialized");
        }
    }
}