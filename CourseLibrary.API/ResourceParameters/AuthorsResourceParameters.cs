namespace CourseLibrary.API.ResourceParameters;

public class AuthorsResourceParameters: RequestParameters
{
    public string? MainCategory { get; set; }

    public string? SearchQuery { get; set; }
}
