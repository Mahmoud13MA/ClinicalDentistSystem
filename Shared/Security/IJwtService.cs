namespace clinical.APIs.Shared.Security
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string name, string userType);
    }
}
