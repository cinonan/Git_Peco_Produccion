namespace CEAM.AzureSearch.WebApp.Models
{
    public class BaseResponse<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public T Object { get; set; }
    }
}
