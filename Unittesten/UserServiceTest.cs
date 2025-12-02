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
        public void GetUserById_ReturnsCorrectUser()
        {
            var user = new User { Id = 1, Name = "Frank" };
            _mockRepo.Setup(r => r.GetById(1)).Returns(user);

            var result = _service.GetUserById(1);

            Assert.NotNull(result);
            Assert.Equal("Frank", result.Name);
        }

        [Fact]
        public void CreateUser_CallsRepositoryAdd()
        {
            var newUser = new User { Id = 3, Name = "Bob" };

            _service.CreateUser(newUser);

            _mockRepo.Verify(r => r.Add(newUser), Times.Once);
        }

        [Fact]
        public void UpdateUser_CallsRepositoryUpdate()
        {
            var user = new User { Id = 1, Name = "Frank" };
            _service.UpdateUser(user);
            _mockRepo.Verify(r => r.Update(user), Times.Once);
        }

        [Fact]
        public void DeleteUser_CallsRepositoryDelete()
        {
            _service.DeleteUser(1);
            _mockRepo.Verify(r => r.Delete(1), Times.Once);
        }

    }
}
