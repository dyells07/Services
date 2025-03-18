using System;
using System.Security.Principal;
using System.Threading.Tasks;
using HawkNet;
using Microsoft.AspNetCore.Http;

namespace JWT_Refresh.Middlewares
{
    public class HawkMiddleware
    {
        private readonly RequestDelegate _next;

        public HawkMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var credentialsResolver = new Func<string, HawkCredential>(id =>
            {
                // Provide credentials based on the user ID
                if (id == "testuser")
                {
                    return new HawkCredential
                    {
                        Id = "testuser",
                        Key = "your-secret-key",
                        Algorithm = "sha256"
                    };
                }
                return null;
            });

            try
            {
                var request = context.Request;
                var method = request.Method;
                var host = request.Host.Value;
                var uri = request.Path.ToUriComponent();
                var fullUrl = $"{request.Scheme}://{host}{uri}";

                IPrincipal principal = Hawk.Authenticate(
                    method,                        
                    fullUrl,                       // Full request URL
                    request.Headers["Authorization"], // Hawk authorization header
                    new Uri(fullUrl),              // Request URI
                    credentialsResolver,           // Function to get credentials
                    0,                             // Nonce validation window
                    () => ""                       // Provide a nonce lookup function if needed
                );

                // Check if authentication failed
                if (principal == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid Hawk Authentication.");
                    return;
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync($"Hawk Authentication Failed: {ex.Message}");
                return;
            }

            await _next(context);
        }
    }
}
