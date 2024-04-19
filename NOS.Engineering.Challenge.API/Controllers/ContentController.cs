using System.Net;
using Microsoft.AspNetCore.Mvc;
using NOS.Engineering.Challenge.API.Models;
using NOS.Engineering.Challenge.Managers;

namespace NOS.Engineering.Challenge.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ContentController : Controller
{
    private readonly IContentsManager _manager;
    private readonly ILogger<ContentController> _logger;
    public ContentController(IContentsManager manager, ILogger<ContentController> logger)
    {
        _manager = manager;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetManyContents()
    {
        var contents = await _manager.GetManyContents().ConfigureAwait(false);

        if (!contents.Any())
            return NotFound();
        
        return Ok(contents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetContent(Guid id)
    {
        var content = await _manager.GetContent(id).ConfigureAwait(false);

        if (content == null)
            return NotFound();
        
        return Ok(content);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateContent(
        [FromBody] ContentInput content
        )
    {
        var createdContent = await _manager.CreateContent(content.ToDto()).ConfigureAwait(false);

        return createdContent == null ? Problem() : Ok(createdContent);
    }
    
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateContent(
        Guid id,
        [FromBody] ContentInput content
        )
    {
        var updatedContent = await _manager.UpdateContent(id, content.ToDto()).ConfigureAwait(false);

        return updatedContent == null ? NotFound() : Ok(updatedContent);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContent(
        Guid id
    )
    {
        var deletedId = await _manager.DeleteContent(id).ConfigureAwait(false);
        return Ok(deletedId);
    }
    
    [HttpPost("{id}/genre")]
    public async Task<IActionResult> AddGenres(
        Guid id,
        [FromBody] IEnumerable<string> genre
    )
    {
        try
        {
            _logger.LogInformation("Adding genres for content with ID {Id}", id);

            var content = await _manager.GetContent(id).ConfigureAwait(false);
            if (content == null)
            {
                _logger.LogInformation("Content with ID {Id} not found", id);
                return NotFound();
            }

            var existingGenres = content.GenreList.ToList();
            existingGenres.AddRange(genre.Except(existingGenres));
            content.GenreList = existingGenres.Distinct().ToList();

            var updatedContent = await _manager.UpdateContent(id, content.ToDto()).ConfigureAwait(false);

            _logger.LogInformation("Genres added successfully for content with ID {Id}", id);

            return Ok(updatedContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding genres for content with ID {Id}", id);
            return Problem();
        }        
    }
    
    [HttpDelete("{id}/genre")]
    public async Task<IActionResult> RemoveGenres(
        Guid id,
        [FromBody] IEnumerable<string> genre
    )
    {
        try
        {
            _logger.LogInformation("Removing genres for content with ID {Id}", id);

            var content = await _manager.GetContent(id).ConfigureAwait(false);
            if (content == null)
            {
                _logger.LogInformation("Content with ID {Id} not found", id);
                return NotFound();
            }

            var existingGenres = content.GenreList.ToList();
            existingGenres.RemoveAll(genre.Contains);
            content.GenreList = existingGenres;

            var updatedContent = await _manager.UpdateContent(id, content.ToDto()).ConfigureAwait(false);

            _logger.LogInformation("Genres removed successfully for content with ID {Id}", id);

            return Ok(updatedContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while removing genres for content with ID {Id}", id);
            return Problem();
        }        
    }
}