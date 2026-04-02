using FluentValidation;
using OAPI.Application.Commands.CreateOrder;
using OAPI.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Validators
{
	public class CreateOrderValidator: AbstractValidator<CreateOrderCommand>
	{
		public CreateOrderValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Customer email is required.")
				.EmailAddress().WithMessage("Invalid email format.");

			RuleFor(x => x.Amount)
				.GreaterThan(0).WithMessage("Total amount must be greater than zero.");
		}
	}
}
