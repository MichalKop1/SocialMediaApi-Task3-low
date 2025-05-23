using Dapper;
using Microsoft.AspNetCore.OpenApi; // Add this using directive for WithOpenApi extension method
using System.Data;
using Microsoft.OpenApi;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));


var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/users", async (IDbConnection db, string username) =>
{
	var sql = "INSERT INTO users (username) VALUES (@Username) RETURNING *;";
	var user = await db.QuerySingleAsync(sql, new { Username = username });
	return Results.Ok(user);
}).WithOpenApi();

app.MapPost("/posts", async (IDbConnection db, string title, string body, int authorId) =>
{
	var sql = "INSERT INTO posts (title, body, author_id) VALUES (@Title, @Body, @AuthorId) RETURNING *;";
	var post = await db.QuerySingleAsync(sql, new { Title = title, Body = body, AuthorId = authorId });
	return Results.Ok(post);
}).WithOpenApi();

app.MapGet("/posts", async (IDbConnection db) =>
{
	var posts = await db.QueryAsync("SELECT * FROM posts;");
	return Results.Ok(posts);
}).WithOpenApi();

app.MapPost("/follow", async (IDbConnection db, int followerId, int followedId) =>
{
	var sql = "INSERT INTO follows (follower_id, followed_id) VALUES (@FollowerId, @FollowedId);";
	await db.ExecuteAsync(sql, new { FollowerId = followerId, FollowedId = followedId });
	return Results.Ok("Followed");
}).WithOpenApi();

app.MapPost("/like", async (IDbConnection db, int userId, int postId) =>
{
	var sql = "INSERT INTO likes (user_id, post_id) VALUES (@UserId, @PostId);";
	await db.ExecuteAsync(sql, new { UserId = userId, PostId = postId });
	return Results.Ok("Liked");
}).WithOpenApi();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
