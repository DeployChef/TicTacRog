using TicTacRog.Core.Domain;
using TicTacRog.Presentation.StateMachine;
using TicTacRog.Presentation.Views;

namespace TicTacRog.Presentation.Presenters
{
    public sealed class StatusTextFormatter
    {
        public string GetStatusText(GameState state, GameFlowState flowState)
        {
            if (state.Status != GameStatus.InProgress)
            {
                return state.Status switch
                {
                    GameStatus.Win => GetWinText(state),
                    GameStatus.Draw => GameTextConstants.StatusDraw,
                    _ => GameTextConstants.StatusGameOver
                };
            }

            return flowState switch
            {
                GameFlowState.WaitingForPlayerInput => GameTextConstants.StatusYourTurn,
                GameFlowState.AnimatingPlayerMove => GameTextConstants.StatusPlacingX,
                GameFlowState.BotThinking => GameTextConstants.StatusBotThinking,
                GameFlowState.AnimatingBotMove => GameTextConstants.StatusBotPlacing,
                GameFlowState.GameFinished => GameTextConstants.StatusGameFinished,
                _ => GameTextConstants.StatusInProgress
            };
        }

        private string GetWinText(GameState state)
        {
            var winner = state.Winner != Mark.None ? state.Winner : state.CurrentPlayer;
            
            return winner switch
            {
                Mark.Cross => GameTextConstants.StatusYouWin,
                Mark.Nought => GameTextConstants.StatusBotWins,
                _ => GameTextConstants.StatusGameOver
            };
        }
    }
}

