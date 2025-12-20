using AbayBank.Domain.Entities;
using AbayBank.Domain.Interfaces;
using AbayBank.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AbayBank.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AbayBankDbContext _context;

    public TransactionRepository(AbayBankDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _context.Transactions.FindAsync(id);
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId, DateTime from, DateTime to)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId && 
                       t.Timestamp >= from && 
                       t.Timestamp <= to)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAndTypeAsync(
        Guid accountId, string transactionType, DateTime from, DateTime to)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId && 
                       t.TransactionType == transactionType &&
                       t.Timestamp >= from && 
                       t.Timestamp <= to)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdWithPagingAsync(
        Guid accountId, DateTime from, DateTime to, string? transactionType, int page, int pageSize)
    {
        var query = _context.Transactions
            .Where(t => t.AccountId == accountId && 
                       t.Timestamp >= from && 
                       t.Timestamp <= to);

        if (!string.IsNullOrEmpty(transactionType))
        {
            query = query.Where(t => t.TransactionType == transactionType);
        }

        return await query
            .OrderByDescending(t => t.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountByAccountIdAsync(Guid accountId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Transactions.Where(t => t.AccountId == accountId);
        
        if (from.HasValue)
            query = query.Where(t => t.Timestamp >= from.Value);
            
        if (to.HasValue)
            query = query.Where(t => t.Timestamp <= to.Value);
            
        return await query.CountAsync();
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        await _context.Transactions.AddRangeAsync(transactions);
    }
}