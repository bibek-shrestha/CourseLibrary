﻿namespace CourseLibrary.API.ResourceParameters;

public class AuthorsResourceParameters: RequestParameters
{
    public string? MainCategory { get; set; }

    public string? SearchQuery { get; set; }

    public string OrderBy { get; set; } = "Name";

    public string? Fields { get; set; }
}
