using AbayBank.Application.Interfaces;
using AbayBank.Domain.Entities;
using AbayBank.Persistence.Context;

namespace AbayBank.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AbayBankDbContext _context;

    public TransactionRepository(AbayBankDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
    }
}
