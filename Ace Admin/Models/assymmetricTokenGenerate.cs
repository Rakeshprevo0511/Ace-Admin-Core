using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Ace_Admin.Models
{
    public class assymmetricTokenGenerate
    {
        private readonly IConfiguration _config;

        public assymmetricTokenGenerate(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateJwtToken(string username, int empId, string machineid)
        {

            var privateKey = File.ReadAllText(_config["JwtSettings:PrivateKeyPath"]);
            int expireMinutesAccess = Convert.ToInt32(_config["JwtSettings:ExpireMinutesAccess"]);
            var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
            
            int expireMinutes = expireMinutesAccess;
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);

            // 🔹 Create signing credentials
            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256
            );
            var claims = new[] {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, empId.ToString()),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("tokenType", "accessToken"),
            new Claim(ClaimTypes.Expired,indiaTime.AddMinutes(expireMinutes).ToString()),
            new Claim("MachineID", machineid),
     
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: indiaTime.AddMinutes(expireMinutes),
                signingCredentials: signingCredentials
            );

            // 🔹 Return serialized token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateAccessToken(string username, int empId, string machineId)
        {
            // Load RSA Private Key (PEM)
            var privateKeyPem = File.ReadAllText(_config["JwtSettings:PrivateKeyPath"]);
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);

            int expireMinutes = Convert.ToInt32(_config["JwtSettings:ExpireMinutesAccess"]);

            // Convert UTC → IST
            var indiaZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaZone);
            var expiry = now.AddMinutes(expireMinutes);

            // Create signing credentials
            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256
            );

            // Claims
            var claims = new[]
            {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.NameIdentifier, empId.ToString()),
        new Claim(ClaimTypes.Role, "Admin"),

        // 🔥 important
        new Claim("tokenType", "accessToken"),
        new Claim("machineId", machineId),

        // 🔥 JTI for traceability
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // Create token
            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                notBefore: now,
                expires: expiry,
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
