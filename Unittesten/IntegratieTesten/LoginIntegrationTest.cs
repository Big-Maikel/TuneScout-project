using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Logic.Services;
using Logic.Models;

namespace Unittesten.IntegratieTesten
{
    public class LoginTestDbContext : DbContext
    {
        public LoginTestDbContext(DbContextOptions<LoginTestDbContext> options)
            : base(options)
        {
        }

        public DbSet<LoginUser> Users => Set<LoginUser>();
    }

    public class LoginUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool NoExplicit { get; set; }
    }

    public class UserRepository
    {
        private readonly LoginTestDbContext _context;

        public UserRepository(LoginTestDbContext context)
        {
            _context = context;
        }

        public void AddUser(LoginUser user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public LoginUser? GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public bool EmailExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public int GetUserCount()
        {
            return _context.Users.Count();
        }
    }

    public class UserService
    {
        private readonly UserRepository _repository;
        private readonly PasswordHasher<LoginUser> _passwordHasher;

        public UserService(UserRepository repository)
        {
            _repository = repository;
            _passwordHasher = new PasswordHasher<LoginUser>();
        }

        public void RegisterUser(string name, string email, string password)
        {
            var user = new LoginUser
            {
                Name = name,
                Email = email,
                Password = string.Empty
            };

            user.Password = _passwordHasher.HashPassword(user, password);
            _repository.AddUser(user);
        }

        public bool Login(string email, string password)
        {
            var user = _repository.GetUserByEmail(email);
            if (user == null) return false;

            var verification = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            return verification == PasswordVerificationResult.Success || 
                   verification == PasswordVerificationResult.SuccessRehashNeeded;
        }

        public bool EmailExists(string email)
        {
            return _repository.EmailExists(email);
        }

        public int GetUserCount()
        {
            return _repository.GetUserCount();
        }
    }

    public class LoginIntegrationTest
    {
        [Fact]
        public void WhenUserRegisters_ThenUserIsCreatedInDatabase()
        {
            var options = new DbContextOptionsBuilder<LoginTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new LoginTestDbContext(options);
            var repository = new UserRepository(context);
            var service = new UserService(repository);

            service.RegisterUser("Test User", "test@example.com", "SecurePassword123!");

            var userCount = service.GetUserCount();
            Assert.Equal(1, userCount);

            var user = repository.GetUserByEmail("test@example.com");
            Assert.NotNull(user);
            Assert.Equal("Test User", user.Name);
            Assert.Equal("test@example.com", user.Email);
        }

        [Fact]
        public void WhenUserRegistersWithExistingEmail_ThenDuplicateIsDetected()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<LoginTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new LoginTestDbContext(options);
            var repository = new UserRepository(context);
            var service = new UserService(repository);

            service.RegisterUser("First User", "duplicate@example.com", "Password1");

            var isDuplicate = service.EmailExists("duplicate@example.com");
            Assert.True(isDuplicate);
            Assert.Equal(1, service.GetUserCount());
        }

        [Fact]
        public void WhenUserLogsInWithCorrectPassword_ThenAuthenticationSucceeds()
        {
            var options = new DbContextOptionsBuilder<LoginTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new LoginTestDbContext(options);
            var repository = new UserRepository(context);
            var service = new UserService(repository);

            service.RegisterUser("Login User", "login@example.com", "MyPassword123!");

            var loginSuccess = service.Login("login@example.com", "MyPassword123!");

            Assert.True(loginSuccess);
        }

        [Fact]
        public void WhenUserLogsInWithWrongPassword_ThenAuthenticationFails()
        {
            var options = new DbContextOptionsBuilder<LoginTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new LoginTestDbContext(options);
            var repository = new UserRepository(context);
            var service = new UserService(repository);

            service.RegisterUser("Secure User", "secure@example.com", "CorrectPassword");

            var loginSuccess = service.Login("secure@example.com", "WrongPassword");

            Assert.False(loginSuccess);
        }

        [Fact]
        public void WhenUserLogsInWithNonExistentEmail_ThenAuthenticationFails()
        {
            var options = new DbContextOptionsBuilder<LoginTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new LoginTestDbContext(options);
            var repository = new UserRepository(context);
            var service = new UserService(repository);

            var loginSuccess = service.Login("nonexistent@example.com", "AnyPassword");

            Assert.False(loginSuccess);
        }

        [Fact]
        public void WhenMultipleUsersRegister_ThenAllAreStoredCorrectly()
        {
            var options = new DbContextOptionsBuilder<LoginTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new LoginTestDbContext(options);
            var repository = new UserRepository(context);
            var service = new UserService(repository);

            service.RegisterUser("User One", "user1@example.com", "Pass1");
            service.RegisterUser("User Two", "user2@example.com", "Pass2");
            service.RegisterUser("User Three", "user3@example.com", "Pass3");

            Assert.Equal(3, service.GetUserCount());
            Assert.True(service.EmailExists("user1@example.com"));
            Assert.True(service.EmailExists("user2@example.com"));
            Assert.True(service.EmailExists("user3@example.com"));
        }

        [Fact]
        public void WhenUserRegistersAndLogsIn_ThenFullFlowWorks()
        {
            var options = new DbContextOptionsBuilder<LoginTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new LoginTestDbContext(options);
            var repository = new UserRepository(context);
            var service = new UserService(repository);

            service.RegisterUser("Complete Flow User", "flow@example.com", "CompletePassword123!");

            var loginSuccess = service.Login("flow@example.com", "CompletePassword123!");

            Assert.True(loginSuccess);
            var user = repository.GetUserByEmail("flow@example.com");
            Assert.NotNull(user);
            Assert.Equal("Complete Flow User", user.Name);
        }

        [Fact]
        public void WhenUserIsCreatedWithNoExplicitPreference_ThenItIsStoredCorrectly()
        {
            var options = new DbContextOptionsBuilder<LoginTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new LoginTestDbContext(options);
            var repository = new UserRepository(context);

            var user = new LoginUser
            {
                Name = "Family Friendly User",
                Email = "family@example.com",
                Password = "HashedPassword",
                NoExplicit = true
            };

            repository.AddUser(user);

            var createdUser = repository.GetUserByEmail("family@example.com");
            Assert.NotNull(createdUser);
            Assert.True(createdUser.NoExplicit);
        }
    }
}
