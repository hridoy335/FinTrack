using FinTrackCore.Application.Interfaces;

namespace FinTrackCore.Infrastructure.Services;

public sealed class PasswordService : IPasswordService
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
