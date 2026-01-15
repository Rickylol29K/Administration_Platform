using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public OperationResult<User> Login(string username, string password)
    {
        //remove spaces from either front or the end of entered strings
        string trimmedUsername;
        if (username == null)
        {
            trimmedUsername = string.Empty;
        }
        else
        {
            trimmedUsername = username.Trim();
        }

        string trimmedPassword;
        if (password == null)
        {
            trimmedPassword = string.Empty;
        }
        else
        {
            trimmedPassword = password.Trim();
        }

        //check if bpth username and password are entered
        if (string.IsNullOrEmpty(trimmedUsername) || string.IsNullOrEmpty(trimmedPassword))
        {
            return OperationResult<User>.Fail("Enter both username and password.");
        }

        User? user = _repository.GetUser(trimmedUsername, trimmedPassword);
        if (user == null)
        {
            return OperationResult<User>.Fail("Invalid username or password.");
        }

        return OperationResult<User>.Ok(user);
    }

    public OperationResult<User> Register(string username, string password, bool isAdmin)
    {
        string trimmedUsername;
        if (username == null)
        {
            trimmedUsername = string.Empty;
        }
        else
        {
            trimmedUsername = username.Trim();
        }

        string trimmedPassword;
        if (password == null)
        {
            trimmedPassword = string.Empty;
        }
        else
        {
            trimmedPassword = password.Trim();
        }

        if (string.IsNullOrEmpty(trimmedUsername) || string.IsNullOrEmpty(trimmedPassword))
        {
            return OperationResult<User>.Fail("Choose a username and password.");
        }

        if (_repository.UsernameExists(trimmedUsername))
        {
            return OperationResult<User>.Fail("Username already exists.");
        }

        User user = _repository.CreateUser(trimmedUsername, trimmedPassword, isAdmin);
        return OperationResult<User>.Ok(user);
    }

    public User? GetUser(string username, string password)
    {
        return _repository.GetUser(username, password);
    }

    public User? GetUserById(int id)
    {
        return _repository.GetUserById(id);
    }

    public bool UsernameExists(string username)
    {
        return _repository.UsernameExists(username);
    }

    public User CreateUser(string username, string password, bool isAdmin)
    {
        return _repository.CreateUser(username, password, isAdmin);
    }

    public List<User> GetTeachers()
    {
        return _repository.GetTeachers();
    }
}
