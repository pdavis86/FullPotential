
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FullPotential.Api.GameManagement.Models
{
    public struct SignInResult
    {
        public string UserId { get; set; }

        public string Username { get; set; }

        public string Token { get; set; }

        public string CharacterId { get; set; }

        public bool IsInvalid { get; set; }
    }
}
