using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace InventorySystemv2;

/// <summary>
///     App-klassens ansvar er at:
///     1) loade XAML-ressourcer (tema, styles, etc.)
///     2) oprette og vise MainWindow ved desktop-livstid
/// </summary>
public class App : Application
{
    /// <summary>
    ///     Indlæser App.axaml (ressourcer, styles mv.).
    ///     Kaldes meget tidligt i app-livscyklussen.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    ///     Når frameworket er klar, opretter vi hovedvinduet.
    ///     Vi bruger IClassicDesktopStyleApplicationLifetime, som er standard for desktop-apps.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            // Sæt dit primære vindue.
            // MainWindow constructoren sætter DataContext og laver testdata,
            // så GUI’en er klar med det samme.
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}