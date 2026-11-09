namespace CrestBank.Http
{
    public interface IBankHttpClient
    {
        Task<(bool Success, string Message)> TransferAsync(TransferRequestDto transfer);

        Task<(bool Success, string Message)> BuyAirtimeAsync(AirtimeRequestDto order);
    }
}
