using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using BCrypt.Net;


public class Program
{
    static async Task Main(string[] args)
    {
        // Ініціалізація баз даних
        var mongoClient = new MongoClient("mongodb://localhost:27017");
        var database = mongoClient.GetDatabase("SocialNetwork");
        var userRepo = new UserRepository(database);
        var postRepo = new PostRepository(database);
        var neo4JRepo = new Neo4JRepository("bolt://localhost:7687", "neo4j", "password");

        var userService = new UserService(userRepo, neo4JRepo);
        var postService = new PostService(postRepo);

        User loggedInUser = null;

        while (true)
        {
            if (loggedInUser == null)
            {
                // Головне меню
                Console.Clear();
                Console.WriteLine("=== Welcome to Social Network ===");
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Exit");
                Console.Write("Choose an option: ");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        loggedInUser = await Register(userService);
                        break;
                    case "2":
                        loggedInUser = await Login(userService);
                        break;
                    case "3":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Try again.");
                        break;
                }
            }
            else
            {
                // Меню для авторизованого користувача
                Console.Clear();
                Console.WriteLine($"=== Hello, {loggedInUser.FirstName}! ===");
                Console.WriteLine("1. View My Profile");
                Console.WriteLine("2. Search Users");
                Console.WriteLine("3. View Posts");
                Console.WriteLine("4. Add Post");
                Console.WriteLine("5. Logout");
                Console.Write("Choose an option: ");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        await ViewProfile(loggedInUser, userService, neo4JRepo);
                        break;
                    case "2":
                        await SearchUsers(loggedInUser, userService, neo4JRepo);
                        break;
                    case "3":
                        await ViewPosts(postService, loggedInUser);
                        break;
                    case "4":
                        await AddPost(postService, loggedInUser);
                        break;
                    case "5":
                        loggedInUser = null;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Try again.");
                        break;
                }
            }
        }
    }

    // Реєстрація користувача
    private static async Task<User> Register(UserService userService)
    {
        Console.Clear();
        Console.WriteLine("=== Register ===");

        Console.Write("Email: ");
        var email = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        Console.Write("First Name: ");
        var firstName = Console.ReadLine();

        Console.Write("Last Name: ");
        var lastName = Console.ReadLine();

        Console.WriteLine("Enter interests (comma separated): ");
        var interests = Console.ReadLine()?.Split(',').Select(s => s.Trim()).ToList();

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            Interests = interests ?? new List<string>()
        };

        await userService.RegisterUser(user);
        Console.WriteLine("Registration successful! You are now logged in.");
        Console.ReadKey();
        return user;
    }

    // Логін користувача
    private static async Task<User> Login(UserService userService)
    {
        Console.Clear();
        Console.WriteLine("=== Login ===");

        Console.Write("Email: ");
        var email = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        var user = userService.GetUserByEmail(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            Console.WriteLine("Invalid email or password. Try again.");
            Console.ReadKey();
            return null;
        }

        Console.WriteLine("Login successful!");
        Console.ReadKey();
        return user;
    }

    // Перегляд профілю
    private static async Task ViewProfile(User loggedInUser, UserService userService, Neo4JRepository neo4JRepo)
    {
        Console.Clear();
        Console.WriteLine("=== My Profile ===");
        Console.WriteLine($"Name: {loggedInUser.FirstName} {loggedInUser.LastName}");
        Console.WriteLine($"Email: {loggedInUser.Email}");
        Console.WriteLine("Interests: " + string.Join(", ", loggedInUser.Interests));

        Console.WriteLine("\nYour connections:");
        var connections = await neo4JRepo.GetUserConnections(loggedInUser.Id);
        foreach (var connection in connections)
        {
            Console.WriteLine($"- {connection.FirstName} {connection.LastName}");
        }

        Console.ReadKey();
    }

    // Пошук користувачів
    private static async Task SearchUsers(User loggedInUser, UserService userService, Neo4JRepository neo4JRepo)
    {
        Console.Clear();
        Console.WriteLine("=== Search Users ===");
        Console.Write("Enter name or email to search: ");
        var query = Console.ReadLine();

        var users = userService.SearchUsers(query);
        foreach (var user in users)
        {
            Console.WriteLine($"- {user.FirstName} {user.LastName} ({user.Email})");

            // Відстань до користувача
            var distance = await neo4JRepo.GetShortestPathDistance(loggedInUser.Id, user.Id);
            Console.WriteLine($"  Distance: {distance}");

            Console.WriteLine("  Actions: [1] Add Friend, [2] View Profile, [0] Skip");
            var action = Console.ReadLine();

            switch (action)
            {
                case "1":
                    await userService.AddFriend(loggedInUser.Id, user.Id);
                    Console.WriteLine("Friend added!");
                    break;
                case "2":
                    await ViewProfile(user, userService, neo4JRepo);
                    break;
                default:
                    break;
            }
        }

        Console.ReadKey();
    }

    // Перегляд постів
    private static async Task ViewPosts(PostService postService, User loggedInUser)
    {
        Console.Clear();
        Console.WriteLine("=== Posts ===");
        var posts = await postService.GetAllPosts();
        foreach (var post in posts)
        {
            Console.WriteLine($"{post.Content} (by {post.UserId})");
            Console.WriteLine("  Comments:");
            foreach (var comment in post.Comments)
            {
                Console.WriteLine($"  - {comment.Content} (by {comment.UserId})");
            }
        }

        Console.ReadKey();
    }

    // Додавання посту
    private static async Task AddPost(PostService postService, User loggedInUser)
    {
        Console.Clear();
        Console.WriteLine("=== Add Post ===");

        Console.Write("Enter content: ");
        var content = Console.ReadLine();

        var post = new Post
        {
            Id = Guid.NewGuid().ToString(),
            UserId = loggedInUser.Id,
            Content = content,
            CreatedAt = DateTime.Now
        };

        await postService.AddPost(post);
        Console.WriteLine("Post added!");
        Console.ReadKey();
    }
}

