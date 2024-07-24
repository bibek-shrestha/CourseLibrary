using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authorcollections")]
public class AuthorCollectionsController: ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;

    private readonly IMapper _mapper;

    public AuthorCollectionsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper)
    {
        _courseLibraryRepository = courseLibraryRepository;
        _mapper = mapper;
    }

    [HttpGet("({authorIds})", Name = "GetAuthorCollection")]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthorCollection(
        [ModelBinder(BinderType = typeof(ArrayModelBinder))]
        [FromRoute] IEnumerable<Guid> authorIds)
    {
        var authorEntities = await _courseLibraryRepository.GetAuthorsAsync(authorIds);
        if (authorIds.Count() != authorEntities.Count())
        {
            return NotFound();
        }
        var authorCollectionDto = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
        return Ok(authorCollectionDto);
    }


    [HttpPost]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> CreateAuthorCollection(
        IEnumerable<AuthorCreationDto> authorCollection)
    {
        var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);
        foreach(var author in authorEntities)
        {
            _courseLibraryRepository.AddAuthor(author);
        }
        await _courseLibraryRepository.SaveAsync();

        var authorCollectionDto = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

        var authorIds = string.Join(", ", authorCollectionDto.Select(a => a.Id));

        return CreatedAtRoute("GetAuthorCollection"
            , new {authorIds}, authorCollectionDto);
    } 

}
