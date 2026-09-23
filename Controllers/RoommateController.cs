using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SimpleWebAppReact.Entities;
using SimpleWebAppReact.Services;

namespace SimpleWebAppReact.Controllers;

/// <summary>
/// Defines endpoints for operations relating the RoommateBio table (issue #87)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RoommateController : ControllerBase
{
    private readonly IMongoCollection<RoommateBio> _bios;

    public RoommateController(MongoDbService mongoDbService)
    {
        _bios = mongoDbService.Database.GetCollection<RoommateBio>("roommateBio");
    }

    /// <summary>
    /// gets roommate bios, so users can browse others looking for a roommate.
    /// optionally filtered to one user's bio
    /// </summary>
    [HttpGet]
    public async Task<IEnumerable<RoommateBio>> Get([FromQuery] string? userId = null)
    {
        var filter = string.IsNullOrEmpty(userId)
            ? Builders<RoommateBio>.Filter.Empty
            : Builders<RoommateBio>.Filter.Eq(b => b.UserId, userId);

        return await _bios.Find(filter).ToListAsync();
    }

    /// <summary>
    /// gets a specific roommate bio by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoommateBio>> GetById(string id)
    {
        var bio = MongoDbService.IsValidId(id)
            ? await _bios.Find(b => b.Id == id).FirstOrDefaultAsync()
            : null;
        return bio is null ? NotFound() : Ok(bio);
    }

    /// <summary>
    /// creates a roommate bio for a user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Post(RoommateBio bio)
    {
        if (string.IsNullOrWhiteSpace(bio.UserId))
        {
            return BadRequest("A userId is required.");
        }

        if (await _bios.Find(b => b.UserId == bio.UserId).AnyAsync())
        {
            return Conflict("This user already has a roommate bio.");
        }

        bio.Id = null;
        await _bios.InsertOneAsync(bio);
        return CreatedAtAction(nameof(GetById), new { id = bio.Id }, bio);
    }

    /// <summary>
    /// updates a roommate bio
    /// </summary>
    [HttpPut]
    public async Task<ActionResult> Update(RoommateBio bio)
    {
        if (!MongoDbService.IsValidId(bio.Id))
        {
            return NotFound();
        }

        var result = await _bios.ReplaceOneAsync(b => b.Id == bio.Id, bio);
        return result.MatchedCount > 0 ? Ok() : NotFound();
    }

    /// <summary>
    /// deletes a roommate bio
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        if (!MongoDbService.IsValidId(id))
        {
            return NotFound();
        }

        var result = await _bios.DeleteOneAsync(b => b.Id == id);
        return result.DeletedCount > 0 ? Ok() : NotFound();
    }
}
