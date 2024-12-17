using System.Collections.Generic;
using System;

public class Post
{
    public string Id { get; set; } // MongoDB ObjectId
    public string UserId { get; set; } // MongoDB ObjectId of the user
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Reaction> Reactions { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}