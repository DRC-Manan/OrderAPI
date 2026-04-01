using FluentValidation;
using OAPI.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Validators
{
	public class CreateOrderValidator: AbstractValidator<Order>
	{
		public CreateOrderValidator()
		{
			RuleFor(x => x.CustomerEmail)
				.NotEmpty().WithMessage("Customer email is required.")
				.EmailAddress().WithMessage("Invalid email format.");

			RuleFor(x => x.TotalAmount)
				.GreaterThan(0).WithMessage("Total amount must be greater than zero.");
		}
	}
}
