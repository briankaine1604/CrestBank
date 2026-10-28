using CrestBank;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrestBank;
public static class BankSeeder
{
    public static void Seed(Bank bank)
    {
        Customer james = new("James", "Benin");
        Account account = new("3030000998", james, 0m, "1234");
        account.Credit(50000m, "Opening deposit", "Deposit");
        bank.AddAccount(account);

        Customer movi = new("Movi", "Bayelsa");
        Account account2 = new("3030000999", movi, 0m, "1234");
        account2.Credit(500000m, "Opening deposit", "Deposit");
        bank.AddAccount(account2);
    }
}
