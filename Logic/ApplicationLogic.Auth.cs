using AdministrationPlat.Models;
using Logic.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public OperationResult<User> Login(string username, string password)
    {
        return _users.Login(username, password);
    }

    public OperationResult<User> Register(string username, string password)
    {
        return _users.Register(username, password);
    }
}
