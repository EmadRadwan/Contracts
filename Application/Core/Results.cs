namespace Application.Core
{
    public class Results<T>
    {
        public bool IsSuccess { get; set; }
        public T Value { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }

        public static Results<T> Success(T value) => new Results<T> { IsSuccess = true, Value = value };

        public static Results<T> Failure(string errorMessage, string errorCode = null) =>
            new Results<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode ?? "DEFAULT" // REFACTOR: Default to "DEFAULT" if no ErrorCode provided
            };
    }
}