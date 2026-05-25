
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FullPotential.Api.GameManagement.Models
{
    public struct SignInResult
    {
        public string Token { get; set; }

        public bool IsInvalid { get; set; }
    }
}
