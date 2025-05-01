using CustomerManagement.API.Controllers;
using CustomerManagement.API.Settings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManagement.Tests
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly JwtSettings _jwtSettings;

        public AuthControllerTests()
        {
            _jwtSettings = new JwtSettings
            {
                SecretKey = "ThisIsAReallyStrongSecretKeyForTestingPurposes123!",
                Issuer = "test-issuer",
                Audience = "test-audience"
            };
            var options = Options.Create(_jwtSettings);
            _controller = new AuthController(options);
        }

        [Fact]
        public void Login_With_InvalidCredentials_ReturnsUnauthorized()
        {
            var badRequest = new LoginRequest { Username = "foo", Password = "bar" };

            var result = _controller.Login(badRequest);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void Login_With_ValidCredentials_ReturnsOk_withValidJwt()
        {
            var goodRequest = new LoginRequest { Username = "admin", Password = "password" };

            var result = _controller.Login(goodRequest);

            var ok = Assert.IsType<OkObjectResult>(result);

            var tokenProp = ok.Value
                              .GetType()
                              .GetProperty("token", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(tokenProp);

            var token = tokenProp.GetValue(ok.Value) as string;
            Assert.False(string.IsNullOrWhiteSpace(token));

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.Equal(_jwtSettings.Issuer, jwt.Issuer);
            Assert.Contains(_jwtSettings.Audience, jwt.Audiences);

            Assert.Contains(jwt.Claims, c =>
                c.Type == JwtRegisteredClaimNames.Sub
             && c.Value == goodRequest.Username);

            Assert.Contains(jwt.Claims, c =>
                c.Type == JwtRegisteredClaimNames.Jti
             && !string.IsNullOrWhiteSpace(c.Value));

            Assert.Contains(jwt.Claims, c =>
                c.Type == ClaimTypes.Role
             && c.Value == "CustomerAdmin");
        }
    }
}
