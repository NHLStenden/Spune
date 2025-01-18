//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Spune.Common.Functions;
using Spune.Core.Core;
using Spune.Core.Miscellaneous;
using Spune.UIShared.Core;
using Spune.UIShared.Functions;

namespace Spune.UIShared.Views;

/// <summary>
/// Represents the main control for the application interface.
/// </summary>
public partial class MainControl : UserControl
{
    /// <summary>
    /// Represents the close delay of a hint.
    /// </summary>
    const double HintCloseDelay = 6000.0;

    /// <summary>
    /// Represents the close delay of the splash screen.
    /// </summary>
    const double SplashDelay = 4000.0;

    /// <summary>
    /// The switch for determining the first time EndInit is called.
    /// </summary>
    bool _firstTimeEndInit = true;

    /// <summary>
    /// Represents an instance of the <see cref="RunningStory" /> class.
    /// </summary>
    readonly RunningStory _runningStory = new();

    /// <summary>
    /// Represents an instance of the <see cref="RunningStoryView" /> class.
    /// </summary>
    readonly RunningStoryView _runningStoryView;

    /// <summary>
    /// Represents the main user interface control in the application.
    /// This control serves as a foundational UI component, hosted within a main window or as a single view in the
    /// application lifecycle.
    /// </summary>
    public MainControl()
    {
        InitializeComponent();
        _runningStory.OnHideElement += RunningStoryHandlerHideElement;
        _runningStory.OnShowMessage += RunningStoryHandlerShowMessage;
        _runningStory.OnNavigateToLink += RunningStoryHandlerNavigateToLink;
        _runningStoryView = new RunningStoryView(_runningStory, this);
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

        if (!_firstTimeEndInit)
            return;

        _firstTimeEndInit = false;
        Dispatcher.UIThread.InvokeAsync(InitAsync);
    }

    /// <summary>
    /// Refreshes the current chapter.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RefreshChapterAsync() => await _runningStory.RefreshChapterAsync();

    /// <summary>
    /// Sets the DataContext of the ChapterListBox to the current story and makes it fully visible.
    /// </summary>
    public void SelectChapter()
    {
        ChapterListBox.DataContext = _runningStory;
        ChapterGrid.Opacity = 1.0;
        ChapterGrid.IsHitTestVisible = true;
        MainGrid.Opacity = 0.0;
        MainGrid.IsHitTestVisible = false;
    }

    /// <summary>
    /// Initializes the control asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task InitAsync()
    {
        var clientProperties = await ClientProperties.GetInstanceAsync();
        if (!clientProperties.SplashScreen)
        {
            await LoadStoryAsync();
            return;
        }

        const float scaleX = 8.0f;
        const float scaleY = 8.0f;
        await ImageFunction.SvgToPngAsync(SplashImage, scaleX, scaleY);
        Background = new SolidColorBrush(Colors.Black);
        SplashImage.Opacity = 1.0;
        await TimerFunction.DelayInvokeAsync(async () =>
        {
            SplashImage.Opacity = 0.0;
            await TimerFunction.DelayInvokeAsync(async () =>
            {
                Background = null;
                await LoadStoryAsync();
            }, SplashDelay);
        }, SplashDelay);
    }

    /// <summary>
    /// Asynchronously loads a story by reading it from a file, initializing it, and setting it up for execution.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task LoadStoryAsync()
    {
        ShowMessage("Loading...");
        await RunningStory.InitializeChatClientAsync();
        HideMessage();

        if (OperatingSystem.IsBrowser())
            await LoadStoryBrowserAsync();
        else
            await LoadStoryDesktopAsync();
    }

    /// <summary>
    /// Loads the story asynchronously for the desktop.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task LoadStoryDesktopAsync()
    {
        _runningStory.Clear();

        var masterStories = new MasterStories { FilePath = DefaultMasterStory.Directory };
        await masterStories.LoadAsync();
        var items = masterStories.Items;
        if (items.Count == 0)
        {
            await _runningStory.StartAsync();
            await CreateChapterAsync();
        }
        else if (items.Count == 1)
        {
            await _runningStory.StartAsync(items[0].GetCombinedFileName());
            await CreateChapterAsync();
        }
        else
        {
            CreateStoryListBox(items);
        }
    }

    /// <summary>
    /// Loads the story asynchronously for the browser.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task LoadStoryBrowserAsync()
    {
        _runningStory.Clear();
        await _runningStory.StartAsync(DefaultMasterStory.FilePath);
        await CreateChapterAsync();
    }

    /// <summary>
    /// Creates a story list box for the given master stories.
    /// </summary>
    /// <param name="shortMasterStories">The master stories.</param>
    void CreateStoryListBox(ObservableCollection<ShortMasterStory> shortMasterStories)
    {
        var masterStoriesGrid = new Grid
        {
            RowDefinitions =
            [
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            ]
        };

        var textBlock = new TextBlock { Margin = new Thickness(0.0, 0.0, 0.0, RunningStoryView.DefaultGridMargin), Text = "Master story" };
        Grid.SetRow(textBlock, 0);
        var listBox = new ListBox
        {
            ItemsSource = shortMasterStories,
            ItemTemplate = new FuncDataTemplate<object>((_, _) => new TextBlock
            { [!TextBlock.TextProperty] = new Binding("Title") })
        };
        Grid.SetRow(listBox, 1);
        listBox.SelectionChanged += async (s, _) =>
        {
            if (s is not ListBox l)
                return;
            MainGrid.Children.Remove(masterStoriesGrid);
            if (l.SelectedItem is not ShortMasterStory selectedItem)
                return;
            await _runningStory.StartAsync(selectedItem.GetCombinedFileName());
            await CreateChapterAsync();
        };
        masterStoriesGrid.Children.Add(textBlock);
        masterStoriesGrid.Children.Add(listBox);
        MainGrid.Children.Add(masterStoriesGrid);
    }

    /// <summary>
    /// Asynchronously creates a control based on the current story state
    /// and updates the user interface by setting the created control
    /// to the main grid of the application.
    /// </summary>
    async Task CreateChapterAsync()
    {
        var control = await _runningStoryView.CreateControlFromChapter(null);
        var currentControl = MainGrid.Children.Count > 0 ? MainGrid.Children[0] : null;
        MainGrid.Children.Add(control);
        if (currentControl != null)
        {
            currentControl.Opacity = 0.0;
            currentControl.Classes.Set("grow_and_fade_out", true);
            currentControl.PropertyChanged += (_, e) =>
            {
                if (!string.Equals(e.Property.Name, "Opacity", StringComparison.Ordinal) || e.NewValue is not double opacity || opacity > 0.0)
                    return;

                MainGrid.Children.RemoveAt(0);
                control.Classes.Set("fade_in", false);
            };
        }

        control.Opacity = 1.0;
        control.Classes.Set("fade_in", true);
    }

    /// <summary>
    /// Asynchronously creates a control based on the current story state
    /// and updates the user interface by setting the created control
    /// to the main grid of the application. There's no animation displayed.
    /// </summary>
    async Task RecreateChapterNoAnimAsync() => await _runningStoryView.CreateControlFromChapter(MainGrid.Children.Count > 0 ? MainGrid.Children[0] : null);

    /// <inheritdoc />
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _runningStory.Clear();
    }

    /// <summary>
    /// Message grid pointer click event: hide the message.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    void MessageGridPointerClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (MessageGrid.DataContext is not TaskCompletionSource messageTask)
                return;
            HideMessage();
            messageTask.SetResult();
            MessageGrid.DataContext = null;
        }
        catch (InvalidOperationException)
        {
        }
    }

    /// <summary>
    /// Displays a message to the user in a dedicated UI area for a specific duration.
    /// <param name="message">The message text to be displayed.</param>
    /// <param name="closeDelay">The delay in ms to close the message automatically.</param>
    /// </summary>
    void ShowMessage(string message, double closeDelay = 0.0)
    {
        MessageTextBlock.Text = message;
        MessageGrid.Opacity = 0.85;
        MessageGrid.IsHitTestVisible = true;
        if (closeDelay > 0.0)
            TimerFunction.DelayInvoke(HideMessage, closeDelay);
    }

    /// <summary>
    /// Displays a message to the user in a dedicated UI area for a specific duration.
    /// <param name="message">The message text to be displayed.</param>
    /// <param name="closeDelay">The delay in ms to close the message automatically.</param>
    /// </summary>
    async Task ShowMessageAsync(string message, double closeDelay = 0.0)
    {
        MessageTextBlock.Text = message;
        MessageGrid.Opacity = 0.85;
        MessageGrid.IsHitTestVisible = true;

        if (closeDelay <= 0.0)
            return;

        var messageTask = new TaskCompletionSource();
        MessageGrid.DataContext = messageTask;
        TimerFunction.DelayInvoke(() =>
        {
            HideMessage();
            try
            {
                messageTask.SetResult();
                MessageGrid.DataContext = null;
            }
            catch (InvalidOperationException)
            {
            }
        }, closeDelay);
        await messageTask.Task;
    }

    /// <summary>
    /// Hides the message.
    /// </summary>
    void HideMessage()
    {
        MessageGrid.IsHitTestVisible = false;
        MessageGrid.Opacity = 0.0;
    }

    /// <summary>
    /// Handles the navigation to a link asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The element to navigate to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task RunningStoryHandlerNavigateToLink(object sender, Element e) => await CreateChapterAsync();

    /// <summary>
    /// Handles showing a message asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The message to show.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task RunningStoryHandlerShowMessage(object sender, string e) => await ShowMessageAsync(e, HintCloseDelay);

    /// <summary>
    /// Handles hiding an element asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The element to hide.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task RunningStoryHandlerHideElement(object sender, Element e) => await RecreateChapterNoAnimAsync();

    /// <summary>
    /// Handles the selection change event of the ChapterListBox.
    /// Navigates to the selected chapter and updates the story asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a ListBox.</param>
    /// <param name="e">The event data containing information about the selection change.</param>
    async void ChapterListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox || listBox.SelectedItem is not Chapter chapter)
            return;

        MainGrid.IsHitTestVisible = true;
        MainGrid.Opacity = 1.0;
        ChapterGrid.IsHitTestVisible = false;
        ChapterGrid.Opacity = 0.0;
        if (!await _runningStory.NavigateToIdentifierAsync(chapter))
            return;
        await CreateChapterAsync();
    }
}