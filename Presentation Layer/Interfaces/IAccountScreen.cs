using System.Collections.Generic;

namespace CrestBank
{
    public interface IAccountScreen
    {
        void DisplayAccountMenu(Account account);

        void DisplayBalance(decimal balance);

        void DisplayDepositSuccess(decimal amount, decimal newBalance);

        void DisplayWithdrawalSuccess(decimal amount, decimal newBalance);

        void DisplayNoTransactions();

        void DisplayTransactionHistory(IReadOnlyList<Transaction> transactions);

        void DisplayAccountFrozen();

        void DisplayInvalidLogin();
    }
}
