using Avalonia.Input.Platform;

namespace KNcloud.Desktop.Common;

internal class AvaUtils
{
    public static async Task<string?> GetClipboardData(Window owner)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(owner)?.Clipboard;
            if (clipboard == null)
            {
                return null;
            }

            return await clipboard.TryGetTextAsync();
        }
        catch
        {
            return null;
        }
    }

    public static async Task SetClipboardData(Visual? visual, string strData)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(visual)?.Clipboard;
            if (clipboard == null)
            {
                return;
            }

            await clipboard.SetTextAsync(strData);
        }
        catch
        {
        }
    }

    public static WindowIcon GetAppIcon(ESysProxyType sysProxyType)
    {
        var index = (int)sysProxyType + 1;
        var fileName = Utils.GetPath($"NotifyIcon{index}.ico");
        if (File.Exists(fileName))
        {
            return new(fileName);
        }

        return GetAssetIcon($"NotifyIcon{index}.ico");
    }

    public static WindowIcon GetMainWindowIcon()
    {
        return GetAssetIcon("KNcloud.ico");
    }

    private static WindowIcon GetAssetIcon(string iconName)
    {
        var uri = new Uri(Path.Combine(Global.AvaAssets, iconName));
        using var bitmap = new Bitmap(AssetLoader.Open(uri));
        return new(bitmap);
    }
}
