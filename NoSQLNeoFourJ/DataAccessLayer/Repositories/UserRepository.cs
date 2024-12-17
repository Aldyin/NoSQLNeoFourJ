using MongoDB.Driver;

public class UserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("Users");
    }

    public void AddUser(User user)
    {
        _users.InsertOne(user);
    }

    public User GetUserById(string id)
    {
        return _users.Find(u => u.Id == id).FirstOrDefault();
    }

    public User GetUserByEmail(string email)
    {
        return _users.Find(u => u.Email == email).FirstOrDefault();
    }

    public void DeleteUser(string id)
    {
        _users.DeleteOne(u => u.Id == id);
    }
}