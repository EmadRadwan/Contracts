public class GeneralServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public T ResultData { get; set; }

    public static GeneralServiceResult<T> Success(T resultData = default) => 
        new GeneralServiceResult<T> { IsSuccess = true, ResultData = resultData };
    public static GeneralServiceResult<T> Error(string errorMessage) => 
        new GeneralServiceResult<T> { IsSuccess = false, ErrorMessage = errorMessage };
}