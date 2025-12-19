using AbayBank.Application.Interfaces;
using AbayBank.Domain.Entities;
using AbayBank.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AbayBank.Persistence.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AbayBankDbContext _context;

    public AccountRepository(AbayBankDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(x => x.AccountNumber == accountNumber);
    }

    public async Task AddAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }
}
