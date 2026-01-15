using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using DataAccess.Contexts;
using DataAccess.Repositories;
using Logic.Services;
using Logic.Models;

namespace Unittesten.IntegratieTesten
{
    public class LoginIntegrationTest
    {
        [Fact]
        public void WhenUserRegisters_ThenUserIsCreatedInDatabase()
        {
            var options = new DbContextOptionsBuilder<TuneScoutContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TuneScoutContext(options);
            var repository = new UserRepository(context);
            var service = new UserService(repository); 

            var newUser = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "HashedPassword123"
            };

            repository.Add(newUser);
            context.SaveChanges();

            var users = repository.GetAll();
            Assert.Single(users);

            var retrievedUser = repository.GetById(newUser.Id);
            Assert.NotNull(retrievedUser);
            Assert.Equal("Test User", retrievedUser.Name);
            Assert.Equal("test@example.com", retrievedUser.Email);
        }

        [Fact]
        public void WhenUserLogsIn_ThenUserServiceValidatesCorrectly()
        {
            var options = new DbContextOptionsBuilder<TuneScoutContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TuneScoutContext(options);
            var repository = new UserRepository(context);
            var service = new UserService(repository);

            var user = new User
            {
                Id= 1,
                Name = "Login User",
                Email = "login@example.com",
                Password = "HashedPassword"
            };

            repository.Add(user);
            context.SaveChanges();

            var retrievedUser = service.GetUserById(1);

            Assert.NotNull(retrievedUser);
            Assert.Equal("Login User", retrievedUser.Name);
        }

        [Fact]
        public void WhenMultipleUsersRegister_ThenAllAreStoredCorrectly()
        {
            var options = new DbContextOptionsBuilder<TuneScoutContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TuneScoutContext(options);
            var repository = new UserRepository(context);

            var user1 = new User { Name = "User One", Email = "user1@example.com", Password = "Pass1" };
            var user2 = new User { Name = "User Two", Email = "user2@example.com", Password = "Pass2" };
            var user3 = new User { Name = "User Three", Email = "user3@example.com", Password = "Pass3" };

            repository.Add(user1);
            repository.Add(user2);
            repository.Add(user3);
            context.SaveChanges();

            var allUsers = repository.GetAll();
            Assert.Equal(3, allUsers.Count());
        }

        [Fact]
        public void WhenUserHasNoExplicitPreference_ThenItIsStoredCorrectly()
        {
            var options = new DbContextOptionsBuilder<TuneScoutContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new TuneScoutContext(options);
            var repository = new UserRepository(context);

            var user = new User
            {
                Name = "Family Friendly User",
                Email = "family@example.com",
                Password = "HashedPassword",
                NoExplicit = true
            };

            repository.Add(user);
            context.SaveChanges();

            var createdUser = repository.GetById(user.Id);
            Assert.NotNull(createdUser);
            Assert.True(createdUser.NoExplicit);
        }
    }
}
