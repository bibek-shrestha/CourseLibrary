namespace CourseLibrary.API;

public class RequestParameters
{
    const int MAXIMUM_PAGE_SIZE = 20;

    public int PageNumber { get; set; } = 1;

    protected int _pageSize = 10;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MAXIMUM_PAGE_SIZE) ? MAXIMUM_PAGE_SIZE : value;
    }

}
