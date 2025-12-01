namespace DroneBuilder.Application.Models;

public class PaginationParams(int page, int pageSize)
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}