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
var httpClient = new BankHttpClient("http://localhost:5001/");
var httpServer = new BankHttpServer(bank, serverConsole, "http://localhost:5002/");
_ = httpServer.Start();

ATM atm = new(bank, aTMConsole, adminConsole, admin, httpClient);
atm.Run();
