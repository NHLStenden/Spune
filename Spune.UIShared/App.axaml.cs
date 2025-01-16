//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Spune.UIShared.Views;

namespace Spune.UIShared;

/// <summary>
/// Represents the core application class that initializes and manages the application.
/// </summary>
/// <remarks>
/// The <c>App</c> class is responsible for application-wide setup, including loading XAML resources
/// and configuring the main application views based on the application lifetime.
/// </remarks>
public class App : Application
{
    /// <summary>
    /// Configures and loads the application's XAML resources during initialization.
    /// </summary>
    /// <remarks>
    /// This method is responsible for loading and applying the XAML-defined resources using the Avalonia XAML loader.
    /// It ensures that the application's UI component structure is properly defined before further processing or
    /// initialization.
    /// </remarks>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Called when the application framework has completed initialization.
    /// </summary>
    /// <remarks>
    /// This method executes post-initialization logic during application startup. It configures
    /// the application's main UI component depending on the type of application lifetime.
    /// For a classic desktop application lifetime, it sets the main window to an instance of <see cref="MainWindow" />.
    /// For a single view application lifetime, it sets the main view to an instance of <see cref="MainControl" />.
    /// This method must also call the base implementation to ensure the framework-level initialization
    /// is properly completed.
    /// </remarks>
    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new MainWindow();
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new MainControl();
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}