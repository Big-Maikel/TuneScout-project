using Logic.Models;
using Logic.Interfaces;
using System.Collections.Generic;

namespace Logic.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IEnumerable<User> GetAllUsers() => _userRepository.GetAll();

        public User? GetUserById(int id) => _userRepository.GetById(id);

        public void CreateUser(User user) => _userRepository.Add(user);

        public void UpdateUser(User user) => _userRepository.Update(user);

        public void DeleteUser(int id) => _userRepository.Delete(id);
    }
} 