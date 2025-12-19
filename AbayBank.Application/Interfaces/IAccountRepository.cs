using AbayBank.Domain.Entities;

namespace AbayBank.Application.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
}
