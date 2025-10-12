using System;
using System.Text.RegularExpressions;

namespace HotelReservationSystemAPI.Domain.ValueObject
{
    public class EmailVO : ValueObject
    {
        public string Value { get; private set; } = string.Empty;

        protected EmailVO() { }

        private EmailVO(string value) => Value = value.ToLowerInvariant().Trim();

        public static Result<EmailVO> Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<EmailVO>.Failure("Email cannot be empty");

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return Result<EmailVO>.Failure("Invalid email format");

            return Result<EmailVO>.Success(new EmailVO(email));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
