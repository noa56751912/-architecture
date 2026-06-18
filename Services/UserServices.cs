using Entities;
using Repositories;
using DTOs;
using AutoMapper;
namespace Services
{
    public class UserServices : IUserServices
    {
        private const int MinimumPasswordScore = 2;
        private readonly IUserRepository _repository;
        private readonly IPasswordServices _passwordServices;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public UserServices(IUserRepository repository, IPasswordServices passwordServices, IMapper mapper, ITokenService tokenService)
        {
            _repository = repository;
            _passwordServices = passwordServices;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<UserDTO?> GetUserById(int id)
        {
            return _mapper.Map<User?, UserDTO?>(await _repository.GetUserById(id));
        }
        public async Task<AuthResponseDTO?> Login(ExistingUserDTO existingUser)
        {
            UserDTO? userDto = _mapper.Map<User?, UserDTO?>(await _repository.Login(existingUser.Email, existingUser.Password));
            if (userDto == null)
                return null;
            string token = _tokenService.CreateToken(userDto);
            return new AuthResponseDTO(token, userDto);
        }
        public async Task<AuthResponseDTO?> Register(PostUserDTO user)
        {
            int passScore = _passwordServices.PasswordScore(user.Password);
            if (passScore < 2)
                return null;

            User userEntity = _mapper.Map<User>(user);
            UserDTO? userDto = _mapper.Map<UserDTO?>(await _repository.Register(userEntity));
            if (userDto == null)
                return null;
            string token = _tokenService.CreateToken(userDto);
            return new AuthResponseDTO(token, userDto);
        }

        public async Task<bool> Update(int id, PostUserDTO updateUser)
        {
            int passScore = _passwordServices.PasswordScore(updateUser.Password);
            if (passScore < 2)
                return false;
            User user = _mapper.Map<User>(updateUser);
            user.UserId = id;
            await _repository.Update(id, user);
            return true;
        }

        public async Task<IEnumerable<UserDTO>> GetUsers()
        {
            return _mapper.Map<IEnumerable<User>, IEnumerable<UserDTO>>(await _repository.GetUsers());
        }
        public async Task<bool> UserWithSameEmail(string email, int id = -1)
        {
            return await _repository.UserWithSameEmail(email, id);
        }
        public bool IsPasswordStrong(string password)
        {
            int passScore = _passwordServices.PasswordScore(password);
            return passScore >= MinimumPasswordScore;
        }
    }
}
