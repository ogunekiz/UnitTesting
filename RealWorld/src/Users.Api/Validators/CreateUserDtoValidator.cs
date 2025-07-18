﻿using FluentValidation;
using Users.Api.DTOs;

namespace Users.Api.Validations;

public sealed class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(p => p.FullName).NotEmpty().WithMessage("Full name cannot be null or empty");
        RuleFor(p => p.FullName).MinimumLength(3).WithMessage("Full name must be greator than 3 letter");
    }
}
