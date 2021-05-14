namespace Assets.Core.Registry
{
    public class UserRegistry
    {
        public string Token { get; private set; }

        public void SignIn(string username, string password)
        {
            //todo: implement UserRegistry.SignIn()
            Token = username;
        }

        public string GetUsername(string token)
        {
            //todo: implement UserRegistry.GetUsername()
            return token;
        }

    }
}
