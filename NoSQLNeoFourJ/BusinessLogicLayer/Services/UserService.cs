using System.Threading.Tasks;

public class UserService
{
    private readonly UserRepository _userRepo;
    private readonly Neo4JRepository _neo4JRepo;

    public UserService(UserRepository userRepo, Neo4JRepository neo4JRepo)
    {
        _userRepo = userRepo;
        _neo4JRepo = neo4JRepo;
    }

    public async Task RegisterUser(User user)
    {
        _userRepo.AddUser(user);
        await _neo4JRepo.CreateUserNode(user.Id, user.FirstName, user.LastName);
    }

    public async Task AddFriend(string userId1, string userId2)
    {
        await _neo4JRepo.CreateFriendship(userId1, userId2);
    }
}   