//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Globalization;
using System.Text;
using Spune.Common.Functions;
using Spune.Common.Miscellaneous;

namespace Spune.Core.Core;

/// <summary>
/// Provides functionality to write and export running story results to a CSV file or initiate browser downloads.
/// </summary>
public static class RunningStoryWriter
{
    /// <summary>
    /// Writes the results of a running story to a stream in CSV format.
    /// </summary>
    /// <param name="runningStory">The running story containing results and lifecycle information to be exported.</param>
    /// <param name="ms">The stream to copy the CSV to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task WriteToStreamAsync(RunningStory runningStory, Stream ms)
    {
        var results = runningStory.Results.ToDictionary(x => x.Key, x => x.Value.Texts);
        results.Add("SpuneStory.StartDateTime",
            [runningStory.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)]);
        results.Add("SpuneStory.EndDateTime",
            [runningStory.EndDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)]);

        var lines = new List<string>();
        foreach (var (key, value) in results)
            lines.Add(CsvExporter.GetLine(key, StringFunction.StringsToString(value, ", ")));
        var line = string.Join(Environment.NewLine, lines);

        var encoding = new UTF8Encoding(true);

        // Convert the string to bytes
        var preamble = encoding.GetPreamble();
        await ms.WriteAsync(preamble);
        var textBytes = encoding.GetBytes(line);
        await ms.WriteAsync(textBytes);
        ms.Position = 0;
    }
}