using ETicaret.Business.Utilities.Results;
using ETicaret.Entity.Concrete;
using ETicaret.Entity.DTOs;

namespace ETicaret.Business.Abstract
{
    public interface IAuthService
    {
        Task<IDataResult<AppUser>> RegisterAsync(UserForRegisterDto userForRegisterDto, string password);
        Task<IDataResult<AccessToken>> LoginAsync(UserForLoginDto userForLoginDto);
        IDataResult<AccessToken> CreateAccessToken(AppUser user);
    }
}
