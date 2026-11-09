using CrestBank.Http;

namespace CrestBank;

public class AirtimeService
{
    private readonly IAirtimeScreen _screen;

    private readonly IInputReader _input;

    private readonly ICommonOutcomes _outcomes;

    private readonly IBankHttpClient _httpClient;

    public AirtimeService(
        IAirtimeScreen screen,
        IInputReader input,
        ICommonOutcomes outcomes,
        IBankHttpClient httpClient)
    {
        _screen = screen;
        _input = input;
        _outcomes = outcomes;
        _httpClient = httpClient;
    }

    // This bank has no telco relationship: Bank A sells, this bank only collects
    // the money and asks. The debit is taken first and reversed if A says no.
    public void BuyAirtime(Account account)
    {
        string? phone = _input.ReadPhoneNumber();
        if (phone == null) return;

        string? network = _input.ChooseNetwork();
        if (network == null)
        {
            _screen.DisplayInvalidNetwork();
            return;
        }

        if (!_input.TryReadAmount(
            "How much do you want to recharge? ",
            out decimal amount))
            return;

        if (!account.TryDebit(amount, $"Airtime for {phone} ({network})", "Airtime"))
        {
            _outcomes.DisplayInsufficientFunds();
            return;
        }

        var order = new AirtimeRequestDto
        {
            Product = "airtime",
            Phone = phone,
            Network = network,
            Amount = amount,
            CustomerAccount = account.AccountNumber
        };

        if (Deliver(order, amount, account))
        {
            _screen.DisplayAirtimePurchaseSuccess(amount, phone, network, account.Balance);
        }
    }

    public void BuyData(Account account)
    {
        string? phone = _input.ReadPhoneNumber();
        if (phone == null) return;

        string? network = _input.ChooseNetwork();
        if (network == null)
        {
            _screen.DisplayInvalidNetwork();
            return;
        }

        DataPlan? plan = _input.ChooseDataPlan();
        if (plan == null) return;

        if (!account.TryDebit(plan.Price, $"{plan.Name} data for {phone} ({network})", "Data"))
        {
            _outcomes.DisplayInsufficientFunds();
            return;
        }

        var order = new AirtimeRequestDto
        {
            Product = "data",
            Phone = phone,
            Network = network,
            Plan = plan.Name,
            Amount = plan.Price,
            CustomerAccount = account.AccountNumber
        };

        if (Deliver(order, plan.Price, account))
        {
            _screen.DisplayDataPurchaseSuccess(plan, phone, network, account.Balance);
        }
    }

    private bool Deliver(AirtimeRequestDto order, decimal amount, Account account)
    {
        (bool success, string message) = _httpClient
            .BuyAirtimeAsync(order)
            .GetAwaiter().GetResult();

        if (success)
        {
            return true;
        }

        // A did not deliver, so the customer must not pay: give the money back.
        account.Credit(amount, "Airtime not delivered - reversed", "Reversal");

        _screen.DisplayPurchaseFailed(message);

        return false;
    }
}
