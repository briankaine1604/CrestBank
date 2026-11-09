using CrestBank;
using CrestBank.Http;
using Microsoft.Extensions.DependencyInjection;

Console.Title = "CrestBank ATM  (Bank B)";

string ownAddress = Argument("--own") ?? "http://localhost:5002/";
string peerAddress = Argument("--peer") ?? "http://localhost:5001/";
string interBankSecret = Argument("--secret") ?? "nova-crest-demo-secret";

string? Argument(string name)
{
    int index = Array.IndexOf(args, name);

    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

var services = new ServiceCollection();

services.AddSingleton(_ =>
{
    var bank = new Bank();
    BankSeeder.Seed(bank);
    return bank;
});

services.AddSingleton(_ => new Admin("admin", "password"));

services.AddSingleton<ATMConsole>();
services.AddSingleton<IInputReader>(p => p.GetRequiredService<ATMConsole>());
services.AddSingleton<IMenuScreen>(p => p.GetRequiredService<ATMConsole>());
services.AddSingleton<IAccountScreen>(p => p.GetRequiredService<ATMConsole>());
services.AddSingleton<ITransferScreen>(p => p.GetRequiredService<ATMConsole>());
services.AddSingleton<IAirtimeScreen>(p => p.GetRequiredService<ATMConsole>());
services.AddSingleton<IAccountOpeningScreen>(p => p.GetRequiredService<ATMConsole>());
services.AddSingleton<ICommonOutcomes>(p => p.GetRequiredService<ATMConsole>());

services.AddSingleton<IAdminConsole, AdminConsole>();
services.AddSingleton<IServerConsole, ServerConsole>();

services.AddSingleton<AccountOpeningService>();
services.AddSingleton<AccountService>();
services.AddSingleton<TransferService>();
services.AddSingleton<AirtimeService>();
services.AddSingleton<AdminService>();

services.AddSingleton<IBankHttpClient>(
    _ => new BankHttpClient(peerAddress, interBankSecret));

const string OwnAccountPrefix = "303";
const string BankName = "CrestBank";
const string PeerBankName = "NovaBank";

services.AddSingleton(_ => new LocalAccountNumbers(OwnAccountPrefix));

// Order matters: the first route that handles the number takes the transfer.
services.AddSingleton<ITransferRoute>(provider => new LocalTransferRoute(
    provider.GetRequiredService<Bank>(),
    provider.GetRequiredService<LocalAccountNumbers>(),
    provider.GetRequiredService<ITransferScreen>(),
    provider.GetRequiredService<ICommonOutcomes>()));

services.AddSingleton<ITransferRoute>(provider => new InterBankTransferRoute(
    provider.GetRequiredService<IBankHttpClient>(),
    provider.GetRequiredService<LocalAccountNumbers>(),
    provider.GetRequiredService<ITransferScreen>(),
    provider.GetRequiredService<ICommonOutcomes>(),
    BankName,
    PeerBankName));

services.AddSingleton<IBankHttpServer>(
    provider => new BankHttpServer(
        provider.GetRequiredService<Bank>(),
        provider.GetRequiredService<IServerConsole>(),
        ownAddress,
        interBankSecret));

services.AddSingleton<ATM>();

await using ServiceProvider provider = services.BuildServiceProvider();

_ = provider.GetRequiredService<IBankHttpServer>().Start();

ATM atm = provider.GetRequiredService<ATM>();

atm.Run();
