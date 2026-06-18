using DTOs;

namespace Services
{
    public interface ITokenService
    {
        string CreateToken(UserDTO user);
    }
}
