using System.Text.Json;
using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet(Name = "GetAuthors")]
    [HttpHead]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(
        [FromQuery] AuthorsResourceParameters resourceParameters
    )
    {
        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(resourceParameters);
        var previousPageLink = authorsFromRepo.HasPrevious ? CreateAuthorResourceURI(resourceParameters, ResourceUriType.PREVIOUS_PAGE) : null;
        var nextPageLink = authorsFromRepo.HasNext ? CreateAuthorResourceURI(resourceParameters, ResourceUriType.NEXT_PAGE): null;
        var paginationMetadata = new {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            currentPage = authorsFromRepo.CurrentPage,
            totalPages = authorsFromRepo.TotalPages,
            previousPageLink,
            nextPageLink
        };

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

        // return them
        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
    }

    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<ActionResult<AuthorDto>> GetAuthor(Guid authorId)
    {
        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        // return author
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorCreationDto author)
    {
        var authorEntity = _mapper.Map<Entities.Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        return CreatedAtRoute("GetAuthor",
            new { authorId = authorToReturn.Id },
            authorToReturn);
    }

    private string? CreateAuthorResourceURI(AuthorsResourceParameters resourceParameters, ResourceUriType type)
    {
        switch (type)
        {
            case ResourceUriType.PREVIOUS_PAGE:
                return Url.Link("GetAuthors"
                    , new
                    {
                        pageNumber = resourceParameters.PageNumber - 1,
                        pageSize = resourceParameters.PageSize,
                        mainCategory = resourceParameters.MainCategory,
                        searchQuery = resourceParameters.SearchQuery
                    });
            case ResourceUriType.NEXT_PAGE:
                return Url.Link("GetAuthors"
                    , new
                    {
                        pageNumber = resourceParameters.PageNumber + 1,
                        pageSize = resourceParameters.PageSize,
                        mainCategory = resourceParameters.MainCategory,
                        searchQuery = resourceParameters.SearchQuery
                    });
            default:
                return Url.Link("GetAuthors"
                   , new
                   {
                       pageNumber = resourceParameters.PageNumber,
                       pageSize = resourceParameters.PageSize,
                       mainCategory = resourceParameters.MainCategory,
                       searchQuery = resourceParameters.SearchQuery
                   });
        }
    }
}
