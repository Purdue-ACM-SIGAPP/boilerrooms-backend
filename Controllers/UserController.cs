using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SimpleWebAppReact.Entities;
using SimpleWebAppReact.Services;

namespace SimpleWebAppReact.Controllers;

/// <summary>
/// Defines endpoints for operations relating the User table
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMongoCollection<User> _users;

    public UserController(MongoDbService mongoDbService)
    {
        _users = mongoDbService.Database.GetCollection<User>("user");
    }

    /// <summary>
    /// gets users matching the optional filters; an undefined account type is ignored.
    /// Filter by username to look an account's id up by hand.
    /// </summary>
    [HttpGet]
    public async Task<IEnumerable<User>> Get(
        [FromQuery] string? username = null,
        [FromQuery] string? name = null,
        [FromQuery] string? phoneNumber = null,
        [FromQuery] int? accountType = null)
    {
        var f = Builders<User>.Filter;
        var filter = f.Empty;

        if (!string.IsNullOrEmpty(username)) filter &= f.Eq(u => u.Username, username);
        if (!string.IsNullOrEmpty(name)) filter &= f.Eq(u => u.Name, name);
        if (!string.IsNullOrEmpty(phoneNumber)) filter &= f.Eq(u => u.PhoneNumber, phoneNumber);
        if (IsValidAccountType(accountType)) filter &= f.Eq(u => u.AccountType, accountType);

        return await _users.Find(filter).ToListAsync();
    }

    /// <summary>
    /// gets specific user with id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(string id)
    {
        var user = MongoDbService.IsValidId(id) ? await _users.Find(u => u.Id == id).FirstOrDefaultAsync() : null;
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// adds user entry to table
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Post(User user)
    {
        if (user.AccountType.HasValue && !IsValidAccountType(user.AccountType))
        {
            return BadRequest("Invalid account type.");
        }

        if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
        {
            return BadRequest("A username and password are required.");
        }

        if (await _users.Find(u => u.Username == user.Username).AnyAsync())
        {
            return Conflict("That username is already taken.");
        }

        user.Id = null;
        await _users.InsertOneAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>
    /// signs in: returns the account whose username and password match
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<User>> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Enter your username and password.");
        }

        var user = await _users.Find(u => u.Username == request.Username).FirstOrDefaultAsync();
        return user is null || user.Password != request.Password
            ? Unauthorized("That username and password don't match an account.")
            : Ok(user);
    }

    /// <summary>
    /// updates a user entry
    /// </summary>
    [HttpPut]
    public async Task<ActionResult> Update(User user)
    {
        if (user.AccountType.HasValue && !IsValidAccountType(user.AccountType))
        {
            return BadRequest("Invalid account type.");
        }

        if (!MongoDbService.IsValidId(user.Id))
        {
            return NotFound();
        }

        var result = await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        return result.MatchedCount > 0 ? Ok() : NotFound();
    }

    /// <summary>
    /// deletes a user entry
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        if (!MongoDbService.IsValidId(id))
        {
            return NotFound();
        }

        var result = await _users.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0 ? Ok() : NotFound();
    }

    private static bool IsValidAccountType(int? accountType) =>
        accountType.HasValue && Enum.IsDefined(typeof(UserType), accountType.Value);

    public record LoginRequest(string? Username, string? Password);
}
