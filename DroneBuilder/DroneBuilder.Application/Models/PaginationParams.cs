namespace DroneBuilder.Application.Models;

public class PaginationParams
{
    public PaginationParams()
    {
    }

    public PaginationParams(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
