using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EQ_Shared;
using Grpc.Core;
using System.Text.RegularExpressions;
using MudBlazor;
using Google.Protobuf.WellKnownTypes;
using EQ_Server.Models;
using Microsoft.Extensions.Configuration;

namespace EQ_Server.Services
{
    public class AuthenticationService : Authentication.AuthenticationBase
    {
        private readonly DataContext _db;
        public AuthenticationService(DataContext db)
        {
            this._db = db;
        }

        public override Task<Empty> GetCode(Phone request, ServerCallContext context)
        {
            // Generate a random code
            var code = new Random().Next(100000, 999999).ToString();
            // Check if the phone number is already in the database and if so, update the code
            var signin = _db.Signins.FirstOrDefault(v => v.Phone == request.Value);
            if (signin != null)
            {
                if (signin.Attempts > 0)
                {
                    signin.Code = code;
                    signin.Expiration = DateTime.UtcNow.AddMinutes(3);
                    signin.Attempts -= 1;
                }
                else if (signin.Attempts == 0 && (DateTime.UtcNow - signin.Expiration).Days > 0)
                {
                    signin.Code = code;
                    signin.Expiration = DateTime.UtcNow.AddMinutes(3);
                    signin.Attempts = 10;
                }
            }
            // If not, create a new record
            else
            {
                User user = new User(request.Value);
                _db.Users.Add(user);
                _db.SaveChanges();
                _db.Signins.Add(new Signin(user.Id, request.Value, code));
                _db.Queues.Add(new EQ_Server.Models.Queue(user.Id));
            }
            _db.SaveChanges();
            // Send the code to the phone number
            Console.WriteLine("Code: " + code);
            return Task.FromResult<Empty>(new Empty());
        }


        public override Task<AccessToken> Validate(Code request, ServerCallContext context)
        {
            // Get phone from claims
            var phone = request.Phone;
            // Check if code is valid
            var signin = _db.Signins.FirstOrDefault(x => x.Phone == phone);
            if (signin!=null && signin.Code == request.Value)
            {
                var user = _db.Users.FirstOrDefault(x => x.Id == signin.UserId);
                signin.Attempts = 10;
                _db.SaveChanges();
                return Task.FromResult(new AccessToken() { Token = CreateAccessToken(user) });
            }
            return base.Validate(request, context);
        }


        private string CreateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim("FirstName", user.FirstName),
                new Claim("Role", user.Role)
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("super secret key"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(30),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}