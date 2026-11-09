# CrestBank

A console banking app in C# (.NET 10). This is Bank B. It is normally started by
NovaBank (Bank A), which passes it its address, the peer's address and the
shared secret.

## Run

Usually you don't run this directly - run NovaBank and it starts this. On its own:

    dotnet run

It defaults to port 5002 and peer http://localhost:5001/. Admin login is
`admin` / `password`.

## What it does itself

Accounts, deposits, withdrawals, and transfers to its own accounts.

## What it asks Bank A for

Airtime and data (menu 6 and 7). This bank has no telco side, so it takes the
money from the customer, calls Bank A's /airtime or /data, and reverses the debit
if Bank A refuses or does not answer.

## Files worth reading

    Presentation Layer/AirtimeService.cs   airtime and data purchases
    Presentation Layer/TransferService.cs  local and inter-bank transfers
    Http/BankHttpClient.cs                 how it calls Bank A
    bank.log                               incoming transfers and rejections

## Accounts to log into

    3030000998 / 1234   50,000
    3030000999 / 1234   500,000

## Notes

- Started by Bank A: when Bank A exits, this ATM closes too.
- This repo was split out of an older solution that held both banks, so the
  earliest commits are not here.
