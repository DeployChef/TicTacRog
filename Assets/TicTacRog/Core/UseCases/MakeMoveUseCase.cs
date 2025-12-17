using TicTacRog.Core.Domain;
using TicTacRog.Core.UseCases;

public sealed class MakeMoveUseCase
{
    private readonly IBoardRepository _boardRepository;
    private readonly IGameRuleSet _ruleSet;
    private readonly IGameEvents _gameEvents;

    public MakeMoveUseCase(IBoardRepository boardRepository, IGameRuleSet ruleSet, IGameEvents gameEvents)
    {
        _boardRepository = boardRepository;
        _ruleSet = ruleSet;
        _gameEvents = gameEvents;
    }

    public void Execute(CellIndex targetCell)
    {
        var state = _boardRepository.GetCurrent();
        if (state.Status != GameStatus.InProgress)
            return;

        var board = state.Board;
        if (!board.IsEmpty(targetCell))
            return;

        board.SetMark(targetCell, state.CurrentPlayer);

        var status = _ruleSet.Evaluate(board, state.CurrentPlayer, targetCell);
        state.SetStatus(status);

        if (status == GameStatus.InProgress)
        {
            state.SwitchPlayer();
        }
        else
        {
            _gameEvents.OnGameFinished(state);
        }

        _boardRepository.Save(state);
        _gameEvents.OnMoveMade(state, targetCell);
    }
}