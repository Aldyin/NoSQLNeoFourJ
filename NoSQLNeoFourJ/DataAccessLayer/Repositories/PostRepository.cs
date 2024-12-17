using MongoDB.Driver;
using NoSQLNeoFourJ.DataAccessLayer.Models;
using System.Collections.Generic;

public class PostRepository
{
    private readonly IMongoCollection<Post> _posts;

    public PostRepository(IMongoDatabase database)
    {
        _posts = database.GetCollection<Post>("Posts");
    }

    public void AddPost(Post post)
    {
        _posts.InsertOne(post);
    }

    public List<Post> GetPostsByUserId(string userId)
    {
        return _posts.Find(p => p.UserId == userId).SortByDescending(p => p.CreatedAt).ToList();
    }

    public List<Post> GetAllPosts()
    {
        return _posts.Find(_ => true).SortByDescending(p => p.CreatedAt).ToList();
    }

    public void AddComment(string postId, Comment comment)
    {
        var update = Builders<Post>.Update.Push(p => p.Comments, comment);
        _posts.UpdateOne(p => p.Id == postId, update);
    }

    public void AddReaction(string postId, Reaction reaction)
    {
        var update = Builders<Post>.Update.Push(p => p.Reactions, reaction);
        _posts.UpdateOne(p => p.Id == postId, update);
    }
}

