//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Spune.Common.Converters;
using Spune.Common.Extensions;
using Spune.Common.Functions;
using Spune.Core.Core;

namespace Spune.UIShared.Views;

/// <summary>
/// This class represents the view layer for the running story.
/// </summary>
/// <param name="runningStory">The running story instance.</param>
/// <param name="resourceHost">The resource host.</param>
public class RunningStoryView(RunningStory runningStory, IResourceHost resourceHost)
{
    /// <summary>
    /// Represents the mathematical constant for the golden ratio.
    /// </summary>
    const double GoldenRatio = 1.0 / 1.618033988749895d;

    /// <summary>
    /// Represents the default grid margin.
    /// </summary>
    public static double DefaultGridMargin => 8.0;

    /// <summary>
    /// The running story member.
    /// </summary>
    readonly RunningStory _runningStory = runningStory;

    /// <summary>
    /// The resource host member.
    /// </summary>
    readonly IResourceHost _resourceHost = resourceHost;

    /// <summary>
    /// Asynchronously creates a control based on the current state of the story or its current chapter.
    /// Handles the creation of UI components like media, text, and interactions for chapters when applicable.
    /// </summary>
    /// <returns>A <see cref="Control" /> representing the generated UI for the story or its chapter.</returns>
    public async Task<Control> CreateControlFromChapter()
    {
        var chapter = _runningStory.GetChapter();

        CreatePanelAndGrid(chapter, out var storyPanel, out var chapterGrid);
        if (chapter == null) return storyPanel;

        if (chapter.CloseDelay > 0.0)
            await TimerFunction.DelayInvokeAsync(async () => await _runningStory.HandleElementAsync(chapter, null), (int)chapter.CloseDelay);

        var imageIndex = chapter.Media.HasImage() ? 0 : -1;
        var textIndex = chapter.HasText() ? chapter.Media.HasImage() ? 1 : 0 : -1;
        var interactionIndex = _runningStory.GetInteractions().Where(x => !x.InImage()).ToList().Count > 0 ? 2 : -1;

        if (imageIndex >= 0)
            await CreateMediaAsync(chapter, chapterGrid, imageIndex);
        if (textIndex >= 0)
            await CreateTextAsync(chapter, chapterGrid, imageIndex >= 0, textIndex);
        if (_runningStory.GetInteractions(chapter).Where(x => !x.InImage()).ToList().Count > 0)
            await CreateInteractionsAsync(chapter, chapterGrid, interactionIndex);

        return storyPanel;
    }

    /// <summary>
    /// Creates and adds an image derived from the chapter's media to the specified grid at the given row index.
    /// </summary>
    /// <param name="chapter">The chapter containing the media to be displayed as an image.</param>
    /// <param name="chapterGrid">The grid to which the created image will be added.</param>
    /// <param name="imageIndex">The row index in the grid where the image will be placed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task CreateMediaAsync(Chapter chapter, Grid chapterGrid, int imageIndex)
    {
        await using var stream = await chapter.Media.GetImageStreamAsync();
        var bitmap = new Bitmap(stream);
        var image = new Image
        {
            Source = bitmap,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0.0, 0.0, 0.0, DefaultGridMargin)
        };
        Grid.SetRow(image, imageIndex);
        chapterGrid.Children.Add(image);
        if (chapter.CloseDelay > 0.0)
            ControlExtensions.SetPointerClick(image, (_, _) => Dispatcher.UIThread.InvokeAsync(async () => await _runningStory.HandleElementAsync(chapter, null)));

        var interactions = _runningStory.GetInteractions(chapter).Where(x => x.InImage()).ToList();
        if (interactions.Count == 0)
            return;

        var canvas = new Canvas
        {
            Background = new SolidColorBrush(Colors.Transparent),
            Margin = new Thickness(0.0, 0.0, 0.0, DefaultGridMargin)
        };
        var binding = new Binding("Bounds.Width") { Source = image };
        canvas.Bind(Layoutable.WidthProperty, binding);
        binding = new Binding("Bounds.Height") { Source = image };
        canvas.Bind(Layoutable.HeightProperty, binding);
        Grid.SetRow(canvas, imageIndex);
        chapterGrid.Children.Add(canvas);

        foreach (var interaction in interactions)
            await CreateImageShapeAsync(canvas, interaction);
    }

    /// <summary>
    /// Creates a shape (interaction) for the image.
    /// </summary>
    /// <param name="canvas">Canvas to add shape to.</param>
    /// <param name="interaction">Interaction to assign to the shape.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task CreateImageShapeAsync(Canvas canvas, Interaction interaction)
    {
        // From Fluent (as used in a button):
        // <CornerRadius x:Key="ControlCornerRadius">3</CornerRadius>
        if (interaction.HasText() && interaction.TextIsVisible)
        {
            var panel = new Panel();
            canvas.Children.Add(panel);
            panel.Bind(Layoutable.WidthProperty, new Binding("Bounds.Width") { Source = canvas, Converter = OneWayConverter<double, double>.GetInstance((d, _, _) => d * interaction.Width) });
            panel.Bind(Layoutable.HeightProperty, new Binding("Bounds.Height") { Source = canvas, Converter = OneWayConverter<double, double>.GetInstance((d, _, _) => d * interaction.Height) });
            var multiBinding1 = new MultiBinding
            {
                Converter = MultiOneWayConverter<Thickness>.GetInstance((x, _, _) => x is [double width, double height] ? new Thickness(width * interaction.XPosition, height * interaction.YPosition, 0, 0) : new Thickness(0.0, 0.0, 0.0, 0.0))
            };
            multiBinding1.Bindings.Add(new Binding("Bounds.Width") { Source = canvas });
            multiBinding1.Bindings.Add(new Binding("Bounds.Height") { Source = canvas });
            panel.Bind(Layoutable.MarginProperty, multiBinding1);
            var textBlock = new TextBlock
            {
                Background = new SolidColorBrush(new Color(0x60, 0x00, 0x00, 0x00)),
                // Standard font size is 14 (with Fluent). This is 1.25 * 14
                FontSize = 17.5,
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(2.0, 2.0, 2.0, 2.0),
                Text = await _runningStory.GetTextAsync(interaction),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            panel.Children.Add(textBlock);
        }
        const double cornerRadius = 3.0;
        var color = _resourceHost.FindResource("SystemSecondaryAccentColor") is Color c ? c : Colors.White;
        var shape = new Rectangle
        { Fill = new SolidColorBrush(color), Opacity = 0.0, RadiusX = cornerRadius, RadiusY = cornerRadius };
        var pointerIsDown = false;
        shape.PointerPressed += (_, _) => pointerIsDown = true;
        ControlExtensions.SetPointerClick(shape, (_, _) => ShapeClick(interaction, shape, ref pointerIsDown));
        canvas.Children.Add(shape);

        shape.Bind(Layoutable.WidthProperty, new Binding("Bounds.Width") { Source = canvas, Converter = OneWayConverter<double, double>.GetInstance((d, _, _) => d * interaction.Width) });
        shape.Bind(Layoutable.HeightProperty, new Binding("Bounds.Height") { Source = canvas, Converter = OneWayConverter<double, double>.GetInstance((d, _, _) => d * interaction.Height) });
        var multiBinding = new MultiBinding
        {
            Converter = MultiOneWayConverter<Thickness>.GetInstance((x, _, _) =>
                x is [double width, double height] ? new Thickness(width * interaction.XPosition, height * interaction.YPosition, 0, 0) : new Thickness(0.0, 0.0, 0.0, 0.0))
        };
        multiBinding.Bindings.Add(new Binding("Bounds.Width") { Source = canvas });
        multiBinding.Bindings.Add(new Binding("Bounds.Height") { Source = canvas });
        shape.Bind(Layoutable.MarginProperty, multiBinding);
    }

    /// <summary>
    /// Handles the click of a shape in the image.
    /// </summary>
    /// <param name="interaction">Interaction coupled to the shape.</param>
    /// <param name="shape">The shape itself.</param>
    /// <param name="pointerIsDown">The state of the pointer.</param>
    void ShapeClick(Interaction interaction, Rectangle shape, ref bool pointerIsDown)
    {
        if (!pointerIsDown)
            return;
        shape.Classes.Set("click_animation", true);
        shape.Opacity = 0.0;
        shape.PropertyChanged += async (_, e) =>
        {
            if (!PropertyFunction.TryGetPropertyNewValue<double>(e, Visual.OpacityProperty, out var opacity) || opacity > 0.0)
                return;
            shape.Classes.Set("click_animation", false);
            await _runningStory.HandleElementAsync(interaction, null);
        };
        pointerIsDown = false;
    }

    /// <summary>
    /// Asynchronously creates a text block for the given chapter and adds it to the specified grid at the specified index.
    /// </summary>
    /// <param name="chapter">The chapter containing the text to be displayed.</param>
    /// <param name="chapterGrid">The grid where the text block will be added.</param>
    /// <param name="hasImage">Switch to indicate if the text block also has an image (above).</param>
    /// <param name="textIndex">The index in the grid's row definitions where the text block will be placed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task CreateTextAsync(Chapter chapter, Grid chapterGrid, bool hasImage, int textIndex)
    {
        var textBlock = new TextBlock
        {
            // Standard font size is 14 (with Fluent). This is 1.5 * 14
            FontSize = 21,
            Text = await _runningStory.GetTextAsync(chapter),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        if (!hasImage)
            textBlock.VerticalAlignment = VerticalAlignment.Center;

        if (chapter.CloseDelay > 0.0)
            ControlExtensions.SetPointerClick(textBlock, (_, _) => Dispatcher.UIThread.InvokeAsync(async () => await _runningStory.HandleElementAsync(chapter, null)));

        Grid.SetRow(textBlock, textIndex);
        chapterGrid.Children.Add(textBlock);
    }

    /// <summary>
    /// Creates and arranges interactions within the specified chapter and grid, adding them to the given row index.
    /// </summary>
    /// <param name="chapter">The chapter containing the interactions to be created.</param>
    /// <param name="chapterGrid">The grid where the interactions will be added.</param>
    /// <param name="interactionIndex">The row index within the grid where the interactions will be placed.</param>
    async Task CreateInteractionsAsync(Chapter chapter, Grid chapterGrid, int interactionIndex)
    {
        var interactionsGrid = new Grid();
        Grid.SetRow(interactionsGrid, interactionIndex);
        chapterGrid.Children.Add(interactionsGrid);

        var columnDefinitions = new ColumnDefinitions();
        var interactions = _runningStory.GetInteractions(chapter).Where(x => !x.InImage()).ToList();
        if (chapter.RandomizeInteractions)
            interactions.Shuffle();

        for (var i = 0; i < interactions.Count; i++)
            columnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        interactionsGrid.ColumnDefinitions = columnDefinitions;

        var interactionCounter = 0;
        foreach (var interaction in interactions)
        {
            await CreateInteractionAsync(interactionsGrid, interactionCounter, interaction);
            interactionCounter++;
        }
    }

    /// <summary>
    /// Asynchronously creates an interaction of a specified type and adds it to the interactions grid.
    /// </summary>
    /// <param name="interactionsGrid">The <see cref="Grid" /> where the interaction will be added.</param>
    /// <param name="counter">The current interaction counter used to track and differentiate interactions.</param>
    /// <param name="interaction">
    /// The <see cref="Interaction" /> object defining the type and details of the interaction to be
    /// created.
    /// </param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    async Task CreateInteractionAsync(Grid interactionsGrid, int counter, Interaction interaction)
    {
        switch (interaction.Type)
        {
            case InteractionType.MultiSelect:
                await CreateInteractionMultiSelectAsync(interactionsGrid, counter, interaction);
                break;
            case InteractionType.OpenQuestion:
                await CreateInteractionOpenQuestionAsync(interactionsGrid, counter, interaction);
                break;
            case InteractionType.SingleSelect:
            default:
                await CreateInteractionSingleSelectAsync(interactionsGrid, counter, interaction);
                break;
        }
    }

    /// <summary>
    /// Asynchronously creates a button-based interaction (single select) and attaches it to the specified grid.
    /// </summary>
    /// <param name="interactionsGrid">The grid to which the interaction button will be added.</param>
    /// <param name="counter">The column index in the grid where the button should be placed.</param>
    /// <param name="interaction">The interaction object used to configure the button's content and behavior.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task CreateInteractionSingleSelectAsync(Grid interactionsGrid, int counter, Interaction interaction)
    {
        var interactionButton = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        interactionButton.Classes.Set("accent", true);
        interactionButton.Click += async (_, _) => await _runningStory.HandleElementAsync(interaction, null);

        var binding = new Binding("Bounds.Height") { Source = interactionButton };
        interactionButton.Bind(Layoutable.MinWidthProperty, binding);

        if (interaction.TextIsVisible)
            interactionButton.Content = await _runningStory.GetTextAsync(interaction);
        Grid.SetColumn(interactionButton, counter);
        interactionsGrid.Children.Add(interactionButton);
    }

    /// <summary>
    /// Asynchronously creates a multi-select interaction within the specified grid, based on the provided interaction
    /// details.
    /// </summary>
    /// <param name="interactionsGrid">The grid where the multi-select interaction will be added.</param>
    /// <param name="counter">The column index in the grid to place the interaction element.</param>
    /// <param name="interaction">The interaction details used to construct the multi-select UI element.</param>
    /// <returns>A task that represents the asynchronous operation of creating the multi-select interaction.</returns>
    async Task CreateInteractionMultiSelectAsync(Grid interactionsGrid, int counter, Interaction interaction)
    {
        var interactionButton = new ToggleButton
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        interactionButton.Click += async (_, _) => await _runningStory.HandleElementAsync(interaction, interactionButton.IsChecked == true);

        var binding = new Binding("Bounds.Height") { Source = interactionButton };
        interactionButton.Bind(Layoutable.MinWidthProperty, binding);

        if (interaction.TextIsVisible)
            interactionButton.Content = await _runningStory.GetTextAsync(interaction);
        Grid.SetColumn(interactionButton, counter);
        interactionsGrid.Children.Add(interactionButton);
    }

    /// <summary>
    /// Creates and displays an open question interaction within the specified grid.
    /// Generates the necessary controls and layouts based on the provided interaction details.
    /// </summary>
    /// <param name="interactionsGrid">The parent grid to which the interaction layout will be added.</param>
    /// <param name="counter">The column index in the grid where the interaction layout should be placed.</param>
    /// <param name="interaction">The interaction data containing details such as the prompt and associated properties.</param>
    /// <returns>A task representing the asynchronous operation of creating the open question interaction.</returns>
    async Task CreateInteractionOpenQuestionAsync(Grid interactionsGrid, int counter, Interaction interaction)
    {
        var horizontalGrid = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            ],
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var verticalGrid = new Grid
        {
            RowDefinitions =
            [
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            ]
        };
        Grid.SetColumn(verticalGrid, 0);
        horizontalGrid.Children.Add(verticalGrid);
        var tb = new TextBlock
        { Text = interaction.Prompt, Margin = new Thickness(0.0, 0.0, 0.0, DefaultGridMargin) };
        Grid.SetRow(tb, 0);
        verticalGrid.Children.Add(tb);
        var t = new TextBox();
        Grid.SetRow(t, 1);
        verticalGrid.Children.Add(t);

        var interactionButton = new Button { Margin = new Thickness(8.0, 0.0, 0.0, 0.0), HorizontalContentAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Center };
        interactionButton.Classes.Set("accent", true);

        var binding = new Binding("Bounds.Height") { Source = interactionButton };
        interactionButton.Bind(Layoutable.MinWidthProperty, binding);
        binding = new Binding("Bounds.Height") { Source = interactionButton, Converter = (OneWayConverter<double, double>?)OneWayConverter<double, double>.GetInstance((d, _, _) => d * 2.0) };
        t.Bind(Layoutable.MinWidthProperty, binding);

        interactionButton.Click += async (_, _) => await _runningStory.HandleElementAsync(interaction, t.Text ?? "");
        if (interaction.TextIsVisible)
            interactionButton.Content = await _runningStory.GetTextAsync(interaction);
        Grid.SetColumn(interactionButton, 1);
        horizontalGrid.Children.Add(interactionButton);
        Grid.SetColumn(horizontalGrid, counter);
        interactionsGrid.Children.Add(horizontalGrid);
    }

    /// <summary>
    /// Creates a panel and grid structure.
    /// </summary>
    /// <param name="chapter">Chapter (when not null) associated with the panel.</param>
    /// <param name="storyPanel">The created or recycled panel.</param>
    /// <param name="chapterGrid">The grid to be created or cleared.</param>
    void CreatePanelAndGrid(Chapter? chapter, out Panel storyPanel, out Grid chapterGrid)
    {
        storyPanel = new Panel();
        storyPanel.Classes.Set("fade_in", true);
        var chapterPanel = new Panel();

        if (chapter?.HasInventoryConditions() == true)
        {
            var storyGrid = new Grid
            {
                RowDefinitions =
                [
                    new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Star }
                ]
            };
            storyPanel.Children.Add(storyGrid);

            var inventoryButton = new Button { Content = _runningStory.MasterStory.InventoryText, Margin = new Thickness(0.0, 0.0, 0.0, DefaultGridMargin) };
            inventoryButton.Classes.Set("accent2", true);
            var copyOfStoryPanel = storyPanel;
            inventoryButton.Click += (_, _) => ShowInventory(chapter, copyOfStoryPanel);
            Grid.SetRow(inventoryButton, 0);
            storyGrid.Children.Add(inventoryButton);
            storyGrid.Children.Add(chapterPanel);
            Grid.SetRow(chapterPanel, 1);
        }
        else
        {
            storyPanel.Children.Add(chapterPanel);
        }

        chapterGrid = new Grid
        {
            RowDefinitions =
            [
                new RowDefinition { Height = new GridLength(GoldenRatio, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength((1.0 - GoldenRatio) * GoldenRatio, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength((1.0 - GoldenRatio) * (1.0 - GoldenRatio), GridUnitType.Star) }
            ]
        };
        chapterPanel.Children.Add(chapterGrid);
    }

    /// <summary>
    /// Shows the inventory panel for the given chapter.
    /// </summary>
    /// <param name="chapter">Chapter to use.</param>
    /// <param name="storyPanel">Story panel to use.</param>
    void ShowInventory(Chapter chapter, Panel storyPanel)
    {
        var inventoryPanel = new Panel { Background = new SolidColorBrush(new Color(0x60, 0x00, 0x00, 0x00)) };
        storyPanel.Children.Add(inventoryPanel);

        var inventoryGrid = new Grid
        {
            RowDefinitions =
            [
                new RowDefinition { Height = new GridLength(GoldenRatio, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(GoldenRatio, GridUnitType.Star) }
            ]
        };
        inventoryPanel.Children.Add(inventoryGrid);

        var closeButton = new Button
        {
            Content = _runningStory.MasterStory.CloseButtonText,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0.0, 0.0, 0.0, DefaultGridMargin)
        };
        closeButton.Classes.Set("accent2", true);
        closeButton.Click += (_, _) =>
        {
            inventoryPanel.Opacity = 0.0;
            inventoryPanel.Classes.Set("fade_out", true);
            inventoryPanel.PropertyChanged += (_, e) => WaitAndRemoveChild(storyPanel, inventoryPanel, e);
        };
        Grid.SetRow(closeButton, 0);

        var listBox = new ListBox
        {
            ItemsSource = _runningStory.GetInventoryItems(),
            ItemTemplate = new FuncDataTemplate<object>((_, _) => new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding("Text")
            })
        };
        Grid.SetRow(listBox, 1);
        inventoryGrid.Children.Add(closeButton);
        inventoryGrid.Children.Add(listBox);
        listBox.SelectionChanged += (_, _) =>
        {
            inventoryPanel.Opacity = 0.0;
            inventoryPanel.Classes.Set("fade_out", true);
            inventoryPanel.PropertyChanged += async (_, e) =>
            {
                if (!WaitAndRemoveChild(storyPanel, inventoryPanel, e))
                    return;
                if (listBox.SelectedItem is not Element inventoryItem)
                    return;
                await _runningStory.UseInventoryAsync(chapter, inventoryItem);
            };
        };

        inventoryPanel.Opacity = 1.0;
        inventoryPanel.Classes.Set("fade_in", true);
    }

    /// <summary>
    /// Waits for the opacity to reach 0.0 and then remove the given child.
    /// </summary>
    /// <param name="panel">Panel to remove child from.</param>
    /// <param name="child">Child to remove.</param>
    /// <param name="propertyChangedEvent">Property changed event to handle (containing the opacity).</param>
    /// <returns>True if removed and false otherwise.</returns>
    static bool WaitAndRemoveChild(Panel panel, Control child, AvaloniaPropertyChangedEventArgs propertyChangedEvent)
    {
        if (!PropertyFunction.TryGetPropertyNewValue<double>(propertyChangedEvent, Visual.OpacityProperty, out var opacity) || opacity > 0.0)
            return false;
        panel.Children.Remove(child);
        return true;
    }
}
