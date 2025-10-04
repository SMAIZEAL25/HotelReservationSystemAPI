
using HotelReservationAPI.Infrastructure.Repositories.Interface;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics.Metrics;
using System.Net;
using System.Text.RegularExpressions;

namespace HotelReservationSystemAPI.Domain.ValueObject
{


    public class Email : IEquatable<Email>
    {
        public string Value { get; private set; } = string.Empty;

        private Email(string value) => Value = value;

        public static APIResponse<Email> Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return APIResponse<Email>.Fail(HttpStatusCode.BadRequest, "Email cannot be empty");

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return APIResponse<Email>.Fail(HttpStatusCode.BadRequest, "Invalid email format");

            return APIResponse<Email>.Success(new Email(email), "Email created successfully");
        }

        public override bool Equals(object? obj) => obj is Email other && Value == other.Value;

        public bool Equals(Email? other) => other is not null && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;
    }
}
