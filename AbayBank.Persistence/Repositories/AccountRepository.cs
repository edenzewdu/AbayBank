using AbayBank.Domain.Entities;
using AbayBank.Domain.Interfaces;
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
    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        return await _context.Accounts.ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string accountNumber)
    {
        return await _context.Accounts
            .AnyAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Account account)
    {
        _context.Accounts.Remove(account);
        await Task.CompletedTask;
    }
}