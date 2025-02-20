using JWT_Refresh.Data;
using JWT_Refresh.Models;
using JWT_Refresh.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JWT_Refresh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserContext _userContext;
        private readonly ITokenService _tokenService;
        public TokenController(UserContext userContext, ITokenService tokenService)
        {
            this._userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            this._tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }
        [HttpPost]
        [Route("refresh")]
        public IActionResult Refresh(TokenApiModel tokenApiModel)
        {
            if (tokenApiModel == null)
                return BadRequest("Invalid client request");

            var principal = _tokenService.GetPrincipalFromExpiredToken(tokenApiModel.AccessToken);
            if (principal?.Identity?.Name == null)
                return BadRequest("Invalid token");

            var username = principal.Identity.Name;
            var user = _userContext.LoginModels.Find(username);

            if (user == null || user.RefreshToken != tokenApiModel.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Update refresh token securely
            using (var transaction = _userContext.Database.BeginTransaction())
            {
                user.RefreshToken = newRefreshToken;
                _userContext.SaveChanges();
                transaction.Commit();
            }

            return Ok(new AuthenticatedResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                //ExpiresIn = _tokenService.GetTokenExpiry(newAccessToken) // Include expiry for frontend tracking
            });
        }

        [HttpPost, Authorize]
        [Route("revoke")]
        public IActionResult Revoke()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not found");

            var user = _userContext.LoginModels.Find(username);
            if (user == null)
                return BadRequest("Invalid request");

            // Securely revoke token
            using (var transaction = _userContext.Database.BeginTransaction())
            {
                user.RefreshToken = null;
                _userContext.SaveChanges();
                transaction.Commit();
            }

            return NoContent();
        }

    }
}
