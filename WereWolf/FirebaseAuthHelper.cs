using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Auth;
using Firebase.Auth.Providers;
using WereWolf;

public static class FirebaseAuthHelper
{
    private static FirebaseAuthClient _authClient;

    public static FirebaseAuthClient GetAuthClient()
    {
        if (_authClient == null)
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = AppConfig.FirebaseApiKey,
                AuthDomain = AppConfig.AuthDomain,
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider() // Bật đăng nhập bằng email/password
                }
            };
            _authClient = new FirebaseAuthClient(config);
        }
        return _authClient;
    }

    public static async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var client = GetAuthClient();
            await client.SignInWithEmailAndPasswordAsync(email, password);
            return true;
        }
        catch (FirebaseAuthException ex)
        {
            MessageBox.Show($"Lỗi đăng nhập: {ex.Reason}");
            return false;
        }
    }

    public static async Task<bool> RegisterAsync(string email, string password)
    {
        try
        {
            var client = GetAuthClient();
            await client.CreateUserWithEmailAndPasswordAsync(email, password);
            return true;
        }
        catch (FirebaseAuthException ex)
        {
            MessageBox.Show($"Lỗi đăng ký: {ex.Reason}");
            return false;
        }
    }
}