using CrestBank;
using CrestBank.Http;
using Microsoft.Extensions.DependencyInjection;

Console.Title = "CrestBank ATM  (Bank B)";

// Shared secret for the inter-bank channel; both banks must hold the same value.
// A real deployment keeps one key per peer in configuration, never in source.
const string InterBankSecret = "nova-crest-demo-secret-change-me";

const string PeerBankAddress = "http://localhost:5001/";
const string OwnAddress = "http://localhost:5002/";

var services = new ServiceCollection();

// One Bank for the life of the process: seeded once here and shared by the ATM
// and the HTTP listener, so both sides see the same accounts.
services.AddSingleton(_ =>
{
    var bank = new Bank();
    BankSeeder.Seed(bank);
    return bank;
});

services.AddSingleton(_ => new Admin("admin", "password"));

// Screens behind interfaces: the ATM never names a concrete console.
services.AddSingleton<IATMConsole, ATMConsole>();
services.AddSingleton<IAdminConsole, AdminConsole>();
services.AddSingleton<IServerConsole, ServerConsole>();

// Transport. Addresses and the shared secret are wired in one place.
services.AddSingleton<IBankHttpClient>(
    _ => new BankHttpClient(PeerBankAddress, InterBankSecret));

services.AddSingleton<IBankHttpServer>(
    provider => new BankHttpServer(
        provider.GetRequiredService<Bank>(),
        provider.GetRequiredService<IServerConsole>(),
        OwnAddress,
        InterBankSecret));

services.AddSingleton<ATM>();

await using ServiceProvider provider = services.BuildServiceProvider();

// Talks to NovaBank, and listens for NovaBank talking to us.
_ = provider.GetRequiredService<IBankHttpServer>().Start();

ATM atm = provider.GetRequiredService<ATM>();

atm.Run();
