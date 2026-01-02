using AdministrationPlat.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public User? GetUser(string username, string password) => _users.GetUser(username, password);

    public User? GetUserById(int id) => _users.GetUserById(id);

    public bool UsernameExists(string username) => _users.UsernameExists(username);

    public User CreateUser(string username, string password, bool isAdmin) => _users.CreateUser(username, password, isAdmin);

    public List<User> GetTeachers() => _users.GetTeachers();
}
