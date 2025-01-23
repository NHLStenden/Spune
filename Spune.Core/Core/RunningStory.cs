//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Microsoft.Extensions.AI;
using OllamaSharp;
using Spune.Common.Extensions;
using Spune.Common.Functions;
using Spune.Common.Handlers;
using Spune.Common.Interfaces;
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
    string _currentIdentifier = string.Empty;

    /// <summary>
    /// Inventory elements list member.
    /// </summary>
    readonly List<Element> _inventoryElements = [];

    /// <summary>
    /// Hidden elements list member.
    /// </summary>
    readonly List<Element> _hiddenElements = [];

    /// <summary>
    /// Hide element event handler.
    /// </summary>
    event AsyncEventHandler<Element>? HideElementEvent;

    /// <summary>
    /// Navigate to link event handler.
    /// </summary>
    event AsyncEventHandler<Element>? NavigateToLinkEvent;

    /// <summary>
    /// Put in inventory element event handler.
    /// </summary>
    event AsyncEventHandler<Element>? PutInInventoryElementEvent;

    /// <summary>
    /// Show message event handler.
    /// </summary>
    event AsyncEventHandler<string>? ShowMessageEvent;

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
    /// Occurs when hide element is called.
    /// </summary>
    public event AsyncEventHandler<Element> OnHideElement
    {
        add => HideElementEvent += value;
        remove => HideElementEvent -= value;
    }

    /// <summary>
    /// Occurs when navigate to link is called.
    /// </summary>
    public event AsyncEventHandler<Element> OnNavigateToLink
    {
        add => NavigateToLinkEvent += value;
        remove => NavigateToLinkEvent -= value;
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
    /// Occurs when show message is called.
    /// </summary>
    public event AsyncEventHandler<string> OnShowMessage
    {
        add => ShowMessageEvent += value;
        remove => ShowMessageEvent -= value;
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
    /// are lists of strings representing the outcomes or data related to those identifiers.
    /// </summary>
    /// <remarks>
    /// The <c>Results</c> property allows tracking and storing information about the outcomes or intermediate
    /// states of a story. It is used to facilitate dynamic content rendering and interaction logic
    /// based on the story progression. The values in the dictionary can be manipulated through methods such as
    /// <c>SetResult</c> or <c>SetResultAsync</c>.
    /// </remarks>
    public Dictionary<string, List<string>> Results { get; } = [];

    /// <summary>
    /// Initializes the chat client asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task InitializeChatClientAsync()
    {
        var clientProperties = await ClientProperties.GetInstanceAsync();
        var uri = new Uri(ClientPropertiesFunction.GetFullChatServerUri(clientProperties));
        var selectedModel = clientProperties.ChatServerModel;
        if (string.IsNullOrEmpty(selectedModel)) return;
        IChatClient client = new OllamaApiClient(uri, selectedModel);
        var input = string.Empty;

        try
        {
            await client.CompleteAsync(input);
        }
        catch (HttpRequestException)
        {
            // Eat it!
        }
    }

    /// <summary>
    /// Starts the master story by initializing the current chapter and setting the start date and time.
    /// </summary>
    /// <param name="filePath">The file path of the master story to be loaded.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartAsync(string filePath)
    {
        var masterStory = await MasterStoryReaderWriter.ReadMasterStoryAsync(filePath);
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
        _currentIdentifier = string.Empty;
        _inventoryElements.Clear();
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
    /// Gets the inventory elements for the given chapter.
    /// </summary>
    /// <returns>Collection with interactions.</returns>
    public List<Element> GetInventoryElements() => _inventoryElements;

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

        if (element is Interaction { IsInventory: true } interaction0 )
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
                await SetResultAsync(element, value);
            if (!await NavigateToLinkAsync(element)) return;
            await InvokeNavigateToLinkAsync(element);
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
        _currentIdentifier = masterStory.Chapters.Count > 0 ? masterStory.Chapters[0].Identifier : string.Empty;
        StartDateTime = DateTime.Now;
        await CheckEndAsync();
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
            _inventoryElements.Add(element);
    }

    /// <summary>
    /// Checks if the given element is in the inventory.
    /// </summary>
    /// <param name="element">Element to check.</param>
    /// <returns>True if it is and false otherwise.</returns>
    bool IsInInventory(Element element) => _inventoryElements.Any(x => string.Equals(x.Identifier, element.Identifier, StringComparison.Ordinal));

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
        _currentIdentifier = chapter != null ? chapter.Identifier :
            masterStory.Chapters.Count > 0 ? masterStory.Chapters[0].Identifier : string.Empty;
        StartDateTime = DateTime.Now;
        await CheckEndAsync();
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
    /// Invokes the navigate to link event asynchronously.
    /// </summary>
    /// <param name="element">The element to navigate to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task InvokeNavigateToLinkAsync(Element element)
    {
        if (NavigateToLinkEvent == null) return;
        await NavigateToLinkEvent.InvokeAsync(this, element);
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
    /// Asynchronously marks the end of a story by setting the EndDateTime to the current date and time.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task CheckEndAsync()
    {
        if (!HasEnded())
            return;
        EndDateTime = DateTime.Now;
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
        var index = MasterStory.Chapters.FindIndex(x =>
            string.Equals(x.Identifier, _currentIdentifier, StringComparison.Ordinal));
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
        _currentIdentifier = element.Identifier;
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
        var inventoryCondititions = chapter.InventoryConditions.Split('.').ToArray();
        if (!inventoryCondititions.Any(x => string.Equals(x, inventoryItem.Identifier, StringComparison.Ordinal)))
        {
            await InvokeShowMessageAsync(MasterStory.InventoryItemIsNotValidText);
            return;
        }
        await SetResultAsync(chapter, inventoryItem.Text);
        if (!await NavigateToLinkAsync(chapter))
            return;
        await InvokeNavigateToLinkAsync(chapter);
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
    /// Asynchronously sets the result of the current chapter in the story.
    /// </summary>
    /// <param name="element">The element used to obtain the result for the current chapter.</param>
    /// <param name="value">
    /// The object value indicating the result to be set. It can be a string, a boolean or null. Null means
    /// take the value (text) from the element.
    /// </param>
    /// <returns>A task that represents the asynchronous operation of setting the current result.</returns>
    async Task SetResultAsync(Element element, object? value)
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
            SetResult(_currentIdentifier, result, b);
        else
            Results[_currentIdentifier] = [result];
    }

    /// <summary>
    /// Navigates to a chapter defined by the link in the specified element object.
    /// </summary>
    /// <param name="element">The element object containing the link to the next chapter or step.</param>
    /// <returns>A task that represents the asynchronous operation. The task result if the navigation was successful or not.</returns>
    async Task<bool> NavigateToLinkAsync(Element element)
    {
        if (!HasValidLink(element)) return false;
        _currentIdentifier = element.Link;
        await CheckEndAsync();
        return true;
    }

    /// <summary>
    /// Checks if the link of the given element is valid.
    /// </summary>
    /// <param name="element">The element containing the link to check.</param>
    /// <returns>True if it is, and false otherwise.</returns>
    bool HasValidLink(Element element) => !string.IsNullOrEmpty(element.Link) && MasterStory.Chapters.Any(x => string.Equals(x.Identifier, element.Link, StringComparison.Ordinal));

    /// <summary>
    /// Checks if the identifier of the given element is valid.
    /// </summary>
    /// <param name="element">The element containing the identifier to check.</param>
    /// <returns>True if it is, and false otherwise.</returns>
    bool HasValidIdentifier(Element element) => !string.IsNullOrEmpty(element.Identifier) && MasterStory.Chapters.Any(x => string.Equals(x.Identifier, element.Identifier, StringComparison.Ordinal));

    /// <summary>
    /// Checks whether the specified story has ended.
    /// </summary>
    /// <returns>True if the story has ended; otherwise, false.</returns>
    bool HasEnded()
    {
        var index = MasterStory.Chapters.FindIndex(x =>
            string.Equals(x.Identifier, _currentIdentifier, StringComparison.Ordinal));
        return index == MasterStory.Chapters.Count - 1;
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
        var selectedModel = clientProperties.ChatServerModel;
        IChatClient client = new OllamaApiClient(uri, selectedModel);

        if (_chatResults.TryGetValue(element.Identifier, out _)) return ReplaceTextWithResults(text);
        var input = chapter.ChatMessage;
        input = ReplacePlaceholders(input, Results);
        string chatResult;
        if (!string.IsNullOrEmpty(selectedModel))
        {
            try
            {
                chatResult = (await client.CompleteAsync(input)).Message.Text ?? "";
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
        result = ReplacePlaceholders(result, _chatResults, "ChatResult.");
        result = ReplacePlaceholders(result, Results);
        return result;
    }

    /// <summary>
    /// Updates the result set for the specified key with the provided name based on the value parameter.
    /// </summary>
    /// <param name="key">The unique identifier associated with the result set to be updated.</param>
    /// <param name="name">The name to be added to or removed from the result set.</param>
    /// <param name="value">
    /// Indicates whether the name should be added or removed from the result set.
    /// If true, the name is added to the result set. If false, the name is removed.
    /// </param>
    void SetResult(string key, string name, bool value)
    {
        if (!Results.TryGetValue(key, out var list))
        {
            if (!value)
                return;
            list = [name];
            Results.Add(key, list);
            return;
        }

        if (value)
        {
            if (!list.Contains(name))
                list.Add(name);
        }
        else
        {
            list.Remove(name);
        }

        Results[key] = list;
    }

    /// <summary>
    /// Replaces placeholders in a string with corresponding values from a dictionary.
    /// </summary>
    /// <param name="text">The string containing placeholders.</param>
    /// <param name="dictionary">A dictionary with keys as placeholder names and values as replacement values.</param>
    /// <returns>The string with placeholders replaced by their corresponding values.</returns>
    static string ReplacePlaceholders(string text, IDictionary<string, List<string>> dictionary)
    {
        return ReplacePlaceholders(text, dictionary, string.Empty);
    }

    /// <summary>
    /// Replaces placeholders in a string with corresponding values from a dictionary.
    /// </summary>
    /// <param name="text">The string containing placeholders.</param>
    /// <param name="dictionary">A dictionary with keys as placeholder names and values as replacement values.</param>
    /// <param name="keyPrefix">A prefix for the key identifier.</param>
    /// <returns>The string with placeholders replaced by their corresponding values.</returns>
    static string ReplacePlaceholders(string text, IDictionary<string, List<string>> dictionary,
        string keyPrefix)
    {
        var newDictionary = new Dictionary<string, string>();
        foreach (var (key, value) in dictionary)
            newDictionary[key] = StringFunction.StringsToString(value, ", ");
        return ReplacePlaceholders(text, newDictionary, keyPrefix);
    }

    /// <summary>
    /// Replaces placeholders in a string with corresponding values from a dictionary.
    /// </summary>
    /// <param name="text">The string containing placeholders.</param>
    /// <param name="dictionary">A dictionary with keys as placeholder names and values as replacement values.</param>
    /// <param name="keyPrefix">A prefix for the key identifier.</param>
    /// <returns>The string with placeholders replaced by their corresponding values.</returns>
    static string ReplacePlaceholders(string text, IDictionary<string, string> dictionary, string keyPrefix)
    {
        const string doubleLeftBrace = @"\{\{";
        const string doubleRightBrace = @"\}\}";
        // Escape literal double curly braces
        var result = text.Replace("{{{{", doubleLeftBrace, StringComparison.Ordinal);
        result = result.Replace("}}}}", doubleRightBrace, StringComparison.Ordinal);

        // Replace placeholders with values
        foreach (var (key, value) in dictionary)
        {
            var placeholder = !string.IsNullOrEmpty(keyPrefix) ? "{{" + keyPrefix + key + "}}" : "{{" + key + "}}";
            result = result.Replace(placeholder, value, StringComparison.Ordinal);
        }

        // Unescape literal double curly braces
        result = result.Replace(doubleLeftBrace, "{{", StringComparison.Ordinal);
        result = result.Replace(doubleRightBrace, "}}", StringComparison.Ordinal);

        return result;
    }
}