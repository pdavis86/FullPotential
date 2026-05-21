namespace FullPotential.Api.Data.Models
{
    public struct SignInResult
    {
        public string Token { get; set; }

        public bool IsInvalid { get; set; }
    }
}
