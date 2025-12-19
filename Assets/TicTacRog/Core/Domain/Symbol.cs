namespace TicTacRog.Core.Domain
{
    /// <summary>
    /// Символ для размещения на поле. В будущем будет содержать эффекты и особое поведение.
    /// </summary>
    public sealed class Symbol
    {
        public SymbolType Type { get; }

        public Symbol(SymbolType type)
        {
            Type = type;
        }

        public override bool Equals(object obj)
        {
            return obj is Symbol other && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
    }

    /// <summary>
    /// Тип символа (временно, пока не добавим эффекты)
    /// </summary>
    public enum SymbolType
    {
        None = 0,
        Cross = 1,
        Nought = 2
    }
}
