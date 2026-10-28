using System.Globalization;
using System.Linq;
using CrestBank;
using CrestBank.Http;


Console.Title = "CrestBank ATM  (Bank B)";

var bank = new Bank();
ATMConsole aTMConsole = new();
AdminConsole adminConsole = new();
ServerConsole serverConsole = new();

Admin admin = new("admin", "password");
BankSeeder.Seed(bank);

// Talks to NovaBank, and listens for NovaBank talking to us.
// Shared secret for the inter-bank channel; both banks must hold the same value.
// A real deployment keeps one key per peer in configuration, never in source.
const string InterBankSecret = "nova-crest-demo-secret-change-me";

var httpClient = new BankHttpClient("http://localhost:5001/", InterBankSecret);
var httpServer = new BankHttpServer(bank, serverConsole, "http://localhost:5002/", InterBankSecret);
_ = httpServer.Start();

ATM atm = new(bank, aTMConsole, adminConsole, admin, httpClient);
atm.Run();
