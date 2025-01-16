//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace Spune.UIShared.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    /// <summary>
    /// Called before the Avalonia.Input.InputElement.KeyDown event occurs.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.Key)
        {
            case Key.F5 when Content is MainControl mainControl1:
                Dispatcher.UIThread.InvokeAsync(async () => await mainControl1.RefreshChapterAsync());
                break;
            case Key.F9 when Content is EditorControl:
                Content = new MainControl();
                break;
            case Key.F9 when Content is MainControl:
                Content = new EditorControl();
                break;
            case Key.N when e.KeyModifiers is KeyModifiers.Control or KeyModifiers.Meta &&
                            Content is MainControl mainControl2:
                mainControl2.SelectChapter();
                break;
        }
    }

    /// <summary>
    /// Completes the initialization process for the control.
    /// This method ensures any necessary final setup tasks are executed after the component
    /// is fully initialized, such as invoking asynchronous operations to prepare the UI or
    /// load required resources.
    /// </summary>
    public override void EndInit()
    {
        base.EndInit();

        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && args[1] == "edit")
            Content = new EditorControl();
        else
            Content = new MainControl();
    }
}