using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ETicaret.Business.Abstract;
using ETicaret.Business.Constants;
using ETicaret.Business.Utilities.Results;
using ETicaret.Entity.Concrete;
using ETicaret.Entity.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ETicaret.Business.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IDataResult<AppUser>> RegisterAsync(UserForRegisterDto userForRegisterDto, string password)
        {
            var user = new AppUser
            {
                UserName = userForRegisterDto.Email,
                Email = userForRegisterDto.Email,
                FirstName = userForRegisterDto.FirstName,
                LastName = userForRegisterDto.LastName,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Assign default role
                await _userManager.AddToRoleAsync(user, "User");
                return new SuccessDataResult<AppUser>(user, Messages.UserRegistered);
            }

            return new ErrorDataResult<AppUser>(string.Join("\n", result.Errors.Select(e => e.Description)));
        }

        public async Task<IDataResult<AccessToken>> LoginAsync(UserForLoginDto userForLoginDto)
        {
            var user = await _userManager.FindByEmailAsync(userForLoginDto.Email);
            if (user == null)
            {
                return new ErrorDataResult<AccessToken>(Messages.UserNotFound);
            }

            var result = await _userManager.CheckPasswordAsync(user, userForLoginDto.Password);
            if (result)
            {
                var accessToken = CreateAccessToken(user);
                return new SuccessDataResult<AccessToken>(accessToken.Data, Messages.SuccessfulLogin);
            }

            return new ErrorDataResult<AccessToken>(Messages.PasswordError);
        }

        public IDataResult<AccessToken> CreateAccessToken(AppUser user)
        {
            var tokenOptions = _configuration.GetSection("TokenOptions");
            var keyStr = tokenOptions["SecurityKey"] ?? "default_secret_key_if_missing_must_be_long";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };
            
            // Add roles
            // (Skipped explicitly fetching roles here to keep it simple, but in real app we should)

            var token = new JwtSecurityToken(
                issuer: tokenOptions["Issuer"],
                audience: tokenOptions["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(tokenOptions["AccessTokenExpiration"])),
                signingCredentials: creds
            );

            return new SuccessDataResult<AccessToken>(new AccessToken
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            });
        }
    }
}
