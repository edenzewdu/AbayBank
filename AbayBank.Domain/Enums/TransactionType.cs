namespace AbayBank.Domain.Enums;

public enum TransactionType
{
    Deposit = 1,
    Withdraw = 2,
    TransferIn = 3,
    TransferOut = 4,
    AccountFrozen = 5,
    AccountUnfrozen = 6,
    AccountClosed = 7,
    Interest = 8,
    Fee = 9
}