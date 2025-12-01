using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public OperationResult<User> Login(string username, string password)
    {
        var trimmedUsername = username?.Trim() ?? string.Empty;
        var trimmedPassword = password?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedUsername) || string.IsNullOrEmpty(trimmedPassword))
        {
            return OperationResult<User>.Fail("Enter both username and password.");
        }

        var user = _repository.GetUser(trimmedUsername, trimmedPassword);
        return user != null
            ? OperationResult<User>.Ok(user)
            : OperationResult<User>.Fail("Invalid username or password.");
    }

    public OperationResult<User> Register(string username, string password)
    {
        var trimmedUsername = username?.Trim() ?? string.Empty;
        var trimmedPassword = password?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedUsername) || string.IsNullOrEmpty(trimmedPassword))
        {
            return OperationResult<User>.Fail("Choose a username and password.");
        }

        if (_repository.UsernameExists(trimmedUsername))
        {
            return OperationResult<User>.Fail("Username already exists.");
        }

        var user = _repository.CreateUser(trimmedUsername, trimmedPassword);
        return OperationResult<User>.Ok(user);
    }
}
