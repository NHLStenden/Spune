//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Spune.Common.Functions;

namespace Spune.Common.Miscellaneous;

/// <inheritdoc />
/// <summary>This class represents a writer for a comma-separated values file.</summary>
public class CsvExporter : IDisposable
{
	/// <summary>
	/// The file stream.
	/// </summary>
	FileStream? _fileStream;

	/// <summary>
	/// The stream writer.
	/// </summary>
	StreamWriter? _streamWriter;

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvExporter" /> class.
	/// </summary>
	public CsvExporter()
    {
        FileName = string.Empty;
    }

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvExporter" /> class.
	/// </summary>
	/// <param name="fileName">The name of the file.</param>
	/// <param name="fileMode">The file mode.</param>
	public CsvExporter(string fileName, FileMode fileMode = FileMode.Create)
    {
        FileName = fileName;
        Open(fileMode);
    }

	/// <summary>
	/// Gets or sets the file name of the file.
	/// </summary>
	/// <value>The name of the file.</value>
	public string FileName { get; set; }

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
	/// Opens the given file.
	/// </summary>
	/// <param name="fileMode">(Optional) The file mode.</param>
	public void Open(FileMode fileMode = FileMode.Create)
    {
        if (string.IsNullOrEmpty(FileName))
        {
            _fileStream = null;
            _streamWriter = null;
            return;
        }

        try
        {
            _fileStream = File.Open(FileName, fileMode);
        }
        catch (Exception)
        {
            _fileStream = null;
            _streamWriter = null;
            return;
        }

        _streamWriter = new StreamWriter(_fileStream);
    }

	/// <summary>
	/// Gets a line (to write).
	/// </summary>
	/// <param name="fields">The fields to write.</param>
	/// <returns>The line.</returns>
	public static string GetLine(params string[] fields)
    {
        var line = string.Empty;
        for (var i = 0; i < fields.Length; i++)
        {
            line += ValueToField(fields[i]);
            if (i < fields.Length - 1) line += ",";
        }

        return line;
    }

	/// <summary>
	/// Writes a line.
	/// </summary>
	/// <param name="fields">The fields to write.</param>
	public async Task WriteLineAsync(params string[] fields)
    {
        if (_streamWriter == null) return;

        var line = GetLine(fields);
        line += Environment.NewLine;
        await _streamWriter.WriteAsync(line);
    }

	/// <summary>
	/// Releases the unmanaged resources used by this class and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing">
	/// True to release both managed and unmanaged resources; false to release only unmanaged
	/// resources.
	/// </param>
	protected void Dispose(bool disposing)
    {
        if (_streamWriter == null) return;
        if (disposing)
            Close();
    }

	/// <summary>
	/// Closes the file.
	/// </summary>
	void Close()
    {
        _streamWriter?.Dispose();
        _streamWriter = null;
        _fileStream?.Dispose();
        _fileStream = null;
    }

	/// <summary>
	/// Clears all buffers for the writer and causes any buffered data to be written to the underlying stream.
	/// </summary>
	protected void Flush()
    {
        _streamWriter?.Flush();
        _fileStream?.Flush();
    }

    /// <summary>
    /// Converts a value to a field: formatted to be exported.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A string.</returns>
    static string ValueToField(string value) => CsvFunction.ValueToCsvField(value);
}