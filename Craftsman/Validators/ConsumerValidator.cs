namespace Craftsman.Validators
{
    using Craftsman.Models;
    using FluentValidation;

    public class ConsumerValidator : AbstractValidator<Consumer>
    {
        public ConsumerValidator()
        {
            RuleFor(c => c.EndpointRegistrationMethodName).NotEmpty().WithMessage("Please specify an endpoint registration method name. This is what will be used to register the service within MassTransit.");
            RuleFor(c => c.ConsumerName).NotEmpty().WithMessage("Please specify a consumer name.");
            RuleFor(c => c.QueueName).NotEmpty().WithMessage("Please specify a queue name.");
            RuleFor(c => c.ExchangeName).NotEmpty().WithMessage("Please specify an exhange name. Note that this should match exchange name in the producer that you want this to be linked to.");
            RuleFor(c => c.MessageName).NotEmpty().WithMessage("Please specify a message name. This is the type of message that you want the consumer to, well consume!");
        }
    }
}