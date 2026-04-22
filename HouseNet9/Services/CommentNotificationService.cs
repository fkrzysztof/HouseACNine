using Data.Data.HouseRentalData;
using HouseNet9.ViewModels;
using Mail;

namespace HouseNet9.Services
{
    public class CommentNotificationService : ICommentNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IRazorViewToStringRenderer _renderer;

        public CommentNotificationService(
            IEmailService emailService,
            IRazorViewToStringRenderer renderer)
        {
            _emailService = emailService;
            _renderer = renderer;
        }

        public async Task SendCommentEditLinkAsync(Comment comment, string token)
        {
            var link = $"https://stronaWWW.pl/Comments/Edit?token={token}";

            var model = new EmailSimpleViewModel
            {
                Title = "Edycja opinii",
                LogoUrl = "/uploads/logo.png",
                ButtonUrl = link,
                ButtonText = "Edytuj opinię",
                FooterText = "Link ważny 24 godziny"
            };

            var body = await _renderer.RenderViewToStringAsync(
                "Email/CommentNotification",
                model);

            await _emailService.SendEmailAsync(
                comment.Email,
                "Edycja Twojej opinii",
                body);
        }

        public async Task SendCommentAddedAsync(Comment comment)
        {
            var model = new EmailSimpleViewModel
            {
                Title = "Dziękujemy za opinię",
                LogoUrl = "/uploads/logo.png",
                ButtonText = "Zobacz stronę",
                ButtonUrl = "https://twojastrona.pl"
            };

            var body = await _renderer.RenderViewToStringAsync(
                "Email/CommentAdded",
                model);

            await _emailService.SendEmailAsync(
                comment.Email,
                "Dziękujemy za opinię",
                body);
        }
    }
}
