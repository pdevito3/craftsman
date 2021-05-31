namespace Craftsman.Validators
{
    using Craftsman.Models;
    using FluentValidation;

    public class MessageValidator : AbstractValidator<Message>
    {
        public MessageValidator()
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage("Please specify a name for your message.");
        }
    }
}