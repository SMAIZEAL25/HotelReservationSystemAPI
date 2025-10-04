
using HotelReservationSystemAPI.Application.CommonResponse;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Diagnostics.Metrics;
using System.Net;
using System.Text.RegularExpressions;

namespace HotelReservationSystemAPI.Domain.ValueObject
{
    public class Email : IEquatable<Email>
    {
        public string Value { get; private set; } =string.Empty;

        public Email(string value) => Value = value;

        public static APIResponse<Email> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return APIResponse<Email>.Fail(HttpStatusCode.BadRequest, "Email cannot be empty");
            }   

            if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return APIResponse<Email>.Fail(HttpStatusCode.BadRequest, "Invalid email format");

            return APIResponse<Email>.Success(new Email(value), "Email created successfully");
        }


        public override bool Equals(Object? obj) => obj is Email other && Value == other.Value;
        
        public bool Equals (Email? other ) =>  other is not null && Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;
        
               
        
    }
}
