﻿using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Auth;
using Firebase.Auth.Providers;
using WerewolfClient;
using WerewolfClient.Models;
using WerewolfClient.Services;

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
            var result = await client.SignInWithEmailAndPasswordAsync(email, password);
            string idToken = await result.User.GetIdTokenAsync();

             // Lấy username từ Firebase
            string userId = result.User.Uid;
            string username = await FirebaseHelper.GetUsernameAsync(userId, idToken);

            // Lưu thông tin user sau khi đăng nhập thành công
             CurrentUserManager.CurrentUser = new WerewolfClient.Models.UserInfo
            {
                Id = userId,
                Email = email,
                IdToken = idToken,
                Username = username
            };
            return true;
        }
        catch (FirebaseAuthException ex)
        {
            MessageBox.Show($"Lỗi đăng nhập: {ex.Reason}");
            return false;
        }
    }

    public static async Task<bool> RegisterAsync(string email, string password, string username)
    {
        try
        {
            var client = GetAuthClient();
            var result = await client.CreateUserWithEmailAndPasswordAsync(email, password);
            string userId = result.User.Uid;
            string idToken = await result.User.GetIdTokenAsync();
             await FirebaseHelper.SaveUsernameAsync(userId, username, email, idToken);
            return true;
        }
        catch (FirebaseAuthException ex)
        {
            MessageBox.Show($"Lỗi đăng ký: {ex.Reason}");
            return false;
        }
    }
}