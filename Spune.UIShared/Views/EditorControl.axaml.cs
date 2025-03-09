//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Spune.Common.Functions;
using Spune.Core.Core;
using Spune.UIShared.Core;
using Spune.UIShared.Functions;

namespace Spune.UIShared.Views;

/// <inheritdoc />
/// <summary>This class represents a file path to image source converter for an image control.</summary>
internal class FilePathToSourceConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s || targetType != typeof(IImage)) return BindingOperations.DoNothing;
        if (string.IsNullOrEmpty(s))
            return ImageFunction.LoadNoImage();
        try
        {
            return new Bitmap(Path.Join(DefaultMasterStory.GetDirectory(), s));
        }
        catch (UnauthorizedAccessException)
        {
            return ImageFunction.LoadNoImage();
        }
        catch (FileNotFoundException)
        {
            return ImageFunction.LoadNoImage();
        }
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;
}

/// <summary>
/// Represents the editor control for the application interface.
/// </summary>
public partial class EditorControl : UserControl
{
    /// <summary>
    /// Represents the default grid margin.
    /// </summary>
    const double DefaultGridMargin = 8.0;

    /// <summary>
    /// Identifies the <see cref="SelectedInventoryItem" /> styled property.
    /// </summary>
    public static readonly StyledProperty<Interaction> SelectedInventoryItemProperty = AvaloniaProperty.Register<EditorControl, Interaction>(nameof(SelectedInventoryItem));

    /// <summary>
    /// Identifies the <see cref="SelectedChapter" /> styled property.
    /// </summary>
    public static readonly StyledProperty<Chapter> SelectedChapterProperty = AvaloniaProperty.Register<EditorControl, Chapter>(nameof(SelectedChapter));

    /// <summary>
    /// Identifies the <see cref="SelectedInteraction" /> styled property.
    /// </summary>
    public static readonly StyledProperty<Interaction> SelectedInteractionProperty = AvaloniaProperty.Register<EditorControl, Interaction>(nameof(SelectedInteraction));

    /// <summary>
    /// Current file name of the master story.
    /// </summary>
    string _currentFileName = DefaultMasterStory.GetFilePath();

    /// <summary>
    /// Represents an instance of the <see cref="MasterStory" /> class used to manage the core logic
    /// and interactions within the EditorControl.
    /// </summary>
    MasterStory _currentMasterStory = MasterStory.CreateInstance();

    /// <summary>
    /// The switch for determining the first time EndInit is called.
    /// </summary>
    bool _firstTimeEndInit = true;

    /// <summary>
    /// Represents the main user interface control in the application.
    /// This control serves as a foundational UI component, hosted within a main window or as a single view in the
    /// application lifecycle.
    /// </summary>
    public EditorControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the selected inventory item.
    /// </summary>
    /// <value>The selected inventory item.</value>
    public Interaction SelectedInventoryItem
    {
        get => GetValue(SelectedInventoryItemProperty);
        set => SetValue(SelectedInventoryItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected chapter.
    /// </summary>
    /// <value>The selected chapter.</value>
    public Chapter SelectedChapter
    {
        get => GetValue(SelectedChapterProperty);
        set => SetValue(SelectedChapterProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected interaction.
    /// </summary>
    /// <value>The selected interaction.</value>
    public Interaction SelectedInteraction
    {
        get => GetValue(SelectedInteractionProperty);
        set => SetValue(SelectedInteractionProperty, value);
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
        Dispatcher.UIThread.InvokeAsync(async () => await InitializeAsync());
    }

    /// <summary>
    /// Asynchronously initializes the UI.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task InitializeAsync()
    {
        // Set up the UI
        InventoryItemListBox.SelectionChanged +=
            (_, _) => SelectedInventoryItem = InventoryItemListBox.SelectedItem as Interaction ?? new Interaction();

        ChapterListBox.SelectionChanged += (_, _) =>
        {
            SelectedChapter = ChapterListBox.SelectedItem as Chapter ?? new Chapter();
            if (SelectedChapter.Interactions.Count > 0)
                InteractionListBox.SelectedIndex = 0;
            else
                InteractionListBox.SelectedItem = null;

            // Scroll to top
            Dispatcher.UIThread.Invoke(() => SelectedChapterScrollViewer.ScrollToHome(), DispatcherPriority.Background);
        };

        InteractionListBox.SelectionChanged +=
            (_, _) => SelectedInteraction = InteractionListBox.SelectedItem as Interaction ?? new Interaction();
        InteractionTypeComboBox.ItemsSource = Enum.GetValues<InteractionType>();

        HandleCanvasToInteraction();

        // Load the master stories
        var filePath = DefaultMasterStory.GetDirectory();
        var masterStories = new MasterStories { FilePath = filePath };
        await masterStories.LoadAsync();
        var items = masterStories.Items;
        if (items.Count == 0)
        {
            _currentFileName = DefaultMasterStory.GetFilePath();
            await LoadMasterStoryAsync();
        }
        else
        {
            CreateMasterStoryListBox(items);
        }
    }

    /// <summary>
    /// Handle the canvas pointer pressed/released to draw an interaction.
    /// </summary>
    void HandleCanvasToInteraction()
    {
        var canvas = ChapterCanvas;

        var pointerIsDown = false;
        var startPoint = new Point();
        canvas.PointerPressed += (_, e) =>
        {
            if (pointerIsDown)
                return;
            startPoint = e.GetPosition(canvas);
            startPoint = new Point(startPoint.X / canvas.Bounds.Width, startPoint.Y / canvas.Bounds.Height);
            pointerIsDown = true;
        };
        canvas.PointerReleased += (_, e) =>
        {
            if (!pointerIsDown)
                return;
            var point = e.GetPosition(canvas);
            point = new Point(point.X / canvas.Bounds.Width, point.Y / canvas.Bounds.Height);
            var width = Math.Abs(point.X - startPoint.X);
            var height = Math.Abs(point.Y - startPoint.Y);
            var xPosition = Math.Min(startPoint.X, point.X);
            var yPosition = Math.Min(startPoint.Y, point.Y);
            var interaction = new Interaction
            {
                XPosition = xPosition,
                YPosition = yPosition,
                Width = width,
                Height = height,
                Identifier = Invariant($"Interaction{SelectedChapter.Interactions.Count + 1}"),
                Text = Invariant($"Interaction {SelectedChapter.Interactions.Count + 1}"),
                TextIsVisible = false
            };
            SelectedChapter.Interactions.Add(interaction);
            pointerIsDown = false;
        };
    }

    /// <summary>
    /// Loads the master story asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task LoadMasterStoryAsync()
    {
        _currentMasterStory.Dispose();
        _currentMasterStory = await MasterStoryReaderWriter.ReadMasterStoryAsync(_currentFileName);
        DataContext = _currentMasterStory;

        if (_currentMasterStory.InventoryItems.Count > 0)
        {
            InventoryItemListBox.SelectedIndex = 0;
        }
        else
        {
            InventoryItemListBox.SelectedItem = null;
        }
        if (_currentMasterStory.Chapters.Count > 0)
        {
            ChapterListBox.SelectedIndex = 0;
            if (_currentMasterStory.Chapters[0].Interactions.Count > 0)
                InteractionListBox.SelectedIndex = 0;
            else
                InteractionListBox.SelectedItem = null;
        }
        else
        {
            ChapterListBox.SelectedItem = null;
        }
    }

    /// <summary>
    /// Creates a master story list box for the given master stories.
    /// </summary>
    /// <param name="shortMasterStories">The master stories.</param>
    void CreateMasterStoryListBox(ObservableCollection<ShortMasterStory> shortMasterStories)
    {
        var masterStoriesGrid = new Grid
        {
            RowDefinitions =
            [
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            ]
        };

        var textBlock = new TextBlock { Margin = new Thickness(0.0, 0.0, 0.0, DefaultGridMargin), Text = "Master story" };
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
            EditorGrid.IsVisible = true;
            if (l.SelectedItem is not ShortMasterStory selectedItem)
                return;

            _currentFileName = selectedItem.GetCombinedFileName();
            await LoadMasterStoryAsync();
        };
        var buttonGrid = new Grid();
        Grid.SetRow(buttonGrid, 2);
        var buttonStackPanel = new StackPanel
            { Margin = new Thickness(0.0, 12.0, 0.0, 0.0), Orientation = Orientation.Vertical, Spacing = 12.0 };
        var masterStoryTextTextBox = new TextBox { DataContext = masterStoriesGrid, Watermark = "Title" };
        buttonStackPanel.Children.Add(masterStoryTextTextBox);
        var addButton = new Button { Content = "+", DataContext = masterStoryTextTextBox };
        addButton.Click += AddMasterStoryButtonClick;
        buttonStackPanel.Children.Add(addButton);
        buttonGrid.Children.Add(buttonStackPanel);
        masterStoriesGrid.Children.Add(textBlock);
        masterStoriesGrid.Children.Add(listBox);
        masterStoriesGrid.Children.Add(buttonGrid);
        MainGrid.Children.Add(masterStoriesGrid);
    }

    /// <inheritdoc />
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _currentMasterStory.Dispose();
    }

    /// <summary>
    /// Add master story button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    async void AddMasterStoryButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: TextBox { DataContext: Grid storiesGrid } masterStoryTextTextBox } ||
            string.IsNullOrEmpty(masterStoryTextTextBox.Text))
        {
            return;
        }

        _currentFileName = MasterStoryReaderWriter.CreateMasterStory(DefaultMasterStory.GetDirectory(), masterStoryTextTextBox.Text);

        MainGrid.Children.Remove(storiesGrid);
        EditorGrid.IsVisible = true;

        await LoadMasterStoryAsync();
    }

    /// <summary>
    /// Save master story button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void SaveMasterStoryButtonClick(object? sender, RoutedEventArgs e) => MasterStoryReaderWriter.WriteMasterStory(_currentFileName, _currentMasterStory);

    /// <summary>
    /// Add inventory item button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void AddInventoryItemButtonClick(object? sender, RoutedEventArgs e)
    {
        var inventoryItem = new Interaction { Identifier = InventoryItemIdentifierTextBox.Text ?? "", IsInventory = true, SetsResult = true };
        _currentMasterStory.InventoryItems.Add(inventoryItem);
    }

    /// <summary>
    /// Delete inventory item button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void DeleteInventoryItemButtonClick(object? sender, RoutedEventArgs e)
    {
        if (InventoryItemListBox.SelectedIndex < 0 || InventoryItemListBox.SelectedIndex >= _currentMasterStory.InventoryItems.Count)
            return;

        var index = InventoryItemListBox.SelectedIndex;
        _currentMasterStory.InventoryItems.RemoveAt(index);
    }

    /// <summary>
    /// Move up inventory item button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void MoveUpInventoryItemButtonClick(object? sender, RoutedEventArgs e)
    {
        if (InventoryItemListBox.SelectedIndex <= 0 || InventoryItemListBox.SelectedIndex >= _currentMasterStory.InventoryItems.Count)
            return;

        var inventoryItem = _currentMasterStory.InventoryItems[InventoryItemListBox.SelectedIndex];
        var index = InventoryItemListBox.SelectedIndex;
        _currentMasterStory.InventoryItems.RemoveAt(index);
        index--;
        if (index < 0)
            index = 0;
        _currentMasterStory.InventoryItems.Insert(index, inventoryItem);
        InventoryItemListBox.SelectedIndex = index;
    }

    /// <summary>
    /// Move down inventory item button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void MoveDownInventoryItemButtonClick(object? sender, RoutedEventArgs e)
    {
        if (InventoryItemListBox.SelectedIndex < 0 || InventoryItemListBox.SelectedIndex >= _currentMasterStory.InventoryItems.Count - 1)
            return;

        var inventoryItem = _currentMasterStory.InventoryItems[InventoryItemListBox.SelectedIndex];
        var index = InventoryItemListBox.SelectedIndex;
        _currentMasterStory.InventoryItems.RemoveAt(index);
        index++;
        if (index > _currentMasterStory.InventoryItems.Count)
            index = _currentMasterStory.InventoryItems.Count - 1;
        _currentMasterStory.InventoryItems.Insert(index, inventoryItem);
        InventoryItemListBox.SelectedIndex = index;
    }

    /// <summary>
    /// Add chapter button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void AddChapterButtonClick(object? sender, RoutedEventArgs e)
    {
        var chapter = new Chapter { Identifier = ChapterIdentifierTextBox.Text ?? "" };
        _currentMasterStory.Chapters.Add(chapter);
    }

    /// <summary>
    /// Clone chapter button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void CloneChapterButtonClick(object? sender, RoutedEventArgs e)
    {
        if (ChapterListBox.SelectedIndex < 0 || ChapterListBox.SelectedIndex >= _currentMasterStory.Chapters.Count)
            return;
        if (ChapterListBox.SelectedItem is not Chapter item) return;
        var clone = ObjectFunction.Clone(item);
        _currentMasterStory.Chapters.Add(clone);
    }

    /// <summary>
    /// Delete chapter button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void DeleteChapterButtonClick(object? sender, RoutedEventArgs e)
    {
        if (ChapterListBox.SelectedIndex < 0 || ChapterListBox.SelectedIndex >= _currentMasterStory.Chapters.Count)
            return;
        var index = ChapterListBox.SelectedIndex;
        _currentMasterStory.Chapters.RemoveAt(index);
    }

    /// <summary>
    /// Move up chapter button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void MoveUpChapterButtonClick(object? sender, RoutedEventArgs e)
    {
        if (ChapterListBox.SelectedIndex <= 0 || ChapterListBox.SelectedIndex >= _currentMasterStory.Chapters.Count)
            return;

        var chapter = _currentMasterStory.Chapters[ChapterListBox.SelectedIndex];
        var index = ChapterListBox.SelectedIndex;
        _currentMasterStory.Chapters.RemoveAt(index);
        index--;
        if (index < 0)
            index = 0;
        _currentMasterStory.Chapters.Insert(index, chapter);
        ChapterListBox.SelectedIndex = index;
    }

    /// <summary>
    /// Move down chapter button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void MoveDownChapterButtonClick(object? sender, RoutedEventArgs e)
    {
        if (ChapterListBox.SelectedIndex < 0 || ChapterListBox.SelectedIndex >= _currentMasterStory.Chapters.Count - 1)
            return;

        var chapter = _currentMasterStory.Chapters[ChapterListBox.SelectedIndex];
        var index = ChapterListBox.SelectedIndex;
        _currentMasterStory.Chapters.RemoveAt(index);
        index++;
        if (index > _currentMasterStory.Chapters.Count)
            index = _currentMasterStory.Chapters.Count - 1;
        _currentMasterStory.Chapters.Insert(index, chapter);
        ChapterListBox.SelectedIndex = index;
    }

    /// <summary>
    /// Add interaction button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void AddInteractionButtonClick(object? sender, RoutedEventArgs e)
    {
        var interaction = new Interaction { Identifier = InteractionIdentifierTextBox.Text ?? "" };
        SelectedChapter.Interactions.Add(interaction);
    }

    /// <summary>
    /// Delete interaction button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void DeleteInteractionButtonClick(object? sender, RoutedEventArgs e)
    {
        if (InteractionListBox.SelectedIndex < 0 || InteractionListBox.SelectedIndex >= SelectedChapter.Interactions.Count)
            return;

        var index = InteractionListBox.SelectedIndex;
        SelectedChapter.Interactions.RemoveAt(index);
    }

    /// <summary>
    /// Move up interaction button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void MoveUpInteractionButtonClick(object? sender, RoutedEventArgs e)
    {
        if (InteractionListBox.SelectedIndex <= 0 || InteractionListBox.SelectedIndex >= SelectedChapter.Interactions.Count)
            return;

        var interaction = SelectedChapter.Interactions[InteractionListBox.SelectedIndex];
        var index = InteractionListBox.SelectedIndex;
        SelectedChapter.Interactions.RemoveAt(index);
        index--;
        if (index < 0)
            index = 0;
        SelectedChapter.Interactions.Insert(index, interaction);
        InteractionListBox.SelectedIndex = index;
    }

    /// <summary>
    /// Move down interaction button click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Arguments of the event.</param>
    void MoveDownInteractionButtonClick(object? sender, RoutedEventArgs e)
    {
        if (InteractionListBox.SelectedIndex < 0 || InteractionListBox.SelectedIndex >= SelectedChapter.Interactions.Count - 1)
            return;

        var interaction = SelectedChapter.Interactions[InteractionListBox.SelectedIndex];
        var index = InteractionListBox.SelectedIndex;
        SelectedChapter.Interactions.RemoveAt(index);
        index++;
        if (index > SelectedChapter.Interactions.Count)
            index = SelectedChapter.Interactions.Count - 1;
        SelectedChapter.Interactions.Insert(index, interaction);
        InteractionListBox.SelectedIndex = index;
    }
}