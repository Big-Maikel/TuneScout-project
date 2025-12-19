using Xunit;
using Moq;
using Logic.Services;
using Logic.Interfaces;
using Logic.Models;
using System.Collections.Generic;
using System.Linq;

namespace Unittesten
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _service = new UserService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllUsers_ReturnsAllUsers()
        {
            var users = new List<User>
            {
              new User { Id = 1, Name = "Frank" },
              new User { Id = 2, Name = "Jaap" }
            };

            _mockRepo.Setup(r => r.GetAll()).Returns(users);

            var result = _service.GetAllUsers();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, u => u.Name == "Frank");
            Assert.Contains(result, u => u.Name == "Jaap");
        }

        [Fact]
        public void GetAllUsers_ReturnsEmptyList_WhenNoUsers()
        {
            _mockRepo.Setup(r => r.GetAll()).Returns(new List<User>());

            var result = _service.GetAllUsers();

            Assert.Empty(result);
        }

        [Fact]
        public void GetUserById_ReturnsCorrectUser()
        {
            var user = new User { Id = 1, Name = "Frank" };
            _mockRepo.Setup(r => r.GetById(1)).Returns(user);

            var result = _service.GetUserById(1);

            Assert.NotNull(result);
            Assert.Equal("Frank", result.Name);
        }

        [Fact]
        public void GetUserById_ReturnsNull_WhenUserNotFound()
        {
            _mockRepo.Setup(r => r.GetById(999)).Returns((User?)null);

            var result = _service.GetUserById(999);

            Assert.Null(result);
        }

        [Fact]
        public void GetUserById_WithDifferentIds_CallsRepositoryWithCorrectId()
        {
            _mockRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns((User?)null);

            _service.GetUserById(42);

            _mockRepo.Verify(r => r.GetById(42), Times.Once);
        }

        [Fact]
        public void CreateUser_CallsRepositoryAdd()
        {
            var newUser = new User { Id = 3, Name = "Bob" };

            _service.CreateUser(newUser);

            _mockRepo.Verify(r => r.Add(newUser), Times.Once);
        }

        [Fact]
        public void CreateUser_WithCompleteUser_CallsRepositoryAdd()
        {
            var newUser = new User 
            { 
                Id = 5, 
                Name = "Alice", 
                Email = "alice@example.com",
                Password = "hashedpassword123",
                NoExplicit = true
            };

            _service.CreateUser(newUser);

            _mockRepo.Verify(r => r.Add(newUser), Times.Once);
        }

        [Fact]
        public void CreateUser_WithNullUser_CallsRepositoryAdd()
        {
            User? nullUser = null;

            _service.CreateUser(nullUser!);

            _mockRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public void UpdateUser_CallsRepositoryUpdate()
        {
            var user = new User { Id = 1, Name = "Frank" };
            
            _service.UpdateUser(user);
            
            _mockRepo.Verify(r => r.Update(user), Times.Once);
        }

        [Fact]
        public void UpdateUser_WithModifiedUser_CallsRepositoryUpdate()
        {
            var user = new User 
            { 
                Id = 1, 
                Name = "Frank Updated",
                Email = "frank.updated@example.com",
                NoExplicit = false
            };
            
            _service.UpdateUser(user);
            
            _mockRepo.Verify(r => r.Update(user), Times.Once);
        }

        [Fact]
        public void UpdateUser_WithNullUser_CallsRepositoryUpdate()
        {
            User? nullUser = null;

            _service.UpdateUser(nullUser!);

            _mockRepo.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public void DeleteUser_CallsRepositoryDelete()
        {
            _service.DeleteUser(1);
            
            _mockRepo.Verify(r => r.Delete(1), Times.Once);
        }

        [Fact]
        public void DeleteUser_WithDifferentIds_CallsRepositoryWithCorrectId()
        {
            _service.DeleteUser(123);
            
            _mockRepo.Verify(r => r.Delete(123), Times.Once);
        }

        [Fact]
        public void DeleteUser_WithZeroId_CallsRepositoryDelete()
        {
            _service.DeleteUser(0);
            
            _mockRepo.Verify(r => r.Delete(0), Times.Once);
        }

        [Fact]
        public void DeleteUser_WithNegativeId_CallsRepositoryDelete()
        {
            _service.DeleteUser(-1);
            
            _mockRepo.Verify(r => r.Delete(-1), Times.Once);
        }

        [Fact]
        public void GetAllUsers_WithMultipleUsers_ReturnsCorrectCount()
        {
            var users = new List<User>
            {
                new User { Id = 1, Name = "User1", Email = "user1@example.com" },
                new User { Id = 2, Name = "User2", Email = "user2@example.com" },
                new User { Id = 3, Name = "User3", Email = "user3@example.com" },
                new User { Id = 4, Name = "User4", Email = "user4@example.com" }
            };

            _mockRepo.Setup(r => r.GetAll()).Returns(users);

            var result = _service.GetAllUsers();

            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void GetAllUsers_WithUsersHavingPreferences_ReturnsCorrectly()
        {
            var users = new List<User>
            {
                new User 
                { 
                    Id = 1, 
                    Name = "User1", 
                    NoExplicit = true,
                    Preferences = new List<Preference>()
                },
                new User 
                { 
                    Id = 2, 
                    Name = "User2", 
                    NoExplicit = false,
                    Preferences = new List<Preference>()
                }
            };

            _mockRepo.Setup(r => r.GetAll()).Returns(users);

            var result = _service.GetAllUsers();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, u => u.NoExplicit == true);
            Assert.Contains(result, u => u.NoExplicit == false);
        }

        [Fact]
        public void GetUserById_ReturnsUserWithEmail()
        {
            var user = new User 
            { 
                Id = 1, 
                Name = "Frank", 
                Email = "frank@example.com" 
            };
            _mockRepo.Setup(r => r.GetById(1)).Returns(user);

            var result = _service.GetUserById(1);

            Assert.NotNull(result);
            Assert.Equal("frank@example.com", result.Email);
        }

        [Fact]
        public void CreateUser_MultipleUsers_CallsRepositoryMultipleTimes()
        {
            var user1 = new User { Id = 1, Name = "User1" };
            var user2 = new User { Id = 2, Name = "User2" };

            _service.CreateUser(user1);
            _service.CreateUser(user2);

            _mockRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Exactly(2));
        }

        [Fact]
        public void UpdateUser_SameUserMultipleTimes_CallsRepositoryEachTime()
        {
            var user = new User { Id = 1, Name = "Frank" };

            _service.UpdateUser(user);
            user.Name = "Frank Updated";
            _service.UpdateUser(user);

            _mockRepo.Verify(r => r.Update(It.IsAny<User>()), Times.Exactly(2));
        }

        [Fact]
        public void DeleteUser_MultipleUsers_CallsRepositoryMultipleTimes()
        {
            _service.DeleteUser(1);
            _service.DeleteUser(2);
            _service.DeleteUser(3);

            _mockRepo.Verify(r => r.Delete(It.IsAny<int>()), Times.Exactly(3));
        }

        [Fact]
        public void GetAllUsers_CallsRepository_OnlyOnce()
        {
            _mockRepo.Setup(r => r.GetAll()).Returns(new List<User>());

            _service.GetAllUsers();

            _mockRepo.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public void GetUserById_CallsRepository_OnlyOnce()
        {
            _mockRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns((User?)null);

            _service.GetUserById(1);

            _mockRepo.Verify(r => r.GetById(It.IsAny<int>()), Times.Once);
        }
    }
}
