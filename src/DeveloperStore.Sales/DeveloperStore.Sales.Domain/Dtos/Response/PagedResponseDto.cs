using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Dtos.Response;

[ExcludeFromCodeCoverage]
public class PagedResponseDto<T>
{
    public IEnumerable<T> Data { get; set; }
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    public PagedResponseDto(IEnumerable<T> data, int totalItems, int currentPage, int totalPages)
    {
        Data = data;
        TotalItems = totalItems;
        CurrentPage = currentPage;
        TotalPages = totalPages;
    }
}
