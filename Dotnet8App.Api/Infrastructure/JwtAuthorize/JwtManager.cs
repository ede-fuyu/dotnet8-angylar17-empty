using Dotnet8App.Service;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dotnet8App.Api.Infrastructure
{
    public class JwtManager
    {
        private readonly ClaimsPrincipal user;
        private readonly ISession session;
        private readonly IConfiguration Configuration;
        private readonly IIdentityService IdentityService;

        private readonly string JwtToken = "JwtToken";
        private readonly string JwtSignKey = "@6LpZg9hK2tY!rA8jB0xW3qD5sF7uX1vH4oMlCpN2iRfEySdUbG6T";

        public JwtManager(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IIdentityService identityService)
        {
            if (httpContextAccessor.HttpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor));
            }

            user = httpContextAccessor.HttpContext.User;
            session = httpContextAccessor.HttpContext.Session;
            Configuration = configuration;
            IdentityService = identityService;
        }

        /// <summary>
        /// 取得 Jwt Token
        /// </summary>
        public string? GetToken()
        {
            return session.GetString(this.JwtToken);
        }

        /// <summary>
        /// 注銷 Jwt Token
        /// </summary>
        public void RemoveToken()
        {
            session.Remove(this.JwtToken);
        }

        /// <summary>
        /// 生成 Jwt Token
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="expireMinutes"></param>
        /// <returns></returns>
        public string GenerateToken(string userName, int? expireMinutes = null)
        {
            var issuer = Configuration.GetValue<string>("JwtSettings:Issuer");
            var signKey = Configuration.GetValue<string>("JwtSettings:SignKey") ?? JwtSignKey;

            // Configuring "Claims" to your JWT Token
            var claims = new List<Claim>
            {
                // In RFC 7519 (Section#4), there are defined 7 built-in Claims, but we mostly use 2 of them.
                //claims.Add(new Claim(JwtRegisteredClaimNames.Iss, issuer));
                new(JwtRegisteredClaimNames.Sub, userName), // User.Identity.Name
                                                                  //claims.Add(new Claim(JwtRegisteredClaimNames.Aud, "The Audience"));
                                                                  //claims.Add(new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds().ToString()));
                                                                  //claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())); // 必須為數字
                                                                  //claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())); // 必須為數字
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID

            };

            var identityUser = IdentityService.GetUser(userName);

            if (identityUser != null && !string.IsNullOrEmpty(identityUser.Email))
            {
                claims.Add(new("UserMail", identityUser.Email));
            }

            // The "NameId" claim is usually unnecessary.
            //claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userName));

            // This Claim can be replaced by JwtRegisteredClaimNames.Sub, so it's redundant.
            //claims.Add(new Claim(ClaimTypes.Name, userName));

            // TODO: You can define your "roles" to your Claims.
            //claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            //claims.Add(new Claim(ClaimTypes.Role, "Users"));

            var userClaimsIdentity = new ClaimsIdentity(claims);

            // Create a SymmetricSecurityKey for JWT Token signatures
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey));

            // HmacSha256 MUST be larger than 128 bits, so the key can't be too short. At least 16 and more characters.
            // https://stackoverflow.com/questions/47279947/idx10603-the-algorithm-hs256-requires-the-securitykey-keysize-to-be-greater
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            // Create SecurityTokenDescriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                //Audience = issuer, // Sometimes you don't have to define Audience.
                //NotBefore = DateTime.Now, // Default is DateTime.Now
                //IssuedAt = DateTime.Now, // Default is DateTime.Now
                Subject = userClaimsIdentity,
                Expires = DateTime.Now.AddMinutes(expireMinutes ?? 30),
                SigningCredentials = signingCredentials
            };

            // Generate a JWT securityToken, than get the serialized Token result (string)
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var serializeToken = tokenHandler.WriteToken(securityToken);

            if (!expireMinutes.HasValue)
            {
                session.SetString(this.JwtToken, serializeToken);
            }

            return serializeToken;
        }

        /// <summary>
        /// 進行 Jwt 驗證
        /// </summary>
        /// <returns></returns>
        public bool VerifyToken(string answerToken, params string[] permissions)
        {
            var havePermission = false;
            var jwtToken = this.GetToken();
            if (jwtToken != null && string.Equals(answerToken, jwtToken, StringComparison.CurrentCultureIgnoreCase))
            {
                if ((permissions ?? []).Length != 0)
                {

                }
                else
                {
                    havePermission = true;
                }
            }

            return havePermission;
        }
    }
}
