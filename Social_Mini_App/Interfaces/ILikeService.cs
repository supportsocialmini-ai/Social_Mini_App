namespace Social_Mini_App.Interfaces
{
    public interface ILikeService
    {
        // Trả về true nếu là Like, false nếu là Unliked
        Task<bool> ToggleLikeAsync(Guid postId, Guid userId);
    }
}