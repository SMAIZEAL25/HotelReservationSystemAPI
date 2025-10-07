
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics.Metrics;
using System.Net;
using System.Text.RegularExpressions;

namespace HotelReservationSystemAPI.Domain.ValueObject
{


    public class EmailVO : IEquatable<EmailVO>
    {
        public string Value { get; private set; }

        // Private constructor with parameter
        private EmailVO(string value)
        {
            Value = value;
        }

        // Factory method returns generic Result<T>
        public static Result<EmailVO> Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<EmailVO>.Failure("Email cannot be empty");

            if (!System.Text.RegularExpressions.Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return Result<EmailVO>.Failure("Invalid email format");

            return Result<EmailVO>.Success(new EmailVO(email), "Email created successfully");
        }

        // Equality implementation
        public override bool Equals(object? obj)
            => obj is EmailVO other && Value == other.Value;

        public bool Equals(EmailVO? other)
            => other is not null && Value == other.Value;

        public override int GetHashCode()
            => Value.GetHashCode();

        public override string ToString()
            => Value;

        // Operators for convenience
        public static bool operator ==(EmailVO? left, EmailVO? right)
            => Equals(left, right);

        public static bool operator !=(EmailVO? left, EmailVO? right)
            => !Equals(left, right);
    }
}