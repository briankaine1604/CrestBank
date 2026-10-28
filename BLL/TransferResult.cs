using System;
using System.Collections.Generic;
using System.Text;

namespace CrestBank;

public enum TransferResult
{
    Success,
    AmountInvalid,
    SameAccount,
    RecipientInvalid,
    InsufficientFunds
}
