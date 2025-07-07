#if false
using Microsoft.AspNetCore.Mvc;

using PokemonTeam.Models;

/// <summary>
/// Controller for managing "Objet" entities.
/// </summary>
/// <remarks>
/// Routes :
/// <list type="bullet">
///   <item>
///     <description>GET api/objet : Get all objects</description>
///   </item>
///   <item>
///     <description>GET api/objet/{id} : Get an object by ID</description>
///   </item>
///   <item>
///     <description>POST api/objet : Create a new object</description>
///   </item>
///   <item>
///     <description>PUT api/objet/{id} : Update an existing object</description>
///   </item>
///   <item>
///     <description>DELETE api/objet/{id} : Delete an object</description>
///   </item>
/// </list>
/// </remarks>
/// <author>
/// Elerig
/// </author>
[Route("api/[controller]")]
[ApiController]
public class ObjetController : ControllerBase
{
    /// <summary>
    /// Database context to access "Objet" entities.
    /// </summary>
    private readonly ArenaContext _context;

    /// <summary>
    /// Constructor with dependency injection.
    /// </summary>
    /// <param name="context">Injected ArenaContext</param>
    public ObjetController(ArenaContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all objects from the database.
    /// </summary>
    /// <returns>List of objects</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Objet>>> GetObjets()
    {
        return await _context.Objets.ToListAsync();
    }

    /// <summary>
    /// Get a specific object by its ID.
    /// </summary>
    /// <param name="id">ID of the object</param>
    /// <returns>The requested object</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Objet>> GetObjet(int id)
    {
        var objet = await _context.Objets.FindAsync(id);

        if (objet == null)
            return NotFound();

        return objet;
    }

    /// <summary>
    /// Create a new object.
    /// </summary>
    /// <param name="objet">The object to create</param>
    /// <returns>The created object with HTTP 201 status</returns>
    [HttpPost]
    public async Task<ActionResult<Objet>> CreateObjet(Objet objet)
    {
        _context.Objets.Add(objet);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetObjet), new { id = objet.Id }, objet);
    }

    /// <summary>
    /// Update an existing object by ID.
    /// </summary>
    /// <param name="id">ID of the object</param>
    /// <param name="objet">Modified object</param>
    /// <returns>NoContent if successful</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateObjet(int id, Objet objet)
    {
        if (id != objet.Id)
            return BadRequest();

        _context.Entry(objet).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Objets.Any(e => e.Id == id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Delete an object by ID.
    /// </summary>
    /// <param name="id">ID of the object</param>
    /// <returns>NoContent if deleted, NotFound if not found</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteObjet(int id)
    {
        var objet = await _context.Objets.FindAsync(id);
        if (objet == null)
            return NotFound();

        _context.Objets.Remove(objet);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
#endif