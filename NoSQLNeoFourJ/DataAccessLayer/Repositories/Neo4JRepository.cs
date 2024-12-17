using Neo4j.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Neo4JRepository
{
    private readonly IDriver _driver;

    public Neo4JRepository(string uri, string user, string password)
    {
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }

    public async Task CreateUserNode(string userId, string firstName, string lastName)
    {
        var query = @"
            CREATE (u:User {UserId: $UserId, FirstName: $FirstName, LastName: $LastName})
        ";
        await ExecuteWriteQuery(query, new { UserId = userId, FirstName = firstName, LastName = lastName });
    }

    public async Task CreateFriendship(string userId1, string userId2)
    {
        var query = @"
            MATCH (u1:User {UserId: $UserId1}), (u2:User {UserId: $UserId2})
            CREATE (u1)-[:FRIEND]->(u2)
        ";
        await ExecuteWriteQuery(query, new { UserId1 = userId1, UserId2 = userId2 });
    }

    public async Task<int> GetShortestPathDistance(string userId1, string userId2)
    {
        var query = @"
            MATCH (u1:User {UserId: $UserId1}), (u2:User {UserId: $UserId2})
            RETURN length(shortestPath((u1)-[*]-(u2))) as distance
        ";
        var result = await ExecuteReadQuery(query, new { UserId1 = userId1, UserId2 = userId2 });
        return result.Single()["distance"].As<int>();
    }

    private async Task ExecuteWriteQuery(string query, object parameters)
    {
        await using var session = _driver.AsyncSession();
        await session.WriteTransactionAsync(tx => tx.RunAsync(query, parameters));
    }

    private async Task<IList<IRecord>> ExecuteReadQuery(string query, object parameters)
    {
        await using var session = _driver.AsyncSession();
        return await session.ReadTransactionAsync(tx => tx.RunAsync(query, parameters).ToListAsync());
    }
}
