using System;

namespace CourseLibrary.API.Models;

public class AuthorCreationWithDateOfDeathDto: AuthorCreationDto
{
    public DateTimeOffset? DateOfDeath { get; set; } 
}
