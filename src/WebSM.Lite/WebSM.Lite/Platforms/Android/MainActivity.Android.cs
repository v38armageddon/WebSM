using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace WebSM.Lite.Droid;

[Activity(
    MainLauncher = true,
    ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
    WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden
)]
[IntentFilter(
    new[] { Android.Content.Intent.ActionView },
    Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
    DataSchemes = new[] { "http", "https" }
)]
public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
{
    const int REQUEST_CODE_BROWSER_ROLE = 1000;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        global::AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);

        base.OnCreate(savedInstanceState);

        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
        {
            var roleManager = (Android.App.Roles.RoleManager)GetSystemService(Android.Content.Context.RoleService);
            if (roleManager != null && roleManager.IsRoleAvailable(Android.App.Roles.RoleManager.RoleBrowser)
                && !roleManager.IsRoleHeld(Android.App.Roles.RoleManager.RoleBrowser))
            {
                var intent = roleManager.CreateRequestRoleIntent(Android.App.Roles.RoleManager.RoleBrowser);
                StartActivityForResult(intent, REQUEST_CODE_BROWSER_ROLE);
            }
        }
    }

}
