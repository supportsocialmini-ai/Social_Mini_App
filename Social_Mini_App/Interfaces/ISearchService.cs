using Social_Mini_App.Dtos;
using Social_Mini_App.Dtos.Responses;
using Social_Mini_App.Models;

namespace Social_Mini_App.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResultResponse> SearchAsync(string query, Guid currentUserId);
    }
}
