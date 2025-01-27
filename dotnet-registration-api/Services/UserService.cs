using dotnet_registration_api.Data;
using dotnet_registration_api.Data.Entities;
using dotnet_registration_api.Data.Models;
using dotnet_registration_api.Data.Repositories;
using dotnet_registration_api.Helpers;
using Mapster;
using Microsoft.Win32;
using System.Linq;
using System.Net.Mail;

namespace dotnet_registration_api.Services
{
    public class UserService : HashHelper
    {
        private readonly UserRepository _userRepository;


        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<User>> GetAll()
        {
            var users = await _userRepository.GetAllUsers();

            return users;
        }
        public async Task<User> GetById(int id)
        {
            User user = new User();
            user = await _userRepository.GetUserById(id);

            if (user != null)
            {
                return user;
            }
            else
            {
               throw new NotFoundException("Id does not exist!");
            }
        }
        public async Task<User> Login(LoginRequest login)
        {
            User user = new User();

            //here is problem
            //we must send a hashPassword for login, not plaintext Password because GetUserByUsernameAndPassword will return empty user.

            string password = login.Password.ToString();
            string hashedPassword = HashPassword(password);

            user = await _userRepository.GetUserByUsernameAndPassword(login.Username, hashedPassword);

            if (user != null)
            {
                return user;
            }
            else
            {
                throw new NotFoundException("User not found!");
            }
        }
        public async Task<User> Register(RegisterRequest register)
        {
            //Id
            //FirstName
            //LastName
            //Username
            //PasswordHash

            User user = new User();

            bool existingUser = true; //we suppose he exist...

            existingUser = await CheckUsername(register.Username, user);

            if (existingUser) {
                throw new NotFoundException("User already exist!");
            }   

            string password = register.Password.ToString();
            string hashedPassword = HashPassword(password);

            user.FirstName = register.FirstName.ToString();
            user.LastName = register.LastName.ToString();
            user.Username = register.Username.ToString();
            user.PasswordHash = hashedPassword;

            //suppose that he dont have id before registraction.... its still anonyumus for us

            user = await _userRepository.CreateUser(user);

            return user;
        }
        public async Task<User> Update(int id, UpdateRequest updateRequest)
        {
            //FirstName
            //LastName
            //Username
            //OldPassword
            //NewPassowrd

            if (updateRequest.OldPassword == updateRequest.NewPassword)
            {
                throw new NotFoundException("Old passowrd cannot be same as New Password!");
            }
            
            User user = new User();

            string password = updateRequest.NewPassword.ToString();
            string hashedPassword = HashPassword(password);

            user.FirstName = updateRequest.FirstName;
            user.LastName = updateRequest.LastName;
            user.Username = updateRequest.Username;
            user.PasswordHash  = hashedPassword;
            //user.OldPassword --- we need only for history pin for example if we need to se rule - new password cannot be same as previus 3, or 5....
            

            return await _userRepository.UpdateUser(user);
        }
        public async Task Delete(int id)
        {
            await _userRepository.DeleteUser(id);
        }

        public async Task<bool> CheckUsername(string newUser, User user)
        {
            bool existingUser = true;

            var users = await _userRepository.GetAllUsers();

            if (users != null && users.Any())
            {
                user = users.Where(x => x.Username == newUser).FirstOrDefault();
                if (user != null)
                {
                    existingUser = true; //does not exist
                }
                else
                {
                    existingUser = false; //does not exist
                }

            }
            else
            {
                existingUser = false;
            }

            return existingUser;
        }
        public async Task<bool> HasPassowd(string newUser, User user)
        {
            bool existingUser = true;

            var users = await _userRepository.GetAllUsers();

            if (users != null && users.Any())
            {
                user = users.Where(x => x.Username == newUser).FirstOrDefault();
                if (user != null)
                {
                    existingUser = true; //does not exist
                }
                else
                {
                    existingUser = false; //does not exist
                }

            }
            else
            {
                existingUser = false;
            }

            return existingUser;
        }
    }
}
