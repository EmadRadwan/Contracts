namespace Application.Accounting.Services.Models
{
    public static class ServiceHelper
    {
        public static ServiceResult<T> ReturnError<T>(string errorMessage)
        {
            return new ServiceResult<T>
            {
                IsError = true,
                ErrorMessage = errorMessage,
                Data = default(T)
            };
        }
    }
}
