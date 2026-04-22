using Data.Data.HouseRentalData;

namespace HouseNet9.Services
{
    public interface ICommentNotificationService
    {
        Task SendCommentEditLinkAsync(Comment comment, string token);
        Task SendCommentAddedAsync(Comment comment);
    }
}
