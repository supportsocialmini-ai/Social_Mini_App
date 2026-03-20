namespace Social_Mini_App.Dtos.Requests
{
    public class PostUploadDto
    {
        public string Content { get; set; } = string.Empty;
        public string Privacy { get; set; } = "Public";
        public IFormFile? ImageFile { get; set; }
    }
}
