//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Globalization;
using Avalonia.Threading;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Spune.Common.Extensions;
using Spune.Common.Functions;
using Spune.Common.Handlers;
using Spune.Common.Interfaces;
using Spune.Core.Extensions;
using Spune.Core.Functions;
using Spune.Core.Miscellaneous;

namespace Spune.Core.Core;

/// <summary>
/// Represents the running state of a story, handling its lifecycle, tracking results, and managing navigation between story chapters.
/// </summary>
public class RunningStory
{
    /// <summary>
    /// Chat results member.
    /// </summary>
    readonly Dictionary<string, string> _chatResults = [];

    /// <summary>
    /// Represents the identifier of the currently active story chapter in a running story.
    /// </summary>
    /// <remarks>
    /// This variable is used to track the progression of a story. It maps to the
    /// <see cref="Chapter.Identifier" /> of the current chapter. It can be updated when transitioning
    /// between chapters or interactions within a story. Its value is initialized to an empty string
    /// and assigned based on the chapters available in the story.
    /// </remarks>
    RunningStoryIdentifier _currentIdentifier = new RunningStoryIdentifier();

    /// <summary>
    /// Inventory items list member.
    /// </summary>
    readonly List<Element> _inventoryItems = [];

    /// <summary>
    /// Hidden elements list member.
    /// </summary>
    readonly List<Element> _hiddenElements = [];

    /// <summary>
    /// Elapsed time event handler.
    /// </summary>
    event AsyncEventHandler<double>? ElpasedTimeEvent;

    /// <summary>
    /// Hide element event handler.
    /// </summary>
    event AsyncEventHandler<Element>? HideElementEvent;

    /// <summary>
    /// Maximum duration timer interval in ms member.
    /// </summary>
    const double MaxDurationTimerInterval = 500.0;

    /// <summary>
    /// Navigate to event handler.
    /// </summary>
    event AsyncEventHandler<Element>? NavigateToEvent;

    /// <summary>
    /// Put in inventory element event handler.
    /// </summary>
    event AsyncEventHandler<Element>? PutInInventoryElementEvent;

    /// <summary>
    /// Spune story results identifier.
    /// </summary>
    const string SpuneStoryResults = "{{SpuneStory.Results}}";

    /// <summary>
    /// Start event handler.
    /// </summary>
    event AsyncEventHandler<RunningStory>? StartEvent;

    /// <summary>
    /// Show message event handler.
    /// </summary>
    event AsyncEventHandler<string>? ShowMessageEvent;

    /// <summary>
    /// Time left event handler.
    /// </summary>
    event AsyncEventHandler<double>? TimeLeftEvent;

    /// <summary>
    /// This property represents a function for creating an email sender interface.
    /// </summary>
    public static Func<IEmailSender>? EmailSenderCreator { get; set; }

    /// <summary>
    /// Gets the end date and time when the story was completed.
    /// </summary>
    /// <remarks>
    /// This property records the exact timestamp when the story is finished.
    /// It is set when the <c>End</c> method is called and remains immutable afterward.
    /// </remarks>
    public DateTime EndDateTime { get; set; }

    /// <summary>
    /// Gets the master story.
    /// </summary>
    /// <returns>The master story.</returns>
    public MasterStory MasterStory { get; private set; } = MasterStory.CreateInstance();

    /// <summary>
    /// Occurs when elapsed time is called.
    /// </summary>
    public event AsyncEventHandler<double> OnElpasedTime
    {
        add => TimeLeftEvent += value;
        remove => TimeLeftEvent -= value;
    }

    /// <summary>
    /// Occurs when hide element is called.
    /// </summary>
    public event AsyncEventHandler<Element> OnHideElement
    {
        add => HideElementEvent += value;
        remove => HideElementEvent -= value;
    }

    /// <summary>
    /// Occurs when navigate to is called.
    /// </summary>
    public event AsyncEventHandler<Element> OnNavigateTo
    {
        add => NavigateToEvent += value;
        remove => NavigateToEvent -= value;
    }

    /// <summary>
    /// Occurs when put in inventory element is called.
    /// </summary>
    public event AsyncEventHandler<Element> OnPutInInventoryElement
    {
        add => PutInInventoryElementEvent += value;
        remove => PutInInventoryElementEvent -= value;
    }

    /// <summary>
    /// Occurs when start is called.
    /// </summary>
    public event AsyncEventHandler<RunningStory> OnStart
    {
        add => StartEvent += value;
        remove => StartEvent -= value;
    }

    /// <summary>
    /// Occurs when show message is called.
    /// </summary>
    public event AsyncEventHandler<string> OnShowMessage
    {
        add => ShowMessageEvent += value;
        remove => ShowMessageEvent -= value;
    }

    /// <summary>
    /// Occurs when time left is called.
    /// </summary>
    public event AsyncEventHandler<double> OnTimeLeft
    {
        add => TimeLeftEvent += value;
        remove => TimeLeftEvent -= value;
    }

    /// <summary>
    /// Represents the start date and time of a story.
    /// </summary>
    /// <remarks>
    /// This property is set when the story begins, using the current system date and time.
    /// It indicates when the process started and is utilized in logging or tracking purposes.
    /// </remarks>
    /// <value>
    /// A <see cref="DateTime" /> value that represents the precise moment of the story start.
    /// </value>
    public DateTime StartDateTime { get; set; }

    /// <summary>
    /// Gets the results of the story. The results are represented as a dictionary where the keys are
    /// unique identifiers corresponding to specific chapters or interactions within the story, and the values
    /// are multiple lists of strings representing the outcomes or data related to those identifiers.
    /// </summary>
    public Dictionary<RunningStoryIdentifier, RunningStoryResult> Results { get; } = [];

    /// <summary>
    /// Starts the master story by initializing the current chapter and setting the start date and time.
    /// </summary>
    /// <param name="filePath">The file path of the master story to be loaded.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartAsync(string filePath)
    {
        var masterStory = await MasterStoryReaderWriter.ReadMasterStoryAsync(filePath);
        await InitializeChatClientAsync(masterStory);
        await StartAsync(masterStory);
    }

    /// <summary>
    /// Starts the master story by initializing the current chapter and setting the start date and time.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartAsync() => await StartAsync(new MasterStory());

    /// <summary>
    /// Refreshes the current chapter by reloading it, and reinitializing the story.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RefreshChapterAsync()
    {
        MasterStory.Dispose();
        MasterStory = await MasterStoryReaderWriter.ReadMasterStoryAsync(MasterStory.GetFilePath());
        var chapter = GetChapter();
        if (chapter == null)
            return;
        await StartAsync(MasterStory, chapter);
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public void Clear()
    {
        MasterStory.Dispose();
        _chatResults.Clear();
        _currentIdentifier = new RunningStoryIdentifier();
        _inventoryItems.Clear();
        _hiddenElements.Clear();
        EndDateTime = new DateTime();
        StartDateTime = new DateTime();
        Results.Clear();
    }

    /// <summary>
    /// Gets the interactions for the current chapter.
    /// </summary>
    /// <returns>Collection with interactions.</returns>
    public ObservableCollection<Interaction> GetInteractions()
    {
        var chapter = GetChapter();
        return chapter != null ? new ObservableCollection<Interaction>(chapter.Interactions.Where(x => !IsInInventory(x) && !IsHidden(x))) : [];
    }

    /// <summary>
    /// Gets the interactions for the given chapter.
    /// </summary>
    /// <param name="chapter">Chapter to get interactions for.</param>
    /// <returns>Collection with interactions.</returns>
    public ObservableCollection<Interaction> GetInteractions(Chapter chapter) => new(chapter.Interactions.Where(x => !IsInInventory(x) && !IsHidden(x)));

    /// <summary>
    /// Gets the inventory items.
    /// </summary>
    /// <returns>Collection with elements.</returns>
    public List<Element> GetInventoryItems() => _inventoryItems;

    /// <summary>
    /// Handles the action of an element.
    /// </summary>
    /// <param name="element">The interaction element to handle.</param>
    /// <param name="value">
    /// The object value indicating the result to be set. It can be a string, a boolean, or null. Null means
    /// take the value (text) from the element.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleElementAsync(Element element, object? value)
    {
        if (element is Chapter chapter && chapter != GetChapter())
        {
            // Image click close can be before close time-out. Do nothing.
            return;
        }

        if (element.HasHint())
            await InvokeShowMessageAsync(GetHint(element));

        if (value is string s && string.IsNullOrEmpty(s))
        {
            await InvokeShowMessageAsync(MasterStory.ValueInputIsMandatoryText);
            return;
        }

        if (element is Interaction { IsInventory: true } interaction0)
        {
            PutInInventoryElement(interaction0);
            await InvokePutInInventoryElementAsync(interaction0);
        }

        if (element is Interaction { RemoveAfterUse: true } interaction1)
        {
            HideElement(interaction1);
            await InvokeHideElementAsync(interaction1);
        }
        else
        {
            if (element is not Interaction interaction2 || interaction2.SetsResult)
            {
                await SetTextResultAsync(element, value);
                SetIdentifierResult(element, value);
            }
            if (!await NavigateToLinkAsync(element)) return;
            await InvokeNavigateToAsync(element);
        }
    }

    /// <summary>
    /// Initializes the chat client asynchronously.
    /// </summary>
    /// <param name="masterStory">The master story to be started. This parameter contains the chapters to initialize the execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task InitializeChatClientAsync(MasterStory masterStory)
    {
        var clientProperties = await ClientProperties.GetInstanceAsync();
        var uri = new Uri(ClientPropertiesFunction.GetFullChatServerUri(clientProperties));
        var selectedModel = masterStory.ChatServerModel;
        if (string.IsNullOrEmpty(selectedModel)) return;
        IChatClient client = new OllamaApiClient(uri, selectedModel);
        var input = string.Empty;

        try
        {
            await client.GetResponseAsync<string>(input);
        }
        catch (HttpRequestException)
        {
            // Eat it!
        }
    }

    /// <summary>
    /// Starts the master story by initializing the current chapter and setting the start date and time.
    /// </summary>
    /// <param name="masterStory">The master story to be started. This parameter contains the chapters to initialize the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task StartAsync(MasterStory masterStory)
    {
        if (MasterStory != masterStory)
            MasterStory.Dispose();
        MasterStory = masterStory;
        _currentIdentifier = MasterStory.Chapters.Count > 0 ? new RunningStoryIdentifier(MasterStory.Chapters[0].Identifier, MasterStory.Chapters[0].IdentifierText) : new RunningStoryIdentifier();
        _inventoryItems.Clear();
        _inventoryItems.AddRange(MasterStory.InventoryItems);

        CheckStart();
        await CheckEndAsync();
        await InvokeStartAsync(this);
    }

    /// <summary>
    /// Starts from the first chapter asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    void StartFirstChapter()
    {
        StartDateTime = DateTime.Now;
        if (MasterStory.HasMaxDuration())
            StartTimoutTimer();
    }

    /// <summary>
    /// Starts the timeout timer.
    /// </summary>
    void StartTimoutTimer()
    {
        var timeSpan = TimeSpan.FromMilliseconds(MaxDurationTimerInterval);
        var timeTimer = new DispatcherTimer { Interval = timeSpan };
        timeTimer.Tick += async (sender, _) => await TimeTimerTick(sender);
        timeTimer.Start();
    }

    /// <summary>
    /// Handles the tick event of the timer.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a DispatcherTimer.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task TimeTimerTick(object? sender)
    {
        if (sender is not DispatcherTimer t) return;
        if (!MasterStory.HasMaxDuration() || HasEnded())
        {
            Stop(t);
            return;
        }
        var now = DateTime.Now;
        var elapsedTime = (now - StartDateTime).TotalMilliseconds;
        var timeLeft = MasterStory.MaxDuration - elapsedTime;
        await InvokeElapsedTimeAsync(timeLeft);
        await InvokeTimeLeftAsync(timeLeft);
        if (elapsedTime <= MasterStory.MaxDuration) return;
        Stop(t);
        await NavigateToTimeoutAsync();
        await InvokeNavigateToAsync();
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    /// <param name="timer">Timer to use.</param>
    void Stop(DispatcherTimer? timer)
    {
        timer?.Stop();
        SetElapsedTimeResult();
    }

    /// <summary>
    /// Sets the elapsed time in the results.
    /// </summary>
    void SetElapsedTimeResult()
    {
        if (!MasterStory.HasMaxDuration())
            return;
        
        var now = DateTime.Now;
        var elapsedTime = (now - StartDateTime).TotalSeconds;
        var result = string.Format(CultureInfo.CurrentCulture, "{0:F0}", elapsedTime);
        Results[new RunningStoryIdentifier("SpuneStory.ElapsedTime", MasterStory.RemainingTimeText)] = new RunningStoryResult { Texts = [result], Identifiers = [result] };
    }

    /// <summary>
    /// Adds an element to the hidden collection.
    /// </summary>
    /// <param name="element">Element to hide.</param>
    void HideElement(Element element)
    {
        if (!element.HasIdentifier())
            return;
        if (!IsHidden(element))
            _hiddenElements.Add(element);
    }

    /// <summary>
    /// Checks if the given element is hidden.
    /// </summary>
    /// <param name="element">Element to check.</param>
    /// <returns>True if it is and false otherwise.</returns>
    bool IsHidden(Element element) => _hiddenElements.Any(x => string.Equals(x.Identifier, element.Identifier, StringComparison.Ordinal));

    /// <summary>
    /// Adds an element to the inventory collection.
    /// </summary>
    /// <param name="element">Element to put in inventory.</param>
    void PutInInventoryElement(Element element)
    {
        if (!element.HasIdentifier())
            return;
        if (!IsInInventory(element))
            _inventoryItems.Add(ElementToInventoryItem(element));
    }

    /// <summary>
    /// Checks if the given element is in the inventory.
    /// </summary>
    /// <param name="element">Element to check.</param>
    /// <returns>True if it is and false otherwise.</returns>
    bool IsInInventory(Element element) => _inventoryItems.Any(x => string.Equals(x.Identifier, element.Identifier, StringComparison.Ordinal));

    /// <summary>
    /// Starts the master story by initializing the current chapter and setting the start date and time.
    /// </summary>
    /// <param name="masterStory">The master story to be started. This parameter contains the chapters to initialize the execution.</param>
    /// <param name="element">The element with the identifier (chapter) to start at.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task StartAsync(MasterStory masterStory, Element element)
    {
        if (MasterStory != masterStory)
            MasterStory.Dispose();
        MasterStory = masterStory;
        var chapter =
            masterStory.Chapters.FirstOrDefault(x => string.Equals(x.Identifier, element.Identifier, StringComparison.Ordinal));
        _currentIdentifier = chapter != null ? new RunningStoryIdentifier(chapter.Identifier, chapter.IdentifierText) : masterStory.Chapters.Count > 0 ? new RunningStoryIdentifier(masterStory.Chapters[0].Identifier, masterStory.Chapters[0].IdentifierText) : new RunningStoryIdentifier();
        CheckStart();
        await CheckEndAsync();
        await InvokeStartAsync(this);
    }

    /// <summary>
    /// Invokes the start event asynchronously.
    /// </summary>
    /// <param name="runningStory">The started running story.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task InvokeStartAsync(RunningStory runningStory)
    {
        if (StartEvent == null) return;
        await StartEvent.InvokeAsync(this, runningStory);
    }

    /// <summary>
    /// Invokes the show message event asynchronously.
    /// </summary>
    /// <param name="text">The message text to show.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task InvokeShowMessageAsync(string text)
    {
        if (ShowMessageEvent == null) return;
        await ShowMessageEvent.InvokeAsync(this, text);
    }

    /// <summary>
    /// Invokes the elpased time event asynchronously.
    /// </summary>
    /// <param name="d">The elapsed time in ms.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task InvokeElapsedTimeAsync(double d)
    {
        if (ElpasedTimeEvent == null) return;
        await ElpasedTimeEvent.InvokeAsync(this, d);
    }

    /// <summary>
    /// Invokes the time left event asynchronously.
    /// </summary>
    /// <param name="d">The time left in ms.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task InvokeTimeLeftAsync(double d)
    {
        if (TimeLeftEvent == null) return;
        await TimeLeftEvent.InvokeAsync(this, d);
    }

    /// <summary>
    /// Invokes the hide element event asynchronously.
    /// </summary>
    /// <param name="element">The element to hide.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task InvokeHideElementAsync(Element element)
    {
        if (HideElementEvent == null) return;
        await HideElementEvent.InvokeAsync(this, element);
    }
    /// <summary>
    /// Invokes the navigate to current chapter event asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task InvokeNavigateToAsync()
    {
        if (NavigateToEvent == null) return;
        var chapter = GetChapter();
        if (chapter == null) return;
        await NavigateToEvent.InvokeAsync(this, chapter);
    }

    /// <summary>
    /// Invokes the navigate to event asynchronously.
    /// </summary>
    /// <param name="element">The element to navigate to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task InvokeNavigateToAsync(Element element)
    {
        if (NavigateToEvent == null) return;
        await NavigateToEvent.InvokeAsync(this, element);
    }

    /// <summary>
    /// Invokes the put in inventory element event asynchronously.
    /// </summary>
    /// <param name="element">The element to put in the inventory.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task InvokePutInInventoryElementAsync(Element element)
    {
        if (PutInInventoryElementEvent == null) return;
        await PutInInventoryElementEvent.InvokeAsync(this, element);
    }

    /// <summary>
    /// Checks if the running is started from the beginning.
    /// </summary>
    void CheckStart()
    {
        if (!IsAtStart())
            return;
        StartFirstChapter();
    }

    /// <summary>
    /// Asynchronously marks the end of a story by setting the EndDateTime to the current date and time.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task CheckEndAsync()
    {
        if (!HasEnded())
            return;

        EndDateTime = DateTime.Now;
        SetElapsedTimeResult();
        await SendEmailAsync();
    }

    /// <summary>
    /// Send an e-mail with attachment to the organizer.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task SendEmailAsync()
    {
        var emailSender = EmailSenderCreator != null ? EmailSenderCreator() : new EmailSender();

        var clientProperties = await ClientProperties.GetInstanceAsync();

        emailSender.SmtpHost = clientProperties.EmailSmtpHost;
        emailSender.Port = clientProperties.EmailPort;
        emailSender.UserName = clientProperties.EmailUserName;
        emailSender.Password = clientProperties.EmailPassword;
        emailSender.From = clientProperties.EmailFrom;
        emailSender.To = MasterStory.EmailOrganizer;
        emailSender.Subject = "Running story result";
        emailSender.AttachmentFileName = "SpuneRunningStory.csv";

        await using var ms = new MemoryStream();
        await RunningStoryWriter.WriteToStreamAsync(this, ms);
        emailSender.AttachmentStream = ms;

        emailSender.Body = StringFunction.StringsToString(
        [
            "Dear organizer,",
            "",
            "The attachment of this e-mail contains the result for a running story.",
            "",
            "Regards, Spune"
        ]);
        await emailSender.SendAsync();
    }

    /// <summary>
    /// Retrieves the current chapter of a given story based on the internal state of the story.
    /// </summary>
    /// <returns>The current chapter of the story if found; otherwise, null.</returns>
    public Chapter? GetChapter()
    {
        var index = MasterStory.Chapters.FindIndex(x => new RunningStoryIdentifier(x.Identifier, x.IdentifierText) == _currentIdentifier);
        return index >= 0 ? MasterStory.Chapters[index] : null;
    }

    /// <summary>
    /// Navigates to a chapter defined by the identifier in the specified element object.
    /// </summary>
    /// <param name="element">The element object containing the identifier to the next chapter or step.</param>
    /// <returns>A task that represents the asynchronous operation. The task result if the navigation was successful or not.</returns>
    public async Task<bool> NavigateToIdentifierAsync(Element element)
    {
        if (!HasValidIdentifier(element)) return false;
        _currentIdentifier = new RunningStoryIdentifier(element.Identifier, element.IdentifierText);
        CheckStart();
        await CheckEndAsync();
        return true;
    }

    /// <summary>
    /// Asynchronously generates and retrieves text based on a provided element.
    /// </summary>
    /// <param name="element">The element to get text from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated or retrieved text.</returns>
    public async Task<string> GetTextAsync(Element element) => await GetTextAsync(element, null);

    /// <summary>
    /// Uses a given inventory item for the specified chapter.
    /// </summary>
    /// <param name="chapter">Chapter to use inventory for.</param>
    /// <param name="inventoryItem">Inventory item to use in the given chapter.</param>
    public async Task UseInventoryAsync(Chapter chapter, Element inventoryItem)
    {
        if (!inventoryItem.HasIdentifier())
            return;
        var inventoryConditions = chapter.InventoryConditions.Split(',').Select(x => x.Trim()).ToArray();
        if (!inventoryConditions.Any(x => string.Equals(x, inventoryItem.Identifier, StringComparison.Ordinal)))
        {
            await InvokeShowMessageAsync(MasterStory.InventoryItemIsNotValidText);
            return;
        }
        if (inventoryItem is Interaction interaction && interaction.RemoveAfterUse)
            _inventoryItems.Remove(inventoryItem);
        await SetTextResultAsync(chapter, inventoryItem.Text);
        SetIdentifierResult(inventoryItem, null);
        if (!await NavigateToLinkAsync(chapter))
            return;
        await InvokeNavigateToAsync(chapter);
    }

    /// <summary>
    /// Asynchronously generates and retrieves the hint based on a provided element.
    /// </summary>
    /// <param name="element">The element to get hint from.</param>
    /// <returns>The hint.</returns>
    string GetHint(Element element)
    {
        var result = element.Hint;
        return ReplaceTextWithResults(result);
    }

    /// <summary>
    /// Asynchronously sets the text result of the current chapter in the story.
    /// </summary>
    /// <param name="element">The element used to obtain the result for the current chapter.</param>
    /// <param name="value">
    /// The object value indicating the result to be set. It can be a string, a boolean or null. Null means
    /// take the value (text) from the element.
    /// </param>
    /// <returns>A task that represents the asynchronous operation of setting the current result.</returns>
    async Task SetTextResultAsync(Element element, object? value)
    {
        string? textOverride;
        if (value is string s)
            textOverride = s;
        else if (element is Chapter)
            textOverride = string.Empty;
        else
            textOverride = null;

        var result = await GetTextAsync(element, textOverride);

        if (element is Interaction interaction && interaction.HasPostProcessingItems())
            result = interaction.PostProcess(result);

        if (value is bool b)
            SetTextResult(_currentIdentifier, result, b);
        else
            Results[_currentIdentifier] = new RunningStoryResult { Texts = [result] };
    }

    /// <summary>
    /// Asynchronously sets the identifier result of the current chapter in the story.
    /// </summary>
    /// <param name="element">The element used to obtain the result for the current chapter.</param>
    /// <param name="value">
    /// The object value indicating the result to be set. It can be a boolean or null. Null just sets the identifier.
    /// </param>
    void SetIdentifierResult(Element element, object? value)
    {
        var key = _currentIdentifier;
        if (!Results.TryGetValue(key, out var list))
        {
            if (value is bool b && !b)
                return;
            list = new RunningStoryResult { Identifiers = [element.Identifier] };
            Results.Add(key, list);
            return;
        }

        if (value is bool enabled)
        {
            if (enabled)
            {
                if (!list.Identifiers.Contains(element.Identifier))
                    list.Identifiers.Add(element.Identifier);
            }
            else
            {
                list.Identifiers.Remove(element.Identifier);
            }
        }
        else
        {
            list.Identifiers.Add(element.Identifier);
        }

        Results[key] = list;
    }

    /// <summary>
    /// Navigates to a chapter defined by the link in the specified element object.
    /// </summary>
    /// <param name="element">The element object containing the link to the next chapter or step.</param>
    /// <returns>A task that represents the asynchronous operation. The task result if the navigation was successful or not.</returns>
    async Task<bool> NavigateToLinkAsync(Element element)
    {
        if (!HasValidLink(element)) return false;
        var link = element.DecodeLink(this);
        var linkChapter = MasterStory.Chapters.FirstOrDefault(x => string.Equals(x.Identifier, link, StringComparison.Ordinal));
        if (linkChapter == null) return false;
        _currentIdentifier = new RunningStoryIdentifier(linkChapter.Identifier, linkChapter.IdentifierText);
        CheckStart();
        await CheckEndAsync();
        return true;
    }

    /// <summary>
    /// Checks if the link of the given element is valid.
    /// </summary>
    /// <param name="element">The element containing the link to check.</param>
    /// <returns>True if it is, and false otherwise.</returns>
    bool HasValidLink(Element element)
    {
        var link = element.DecodeLink(this);
        return !string.IsNullOrEmpty(link) && MasterStory.Chapters.Any(x => string.Equals(x.Identifier, link, StringComparison.Ordinal));
    }

    /// <summary>
    /// Checks if the identifier of the given element is valid.
    /// </summary>
    /// <param name="element">The element containing the identifier to check.</param>
    /// <returns>True if it is, and false otherwise.</returns>
    bool HasValidIdentifier(Element element) => !string.IsNullOrEmpty(element.Identifier) && MasterStory.Chapters.Any(x => string.Equals(x.Identifier, element.Identifier, StringComparison.Ordinal));

    /// <summary>
    /// Navigate to the timeout chapter.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task NavigateToTimeoutAsync()
    {
        if (MasterStory.Chapters.Count == 0)
            return;
        if (!string.IsNullOrEmpty(MasterStory.TimeoutLink))
        {
            var timeoutLinkChapter = MasterStory.Chapters.FirstOrDefault(x => string.Equals(x.Identifier, MasterStory.TimeoutLink, StringComparison.Ordinal));
            _currentIdentifier = timeoutLinkChapter != null ? new RunningStoryIdentifier(timeoutLinkChapter.Identifier, timeoutLinkChapter.IdentifierText) : new RunningStoryIdentifier(MasterStory.Chapters[^1].Identifier, MasterStory.Chapters[^1].IdentifierText);
        }
        else
        {
            _currentIdentifier = new RunningStoryIdentifier(MasterStory.Chapters[^1].Identifier, MasterStory.Chapters[^1].IdentifierText);
        }
        CheckStart();
        await CheckEndAsync();
    }

    /// <summary>
    /// Checks whether the specified story is at the start.
    /// </summary>
    /// <returns>True if the story is at the start; otherwise, false.</returns>
    bool IsAtStart()
    {
        var chapter = GetChapter();
        return chapter?.IsStart == true;
    }

    /// <summary>
    /// Checks whether the specified story has ended.
    /// </summary>
    /// <returns>True if the story has ended; otherwise, false.</returns>
    bool HasEnded()
    {
        var chapter = GetChapter();
        return chapter?.IsEnd == true;
    }

    /// <summary>
    /// Asynchronously generates and retrieves text based on a provided element.
    /// </summary>
    /// <param name="element">The element to get text from.</param>
    /// <param name="textOverride">Set this to a value to not use element.Text, but use this value instead for the text.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated or retrieved text.</returns>
    async Task<string> GetTextAsync(Element element, string? textOverride)
    {
        var text = textOverride ?? element.Text;
        if (element is not Chapter chapter || string.IsNullOrEmpty(chapter.ChatMessage))
            return ReplaceTextWithResults(text);

        var clientProperties = await ClientProperties.GetInstanceAsync();
        var uri = new Uri(ClientPropertiesFunction.GetFullChatServerUri(clientProperties));
        var selectedModel = MasterStory.ChatServerModel;
        IChatClient client = new OllamaApiClient(uri, selectedModel);

        if (_chatResults.TryGetValue(element.Identifier, out _)) return ReplaceTextWithResults(text);
        var input = chapter.ChatMessage;
        input = PlaceholderFunction.ReplacePlaceholders(input, Results.ToDictionary(x => x.Key.Identifier, x => x.Value.Texts));
        string chatResult;
        if (!string.IsNullOrEmpty(selectedModel))
        {
            try
            {
                chatResult = (await client.GetResponseAsync<string>(input)).Result;
            }
            catch (HttpRequestException)
            {
                // Replace with fallback
                chatResult = chapter.ChatFallback;
            }
        }
        else
        {
            chatResult = chapter.ChatFallback;
        }

        chatResult = chatResult.Trim();
        _chatResults[element.Identifier] = chatResult;

        return ReplaceTextWithResults(text);
    }

    /// <summary>
    /// Replaces variables with identifier '%Id%' in the given text with the results.
    /// </summary>
    /// <param name="text">Text to replace.</param>
    /// <returns>The replaced text.</returns>
    string ReplaceTextWithResults(string text)
    {
        var result = text;
        if (string.Equals(result, SpuneStoryResults, StringComparison.Ordinal))
        {
            var dictionary = Results.ToDictionary(x => x.Key, x => x.Value.Texts).Where(x => !x.Key.Identifier.StartsWith("SpuneStory.", StringComparison.Ordinal) && x.Value.Count > 0 && x.Value.All(y => !string.IsNullOrEmpty(y)));
            var list = new List<string>();
            // Same format is in RunningStoryView.cs: 248
            foreach (var (key, value) in dictionary)
                list.Add(Invariant($"{key.Text}\t{StringFunction.StringsToString(value, ", ")}"));
            result = StringFunction.StringsToString(list, "\0");
        }
        {
            result = PlaceholderFunction.ReplacePlaceholders(result, _chatResults, "ChatResult.");
            result = PlaceholderFunction.ReplacePlaceholders(result, Results.ToDictionary(x => x.Key.Identifier, x => x.Value.Texts));
        }
        return result;
    }

    /// <summary>
    /// Updates the text result set for the specified key with the provided name based on the value parameter.
    /// </summary>
    /// <param name="key">The unique identifier associated with the result set to be updated.</param>
    /// <param name="name">The name to be added to or removed from the result set.</param>
    /// <param name="value">
    /// Indicates whether the name should be added or removed from the result set.
    /// If true, the name is added to the result set. If false, the name is removed.
    /// </param>
    void SetTextResult(RunningStoryIdentifier key, string name, bool value)
    {
        if (!Results.TryGetValue(key, out var list))
        {
            if (!value)
                return;
            list = new RunningStoryResult { Texts = [name] };
            Results.Add(key, list);
            return;
        }

        if (value)
        {
            if (!list.Texts.Contains(name))
                list.Texts.Add(name);
        }
        else
        {
            list.Texts.Remove(name);
        }

        Results[key] = list;
    }

    /// <summary>
    /// Converts an <see cref="Element"/> to an inventory item.
    /// </summary>
    /// <param name="element">The <see cref="Element"/> to be converted.</param>
    /// <returns>The converted <see cref="Element"/>.</returns>
    static Element ElementToInventoryItem(Element element)
    {
        if (element is not Interaction interaction || string.IsNullOrEmpty(interaction.HintForInventory))
            return element;
        interaction.Hint = interaction.HintForInventory;
        return interaction;
    }
}