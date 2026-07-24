namespace ExpenSR.Helpers
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(
            Guid id,
            string email,
            string role,
            Guid companyId);
    }
}