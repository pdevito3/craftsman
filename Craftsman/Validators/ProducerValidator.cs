namespace Craftsman.Validators;

using Domain;
using FluentValidation;

public class ProducerValidator : AbstractValidator<Producer>
{
    public ProducerValidator()
    {
        RuleFor(c => c.ProducerName).NotEmpty().WithMessage("Please specify a consumer name.");
        RuleFor(c => c.EndpointRegistrationMethodName).NotEmpty().WithMessage("Please specify an endpoint registration method name. This is what will be used to register the service within MassTransit.");
        RuleFor(c => c.ExchangeName).NotEmpty().WithMessage("Please specify an exchange name. Note that this should match exchange name in the consumer that you want this to be linked to.");
        RuleFor(c => c.MessageName).NotEmpty().WithMessage("Please specify a message name. This is the type of message that you want the producer to, well produce!");
    }
}
