namespace CrestBank.Http
{
    public interface IBankHttpClient
    {
        Task<(bool Success, string Message)> TransferAsync(TransferRequestDto transfer);
    }
}
