namespace DTOs
{
    public record AuthResponseDTO(
        string Token,
        UserDTO User
    );
}
