using AdministrationPlat.Models;
using Logic.Models;

namespace Logic.Contracts;

public interface IUserLogic
{
    OperationResult<User> Login(string username, string password);
    OperationResult<User> Register(string username, string password);

    User? GetUser(string username, string password);
    bool UsernameExists(string username);
    User CreateUser(string username, string password);
}
