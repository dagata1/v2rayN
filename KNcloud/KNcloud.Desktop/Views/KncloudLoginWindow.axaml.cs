using KNcloud.Desktop.Base;

namespace KNcloud.Desktop.Views;

public partial class KncloudLoginWindow : WindowBase<KncloudLoginViewModel>
{
    public KncloudLoginResult? LoginResult => ViewModel?.LoginResult;

    public KncloudLoginWindow()
    {
        InitializeComponent();

        Loaded += Window_Loaded;
        btnCancel.Click += (s, e) => Close();

        ViewModel = new KncloudLoginViewModel(UpdateViewHandler);

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.Email, v => v.txtEmail.Text).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.Password, v => v.txtPassword.Text).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.LoginCmd, v => v.btnLogin).DisposeWith(disposables);
        });
    }

    private async Task<bool> UpdateViewHandler(EViewAction action, object? obj)
    {
        switch (action)
        {
            case EViewAction.CloseWindow:
                Close(true);
                break;
        }
        return await Task.FromResult(true);
    }

    private void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        txtEmail.Focus();
    }
}
