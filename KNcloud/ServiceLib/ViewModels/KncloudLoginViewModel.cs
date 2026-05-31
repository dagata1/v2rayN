namespace ServiceLib.ViewModels;

public class KncloudLoginViewModel : MyReactiveObject
{
    private readonly KncloudAuthService _authService = new();

    [Reactive]
    public string Email { get; set; } = string.Empty;

    [Reactive]
    public string Password { get; set; } = string.Empty;

    [Reactive]
    public bool IsBusy { get; set; }

    public KncloudLoginResult? LoginResult { get; private set; }

    public ReactiveCommand<Unit, Unit> LoginCmd { get; }

    public KncloudLoginViewModel(Func<EViewAction, object?, Task<bool>>? updateView)
    {
        _updateView = updateView;

        var canLogin = this.WhenAnyValue(
            x => x.Email,
            x => x.Password,
            x => x.IsBusy,
            (email, password, isBusy) => !isBusy && email.IsNotEmpty() && password.IsNotEmpty());

        LoginCmd = ReactiveCommand.CreateFromTask(LoginAsync, canLogin);
    }

    private async Task LoginAsync()
    {
        IsBusy = true;
        try
        {
            var ret = await _authService.LoginAsync(Email.TrimEx(), Password);
            if (!ret.Success)
            {
                NoticeManager.Instance.Enqueue(ret.Msg ?? ResUI.OperationFailed);
                return;
            }

            LoginResult = ret.Data as KncloudLoginResult;
            NoticeManager.Instance.Enqueue(ret.Msg ?? ResUI.OperationSuccess);
            await _updateView?.Invoke(EViewAction.CloseWindow, null);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
