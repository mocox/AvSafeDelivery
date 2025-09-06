using System.Threading.Tasks;

namespace AvSafeDelivery.Services;

public interface IAuthService
{
    /// <summary>
    /// Start the OAuth flow for the named provider (e.g. "google" or "facebook").
    /// Returns the authorization code or an error string starting with "error:".
    /// </summary>
    Task<string> StartOAuthAsync(string provider);
}

