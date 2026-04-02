namespace HouseNet9.ViewModels
{
    public class CommentModerationViewModel
    {
        public int Id { get; set; }
        public string ClientText { get; set; }
        public string AdminText { get; set; }
        public bool IsApproved { get; set; }
    }
}
