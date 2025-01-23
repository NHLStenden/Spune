//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Core.Core;

/// <summary>
/// Represents a master story consisting of multiple chapters and interactions.
/// Provides functionality to initialize and load associated media resources.
/// </summary>
/// <remarks>
/// This class extends the <see cref="Element" /> base class and includes properties and methods specific to handling a
/// master story structure.
/// It supports asynchronous initialization and resource loading, as well as storing metadata such as the author name
/// and validation text.
/// </remarks>
public class MasterStory : Element, IDisposable
{
    /// <summary>
    /// The author member.
    /// </summary>
    string _author = string.Empty;

    /// <summary>
    /// The chapters are disposed member.
    /// </summary>
    bool _chaptersDisposed;

    /// <summary>
    /// Close button text member.
    /// </summary>
    string _closeButtonText = string.Empty;

    /// <summary>
    /// E-mail of organiser member.
    /// </summary>
    string _emailOrganizer = string.Empty;

    /// <summary>
    /// Gets the file path.
    /// </summary>
    string _filePath = string.Empty;

    /// <summary>
    /// The inventory text member.
    /// </summary>
    string _inventoryText = string.Empty;

    /// <summary>
    /// The inventory item is not valid text member.
    /// </summary>
    string _inventoryItemIsNotValidText = string.Empty;

    /// <summary>
    /// The value is mandatory text member.
    /// </summary>
    string _valueInputIsMandatoryText = string.Empty;

    /// <summary>
    /// The value select is mandatory text member.
    /// </summary>
    string _valueSelectIsMandatoryText = string.Empty;

    /// <summary>
    /// Gets the name of the author associated with the master story.
    /// </summary>
    /// <remarks>
    /// This property represents the author of the master story, providing information about the person responsible for its
    /// creation.
    /// It is initialized during object creation and remains immutable afterward.
    /// </remarks>
    public string Author
    {
        get => _author;
        set
        {
            _author = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Represents a collection of chapters that make up the master story.
    /// Each chapter contains interactions and media content relevant to the master story's structure.
    /// </summary>
    /// <remarks>
    /// This property is initialized with an empty list by default. Chapters are loaded and processed during
    /// the master story initialization phase. It serves as a key component in determining the progress
    /// and interactions within the master story.
    /// </remarks>
    public ObservableCollection<Chapter> Chapters { get; set; } = [];

    /// <summary>
    /// Represent the text for a close button.
    /// </summary>
    public string CloseButtonText
    {
        get => _closeButtonText;
        set
        {
            _closeButtonText = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Represent the e-mail address of the organiser.
    /// </summary>
    public string EmailOrganizer
    {
        get => _emailOrganizer;
        set
        {
            _emailOrganizer = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a text for the inventory.
    /// </summary>
    public string InventoryText
    {
        get => _inventoryText;
        set
        {
            _inventoryText = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a text message that is used if the selected inventory item is not valid.
    /// </summary>
    public string InventoryItemIsNotValidText
    {
        get => _inventoryItemIsNotValidText;
        set
        {
            _inventoryItemIsNotValidText = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a text message indicating that an input value is mandatory.
    /// </summary>
    /// <remarks>
    /// This property is used to provide a localized or user-friendly message when a required
    /// input field is left empty by the user. It ensures that the application communicates
    /// mandatory input requirements clearly to the user.
    /// </remarks>
    public string ValueInputIsMandatoryText
    {
        get => _valueInputIsMandatoryText;
        set
        {
            _valueInputIsMandatoryText = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a user-facing message indicating that selection of a value is mandatory.
    /// </summary>
    /// <remarks>
    /// This property is used to provide error or informational text when a mandatory selection
    /// of a value is required during user interaction within the application.
    /// </remarks>
    public string ValueSelectIsMandatoryText
    {
        get => _valueSelectIsMandatoryText;
        set
        {
            _valueSelectIsMandatoryText = value;
            NotifyPropertyChanged();
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Creates and returns an instance of MasterStory.
    /// </summary>
    /// <returns>The instance.</returns>
    public static MasterStory CreateInstance()
    {
        var masterStory = new MasterStory();
        masterStory.Initialize();
        return masterStory;
    }

    /// <summary>
    /// Initializes the instance.
    /// </summary>
    public void Initialize() => Chapters.CollectionChanged += (_, _) => NotifyPropertyChanged(nameof(Chapters));

    /// <summary>
    /// Asynchronously initializes the master story by loading media resources and setting up chapter and interaction hierarchies.
    /// </summary>
    /// <param name="filePath">
    /// The file path of the master story to be initialized. It is also used to load associated media
    /// resources.
    /// </param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public async Task StartAsync(string filePath)
    {
        _filePath = filePath;
        var directory = Path.GetDirectoryName(filePath) ?? "";
        foreach (var chapter in Chapters)
        {
            chapter.SetParent(this);
            await chapter.Media.LoadAsync(directory);
            foreach (var interaction in chapter.Interactions)
            {
                interaction.SetParent(chapter);
                await chapter.Media.LoadAsync(directory);
            }
        }
    }

    /// <summary>
    /// Retrieves the file path associated with the current master story instance.
    /// </summary>
    /// <returns>
    /// The file path of the master story as a string.
    /// </returns>
    public string GetFilePath() => _filePath;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    void Dispose(bool disposing)
    {
        if (_chaptersDisposed) return;
        if (!disposing) return;
        foreach (var c in Chapters.Reverse())
            c.Dispose();
        _chaptersDisposed = true;
    }
}