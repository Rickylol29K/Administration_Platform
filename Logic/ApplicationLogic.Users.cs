using AdministrationPlat.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public User? GetUser(string username, string password) => _users.GetUser(username, password);

    public bool UsernameExists(string username) => _users.UsernameExists(username);

    public User CreateUser(string username, string password) => _users.CreateUser(username, password);
}
