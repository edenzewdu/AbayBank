using AbayBank.Domain.Entities;

namespace AbayBank.Application.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);
}
