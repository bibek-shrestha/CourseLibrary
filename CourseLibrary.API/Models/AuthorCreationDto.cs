namespace CourseLibrary.API.Models;

public class AuthorCreationDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTimeOffset DateOfBirth { get; set; }

    public string MainCategory { get; set; } = string.Empty;

    public ICollection<CourseCreationDto> Courses{ get; set; } = new List<CourseCreationDto>();

}
