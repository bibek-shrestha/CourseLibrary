﻿
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authors/{authorId}/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;

    public CoursesController(ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet(Name = "GetCoursesForAuthor")]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesForAuthor(Guid authorId)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var coursesForAuthorFromRepo = await _courseLibraryRepository.GetCoursesAsync(authorId);
        return Ok(_mapper.Map<IEnumerable<CourseDto>>(coursesForAuthorFromRepo));
    }

    [HttpGet("{courseId}", Name = "GetCourseForAuthor")]
    public async Task<ActionResult<CourseDto>> GetCourseForAuthor(Guid authorId, Guid courseId)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var courseForAuthorFromRepo = await _courseLibraryRepository.GetCourseAsync(authorId, courseId);

        if (courseForAuthorFromRepo == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<CourseDto>(courseForAuthorFromRepo));
    }


    [HttpPost(Name = "CreateCourseForAuthor")]
    public async Task<ActionResult<CourseDto>> CreateCourseForAuthor(
            Guid authorId, CourseCreationDto course)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var courseEntity = _mapper.Map<Entities.Course>(course);
        _courseLibraryRepository.AddCourse(authorId, courseEntity);
        await _courseLibraryRepository.SaveAsync();

        var courseToReturn = _mapper.Map<CourseDto>(courseEntity);
        return CreatedAtRoute("GetCourseForAuthor",
                    new {authorId, courseId = courseToReturn.Id}
                    , courseToReturn);
    }

    [HttpPatch("{courseId}")]
    public async Task<ActionResult> PartiallyUpdateCourseOfAuthor(
        [FromRoute] Guid authorId,
        [FromRoute] Guid courseId,
        [FromBody] JsonPatchDocument<CourseUpdateDto> patchDocument)
        {
            if (!await _courseLibraryRepository.AuthorExistsAsync(authorId)) {
                return NotFound();
            }
            var courseFromAuthor = await _courseLibraryRepository.GetCourseAsync(authorId, courseId);
            if (courseFromAuthor == null) 
            {
                return NotFound();
            }
            var courseUpdateDto = _mapper.Map<CourseUpdateDto>(courseFromAuthor);
            patchDocument.ApplyTo(courseUpdateDto, ModelState);
            if (!TryValidateModel(courseUpdateDto))
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(courseUpdateDto, courseFromAuthor);
            _courseLibraryRepository.UpdateCourse(courseFromAuthor);
            await _courseLibraryRepository.SaveAsync();
            return NoContent();

        }


    [HttpPut("{courseId}")]
    public async Task<IActionResult> UpdateCourseForAuthor(Guid authorId,
      Guid courseId,
      CourseUpdateDto course)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var courseForAuthorFromRepo = await _courseLibraryRepository.GetCourseAsync(authorId, courseId);

        if (courseForAuthorFromRepo == null)
        {
            var courseToAdd = _mapper.Map<Course>(course);
            courseToAdd.Id = courseId;
            _courseLibraryRepository.AddCourse(authorId, courseToAdd);
            await _courseLibraryRepository.SaveAsync();
            var courseForResponse = _mapper.Map<CourseDto>(courseToAdd);
            return CreatedAtRoute("GetCourseForAuthor"
                , new {authorId, courseId = courseForResponse.Id}
                , courseForResponse);
        }

        _mapper.Map(course, courseForAuthorFromRepo);

        _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);

        await _courseLibraryRepository.SaveAsync();
        return NoContent();
    }

    [HttpDelete("{courseId}")]
    public async Task<ActionResult> DeleteCourseForAuthor(Guid authorId, Guid courseId)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var courseForAuthorFromRepo = await _courseLibraryRepository.GetCourseAsync(authorId, courseId);

        if (courseForAuthorFromRepo == null)
        {
            return NotFound();
        }

        _courseLibraryRepository.DeleteCourse(courseForAuthorFromRepo);
        await _courseLibraryRepository.SaveAsync();

        return NoContent();
    }

}