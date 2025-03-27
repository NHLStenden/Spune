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
        _runningStory.OnElpasedTime += RunningStoryHandlerElpasedTime;
        _runningStory.OnHideElement += RunningStoryHandlerHideElement;
        _runningStory.OnPutInInventoryElement += RunningStoryHandlerPutInInventoryElement;
        _runningStory.OnShowMessage += RunningStoryHandlerShowMessage;
        _runningStory.OnStart += RunningStoryHandlerStart;
        _runningStory.OnTimeLeft += RunningStoryHandlerTimeLeft;
        _runningStory.OnNavigateTo += RunningStoryHandlerNavigateTo;
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

        var filePath = DefaultMasterStory.GetDirectory();
        var masterStories = new MasterStories { FilePath = filePath };
        await masterStories.LoadAsync();
        var items = masterStories.Items;
        switch (items.Count)
        {
            case 0:
                await _runningStory.StartAsync();
                HideMessage();
                await CreateChapterAsync();
                break;
            case 1:
                await _runningStory.StartAsync(items[0].GetCombinedFileName());
                HideMessage();
                await CreateChapterAsync();
                break;
            default:
                CreateStoryListBox(items);
                break;
        }
    }

    /// <summary>
    /// Loads the story asynchronously for the browser.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task LoadStoryBrowserAsync()
    {
        _runningStory.Clear();
        await _runningStory.StartAsync(DefaultMasterStory.GetFilePath());
        HideMessage();
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
            HideMessage();
            await CreateChapterAsync();
        };
        masterStoriesGrid.Children.Add(textBlock);
        masterStoriesGrid.Children.Add(listBox);
        MainGrid.Children.Add(masterStoriesGrid);
    }

    /// <summary>
    /// Initializes the control from the given running story.
    /// </summary>
    /// <param name="runningStory">Running story to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InitializeFromAsync(RunningStory runningStory)
    {
        TimeTextBlock.IsVisible = runningStory.MasterStory.HasMaxDuration();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously creates a control based on the current story state
    /// and updates the user interface by setting the created control
    /// to the main grid of the application.
    /// </summary>
    async Task CreateChapterAsync()
    {
        var control = await _runningStoryView.CreateControlFromChapter();
        var currentControl = MainGrid.Children.Count > 0 ? MainGrid.Children[0] : null;
        MainGrid.Children.Add(control);
        if (currentControl != null)
        {
            currentControl.Opacity = 0.0;
            currentControl.Classes.Set("grow_and_fade_out", true);
            currentControl.PropertyChanged += (_, e) =>
            {
                if (!PropertyFunction.TryGetPropertyNewValue<double>(e, OpacityProperty, out var opacity) || opacity > 0.0)
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
    async Task RecreateChapterNoAnimAsync()
    {
        var control = await _runningStoryView.CreateControlFromChapter();
        var currentControl = MainGrid.Children.Count > 0 ? MainGrid.Children[0] : null;
        MainGrid.Children.Add(control);
        if (currentControl != null)
            MainGrid.Children.Remove(currentControl);
    }

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
            messageTask.SetResult();
            MessageGrid.DataContext = null;
            HideMessage();
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
            try
            {
                messageTask.SetResult();
                MessageGrid.DataContext = null;
                HideMessage();
            }
            catch (InvalidOperationException)
            {
            }
        }, closeDelay);
        await messageTask.Task;
    }

    /// <summary>
    /// Displays the elapsed left.
    /// <param name="d">The elapsed time in ms.</param>
    /// </summary>
    void ElapsedTime(double d)
    {
        // d = Math.Abs(d / 1000.0);
        // var remainingTimeText = !string.IsNullOrEmpty(_runningStory.MasterStory.RemainingTimeText) ?
        //     _runningStory.MasterStory.RemainingTimeText : "Remaining time: ";
        // TimeTextBlock.Text = remainingTimeText + d.ToString("0");
    }

    /// <summary>
    /// Displays the time left.
    /// <param name="d">The time left in ms.</param>
    /// </summary>
    void TimeLeft(double d)
    {
        d = Math.Abs(d / 1000.0);
        var remainingTimeText = !string.IsNullOrEmpty(_runningStory.MasterStory.RemainingTimeText) ?
            _runningStory.MasterStory.RemainingTimeText : "Remaining time: ";
        TimeTextBlock.Text = remainingTimeText + d.ToString("0");
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
    /// Handles the navigation to asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The element to navigate to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task RunningStoryHandlerNavigateTo(object sender, Element e) => await CreateChapterAsync();

    /// <summary>
    /// Handles showing a message asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The message to show.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task RunningStoryHandlerShowMessage(object sender, string e) => await ShowMessageAsync(e, HintCloseDelay);

    /// <summary>
    /// Handles the start event asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The started running story.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task RunningStoryHandlerStart(object sender, RunningStory e) => await InitializeFromAsync(e);

    /// <summary>
    /// Handles showing the elapsed time asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The message to show.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunningStoryHandlerElpasedTime(object sender, double e)
    {
        ElapsedTime(e);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles showing the time left asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The message to show.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunningStoryHandlerTimeLeft(object sender, double e)
    {
        TimeLeft(e);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles hiding an element asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The element to hide.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task RunningStoryHandlerHideElement(object sender, Element e) => await RecreateChapterNoAnimAsync();

    /// <summary>
    /// Handles putting an element in the inventory asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The element to hide.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task RunningStoryHandlerPutInInventoryElement(object sender, Element e) => await RecreateChapterNoAnimAsync();

    /// <summary>
    /// Handles the selection change event of the ChapterListBox.
    /// Navigates to the selected chapter and updates the story asynchronously.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a ListBox.</param>
    /// <param name="e">The event data containing information about the selection change.</param>
    async void ChapterListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox { SelectedItem: Chapter chapter })
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