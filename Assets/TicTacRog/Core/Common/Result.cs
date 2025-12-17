namespace TicTacRog.Core.Common
{
    /// <summary>
    /// Результат операции с возможностью успеха или ошибки.
    /// </summary>
    public readonly struct Result
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }

        private Result(bool isSuccess, string errorMessage = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static Result Success() => new Result(true);
        public static Result Failure(string errorMessage) => new Result(false, errorMessage);
    }

    /// <summary>
    /// Результат операции с данными.
    /// </summary>
    public readonly struct Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string ErrorMessage { get; }

        private Result(bool isSuccess, T value, string errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, null);
        public static Result<T> Failure(string errorMessage) => new Result<T>(false, default, errorMessage);
    }
}

