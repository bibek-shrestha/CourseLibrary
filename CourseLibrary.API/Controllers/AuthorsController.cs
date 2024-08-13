using System.Net;
using System.Text.Json;
using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;

    private readonly IPropertyMappingService _propertyMappingService;

    private readonly IPropertyCheckerService _propertyCheckerService;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository
        , IMapper mapper
        , IPropertyMappingService propertyMappingService
        , IPropertyCheckerService propertyCheckerService
        , ProblemDetailsFactory problemDetailsFactory)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
        _propertyMappingService = propertyMappingService
            ?? throw new ArgumentNullException(nameof(propertyMappingService));
        _propertyCheckerService = propertyCheckerService
            ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        _problemDetailsFactory = problemDetailsFactory
            ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    }

    [HttpGet(Name = "GetAuthors")]
    [HttpHead]
    public async Task<IActionResult> GetAuthors(
        [FromQuery] AuthorsResourceParameters resourceParameters
    )
    {
        if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Entities.Author>(resourceParameters.OrderBy))
        {
            return BadRequest();
        }
        if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(resourceParameters.Fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(
                HttpContext
                , statusCode: 400
                , detail: $"Some or all of the requested fields does not exist on the resource: {resourceParameters.Fields}."
            ));
        };
        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(resourceParameters);
        var paginationMetadata = new
        {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            currentPage = authorsFromRepo.CurrentPage,
            totalPages = authorsFromRepo.TotalPages
        };

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
        var links = CreateLinksForAuthors(resourceParameters, authorsFromRepo.HasNext, authorsFromRepo.HasPrevious);
        var shapedResponse = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo).ShapeData(resourceParameters.Fields);
        var shapedResponseWithLinks = shapedResponse.Select(author =>
        {
            var authorDictionary = author as IDictionary<string, object?>;
            authorDictionary.Add("links", CreateLinksForAuthor((Guid)authorDictionary["Id"], null));
            return authorDictionary;
        });
        var linkedResourceResponse = new
        {
            value = shapedResponseWithLinks,
            links
        };
        // return them
        return Ok(linkedResourceResponse);
    }

    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<IActionResult> GetAuthor(
        [FromRoute] Guid authorId
        , [FromQuery] string? fields
        , [FromHeader(Name =  "Accept")] string? mediaType)
    {
        if (!MediaTypeHeaderValue  .TryParse(mediaType, out var mediaTypeHeaderValue))
        {
            return BadRequest(
                _problemDetailsFactory.CreateProblemDetails
                    (HttpContext
                    , statusCode: 400
                    , detail: $"{mediaType} is not supported")
            );
        }
        if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(
                HttpContext
                , statusCode: 400
                , detail: $"Some or all of the requested fields does not exist on the resource: {fields}."
            ));
        };
        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }
        if (mediaTypeHeaderValue.MediaType == "application/vnd.darkhorse.hateos+json")
        {
            var links = CreateLinksForAuthor(authorId, fields);
            var resourceResponse = _mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields)
                as IDictionary<string, object?>;
            resourceResponse.Add("links", links);

            // return author
            return Ok(resourceResponse);
        }
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
    }

    [HttpPost(Name = "CreateAuthor")]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorCreationDto author)
    {
        var authorEntity = _mapper.Map<Entities.Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        var links = CreateLinksForAuthor(authorToReturn.Id, null);

        var resourceResponse = authorToReturn.ShapeData(null)
            as IDictionary<string, object?>;
        resourceResponse.Add("links", links);

        return CreatedAtRoute("GetAuthor",
            new { authorId = resourceResponse["Id"] },
            resourceResponse);
    }

    private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string? fields)
    {
        var links = new List<LinkDto>();
        links.Add(new(
                string.IsNullOrWhiteSpace(fields)
                ? Url.Link("GetAuthor", new { authorId })
                : Url.Link("GetAuthor", new { authorId, fields })
            , "self"
            , "GET"));
        links.Add(new(
            Url.Link("CreateCourseForAuthor", new { authorId })
            , "create_course_for_author"
            , "POST"));
        links.Add(new(
            Url.Link("GetCoursesForAuthor", new { authorId })
            , "courses"
            , "GET"));
        return links;
    }

    private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters resourceParameters
        , bool hasNext, bool hasPrevious)
    {
        var links = new List<LinkDto>();
        links.Add(new((CreateAuthorResourceURI(resourceParameters, ResourceUriType.CURRENT_PAGE))
            , "self"
            , "GET"));
        if (hasNext)
        {
            links.Add(new((CreateAuthorResourceURI(resourceParameters, ResourceUriType.NEXT_PAGE))
            , "nextPage"
            , "GET"));
        }
        if (hasPrevious)
        {
            links.Add(new((CreateAuthorResourceURI(resourceParameters, ResourceUriType.PREVIOUS_PAGE))
            , "previousPage"
            , "GET"));
        }
        return links;
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
                        searchQuery = resourceParameters.SearchQuery,
                        orderBy = resourceParameters.OrderBy,
                        fields = resourceParameters.Fields
                    });
            case ResourceUriType.NEXT_PAGE:
                return Url.Link("GetAuthors"
                    , new
                    {
                        pageNumber = resourceParameters.PageNumber + 1,
                        pageSize = resourceParameters.PageSize,
                        mainCategory = resourceParameters.MainCategory,
                        searchQuery = resourceParameters.SearchQuery,
                        orderBy = resourceParameters.OrderBy,
                        fields = resourceParameters.Fields
                    });
            case ResourceUriType.CURRENT_PAGE:
            default:
                return Url.Link("GetAuthors"
                   , new
                   {
                       pageNumber = resourceParameters.PageNumber,
                       pageSize = resourceParameters.PageSize,
                       mainCategory = resourceParameters.MainCategory,
                       searchQuery = resourceParameters.SearchQuery,
                       orderBy = resourceParameters.OrderBy,
                       fields = resourceParameters.Fields
                   });
        }
    }
}
