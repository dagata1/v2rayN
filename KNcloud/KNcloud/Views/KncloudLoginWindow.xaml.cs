namespace KNcloud.Views;

public partial class KncloudLoginWindow
{
    public KncloudLoginResult? LoginResult => ViewModel?.LoginResult;

    public KncloudLoginWindow()
    {
        InitializeComponent();

        Owner = Application.Current.MainWindow;
        Loaded += Window_Loaded;
        txtPassword.PasswordChanged += TxtPassword_PasswordChanged;
        btnForgetPassword.Click += BtnForgetPassword_Click;

        ViewModel = new KncloudLoginViewModel(UpdateViewHandler);

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.Email, v => v.txtEmail.Text).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.LoginCmd, v => v.btnLogin).DisposeWith(disposables);
        });
        WindowsUtils.SetDarkBorder(this, AppManager.Instance.Config.UiItem.CurrentTheme);
    }

    private async Task<bool> UpdateViewHandler(EViewAction action, object? obj)
    {
        switch (action)
        {
            case EViewAction.CloseWindow:
                DialogResult = true;
                break;
        }
        return await Task.FromResult(true);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        txtEmail.Focus();
    }

    private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.Password = txtPassword.Password;
        }
    }

    private async void BtnForgetPassword_Click(object sender, RoutedEventArgs e)
    {
        ProcUtils.ProcessStart(await ServiceLib.Services.KncloudAuthService.GetForgetPasswordUrlAsync());
    }
}
