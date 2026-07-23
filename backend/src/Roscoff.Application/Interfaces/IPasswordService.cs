namespace Roscoff.Application.Interfaces;

public interface IPasswordService
{
    bool IsPasswordValid(string password);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}