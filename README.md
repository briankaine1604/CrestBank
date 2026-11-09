# CrestBank

A console banking application (.NET 10). One of a pair of banks — CrestBank is
"Bank B" — that each run as their own process and move money between each
other over HTTP.

## Layers

    BLL/                  Customers, accounts, transactions, seeding, transfers
    BLL/Interfaces/       Contracts the business layer exposes
    Http/                 Inter-bank transfer server and client, request signing
    Http/Interfaces/      Contracts for the server and client
    Presentation Layer/   ATM console, ATM session, admin console, services, DI wiring

## Run

    dotnet run

or open `CrestBank.slnx` in Visual Studio and press F5. Admin login is
`admin` / `password`.

## Inter-bank contract

CrestBank is one half of a two-bank system. Its peer, NovaBank, lives in its own
repository. Both processes must be running for a cross-bank transfer to work.

| Setting            | CrestBank       | NovaBank (peer) |
| ------------------ | --------------- | --------------- |
| HTTP listener      | localhost:5002  | localhost:5001  |
| Account prefix     | `303`           | `000`           |
| Shared secret      | `nova-crest-demo-secret` | same    |

These values are set in `Presentation Layer/Program.cs`. Changing a port, the
account prefix, the peer address, or the shared secret means changing it in the
peer repository too — there is no shared package holding them.

Requests to `/transfers` are signed with the shared secret and a timestamp;
unsigned, stale, or wrongly signed requests are rejected by both sides.

## History

This repository was split out of the `ConsoleBank` solution, which held both
banks. Its commit history was extracted from that repository with
`git subtree split`, so commits that touched only the other bank are not here.
