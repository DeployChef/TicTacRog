using TicTacRog.Core.Domain;

namespace TicTacRog.Core.Domain
{
    public sealed class Classic3X3RuleSet : IGameRuleSet
    {
        public GameStatus Evaluate(Board board, Mark lastMark, CellIndex lastMove)
        {
            if (lastMark == Mark.None)
                return GameStatus.InProgress;

            var size = board.Size;

            // Проверка строки
            bool rowWin = true;
            for (int col = 0; col < size; col++)
            {
                if (board.GetMark(new CellIndex(lastMove.Row, col)) != lastMark)
                {
                    rowWin = false;
                    break;
                }
            }

            if (rowWin)
                return GameStatus.Win;

            // Проверка колонки
            bool colWin = true;
            for (int row = 0; row < size; row++)
            {
                if (board.GetMark(new CellIndex(row, lastMove.Column)) != lastMark)
                {
                    colWin = false;
                    break;
                }
            }

            if (colWin)
                return GameStatus.Win;

            // Главная диагональ
            if (lastMove.Row == lastMove.Column)
            {
                bool diagWin = true;
                for (int i = 0; i < size; i++)
                {
                    if (board.GetMark(new CellIndex(i, i)) != lastMark)
                    {
                        diagWin = false;
                        break;
                    }
                }

                if (diagWin)
                    return GameStatus.Win;
            }

            // Побочная диагональ
            if (lastMove.Row + lastMove.Column == size - 1)
            {
                bool antiDiagWin = true;
                for (int i = 0; i < size; i++)
                {
                    if (board.GetMark(new CellIndex(i, size - 1 - i)) != lastMark)
                    {
                        antiDiagWin = false;
                        break;
                    }
                }

                if (antiDiagWin)
                    return GameStatus.Win;
            }

            // Проверка на ничью: нет пустых клеток
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (board.GetMark(new CellIndex(row, col)) == Mark.None)
                        return GameStatus.InProgress;
                }
            }

            return GameStatus.Draw;
        }
    }
}
