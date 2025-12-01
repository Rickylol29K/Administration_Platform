using AdministrationPlat.Models;

namespace Logic;

public partial class ApplicationLogic
{
    public User? GetUser(string username, string password) => _repository.GetUser(username, password);

    public bool UsernameExists(string username) => _repository.UsernameExists(username);

    public User CreateUser(string username, string password) => _repository.CreateUser(username, password);
}
