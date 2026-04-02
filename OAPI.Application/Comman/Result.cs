using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Comman
{
	public class Result<T>
	{
		public bool IsSuccess { get; private set; }
		public T Value { get; private set; }
		public string ErrorMessage { get; private set; }

		private Result(bool isSuccess, T value, string errorMessage)
		{
			IsSuccess = isSuccess;
			Value = value;
			ErrorMessage = errorMessage;
		}

		public static Result<T> Success(T value)
		{
			return new Result<T>(true, value, null);
		}

		public static Result<T> Failure(string errorMessage)
		{
			return new Result<T>(false, default(T), errorMessage);
		}
	}
}
