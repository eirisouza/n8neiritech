using FluentValidation;
using n8neiritech.Application.DTOs;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Application.Validators;

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .When(x => x.Type == MessageType.Text);
        RuleFor(x => x.MediaUrl)
            .NotEmpty()
            .When(x => x.Type != MessageType.Text);
    }
}
