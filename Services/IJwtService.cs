namespace clinical.APIs.Services
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string name, string userType);
    }
}
