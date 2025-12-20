using AbayBank.Domain.Interfaces;
using AbayBank.Persistence.Context;
using AbayBank.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace AbayBank.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AbayBankDbContext _context;
    private IAccountRepository? _accountRepository;
    private ITransactionRepository? _transactionRepository;
    private IUserRepository? _userRepository;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AbayBankDbContext context)
    {
        _context = context;
    }

    public IAccountRepository Accounts => 
        _accountRepository ??= new AccountRepository(_context);

    public ITransactionRepository Transactions => 
        _transactionRepository ??= new TransactionRepository(_context);

    public IUserRepository Users => 
        _userRepository ??= new UserRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}