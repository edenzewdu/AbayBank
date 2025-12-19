namespace AbayBank.Domain.Exceptions;

public class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException()
        : base("Insufficient balance for withdrawal.")
    {
    }
}
