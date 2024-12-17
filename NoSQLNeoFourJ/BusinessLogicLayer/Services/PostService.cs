using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PostService
{
    private readonly IMongoCollection<Post> _postCollection;

    public PostService(IMongoDatabase database)
    {
        _postCollection = database.GetCollection<Post>("Posts");
    }

    // Додати новий пост
    public async Task AddPost(Post post)
    {
        if (post == null) throw new ArgumentNullException(nameof(post));
        await _postCollection.InsertOneAsync(post);
    }

    // Отримати всі пости
    public async Task<List<Post>> GetAllPosts()
    {
        return await _postCollection.Find(_ => true).SortByDescending(post => post.CreatedAt).ToListAsync();
    }

    // Отримати пости користувача
    public async Task<List<Post>> GetPostsByUserId(string userId)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
        return await _postCollection.Find(post => post.UserId == userId).SortByDescending(post => post.CreatedAt).ToListAsync();
    }

    // Додати коментар до посту
    public async Task AddComment(string postId, Comment comment)
    {
        if (string.IsNullOrEmpty(postId)) throw new ArgumentNullException(nameof(postId));
        if (comment == null) throw new ArgumentNullException(nameof(comment));

        var update = Builders<Post>.Update.Push(p => p.Comments, comment);
        await _postCollection.UpdateOneAsync(post => post.Id == postId, update);
    }

    // Додати реакцію до посту
    public async Task AddReaction(string postId, Reaction reaction)
    {
        if (string.IsNullOrEmpty(postId)) throw new ArgumentNullException(nameof(postId));
        if (reaction == null) throw new ArgumentNullException(nameof(reaction));

        var update = Builders<Post>.Update.Push(p => p.Reactions, reaction);
        await _postCollection.UpdateOneAsync(post => post.Id == postId, update);
    }

    // Видалити пост
    public async Task DeletePost(string postId)
    {
        if (string.IsNullOrEmpty(postId)) throw new ArgumentNullException(nameof(postId));
        await _postCollection.DeleteOneAsync(post => post.Id == postId);
    }
}


