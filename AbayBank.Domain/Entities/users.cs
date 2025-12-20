using System.ComponentModel.DataAnnotations;

namespace AbayBank.Domain.Entities;

public class User : BaseEntity
{
    [Required]
    public string FirstName { get; private set; }

    [Required]
    public string LastName { get; private set; }

    [Required]
    [EmailAddress]
    public string Email { get; private set; }

    [Required]
    public string PasswordHash { get; private set; }

    public string? PhoneNumber { get; private set; }

    [Required]
    public string Role { get; private set; }

    public bool IsActive { get; private set; }

    private readonly List<Account> _accounts = new();
    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

    // Private constructor for EF Core
    private User() 
    {
        // Initialize with default values
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        Role = "User";
    }

    public User(string firstName, string lastName, string email, string passwordHash, string role = "User")
    {
        Id = Guid.NewGuid();
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Role = role ?? "User";
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string firstName, string lastName, string? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(string newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAccount(Account account)
    {
        if (account.UserId != Id)
            throw new InvalidOperationException("Account does not belong to this user");
        
        _accounts.Add(account);
    }
}