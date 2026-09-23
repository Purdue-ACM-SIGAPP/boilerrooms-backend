using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SimpleWebAppReact.Entities;
using SimpleWebAppReact.Services;

namespace SimpleWebAppReact.Controllers;

/// <summary>
/// Defines endpoints for operations relating the RoommatePost table
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RoommateController : ControllerBase
{
    private readonly IMongoCollection<RoommatePost> _roommatePosts;

    public RoommateController(MongoDbService mongoDbService)
    {
        _roommatePosts = mongoDbService.Database.GetCollection<RoommatePost>("roommatePost");
    }

    /// <summary>
    /// gets roommate posts, optionally filtered by the user who made them
    /// </summary>
    [HttpGet]
    public async Task<IEnumerable<RoommatePost>> Get([FromQuery] string? userId = null)
    {
        var filter = string.IsNullOrEmpty(userId)
            ? Builders<RoommatePost>.Filter.Empty
            : Builders<RoommatePost>.Filter.Eq(p => p.UserId, userId);

        return await _roommatePosts.Find(filter).ToListAsync();
    }

    /// <summary>
    /// gets a specific roommate post by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoommatePost>> GetById(string id)
    {
        var post = MongoDbService.IsValidId(id)
            ? await _roommatePosts.Find(p => p.Id == id).FirstOrDefaultAsync()
            : null;
        return post is null ? NotFound() : Ok(post);
    }

    /// <summary>
    /// creates a roommate post
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Post(RoommatePost post)
    {
        if (string.IsNullOrWhiteSpace(post.UserId))
        {
            return BadRequest("A userId is required.");
        }

        post.Id = null;
        await _roommatePosts.InsertOneAsync(post);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
    }
}
