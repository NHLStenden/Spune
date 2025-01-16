//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Runtime.InteropServices.JavaScript;

namespace Spune.Common.Functions;

/// <summary>
/// This class contains helper functions used for various operations such as file downloads.
/// </summary>
public static partial class HelperFunction
{
    /// <summary>
    /// Downloads a file with the specified name, MIME type, and content.
    /// </summary>
    /// <param name="fileName">The name of the file to download.</param>
    /// <param name="fileMimeType">The MIME type of the file.</param>
    /// <param name="fileContents">The content of the file, represented as a byte array.</param>
    [JSImport("globalThis.spuneDownloadFile")]
    internal static partial void DownloadFile([JSMarshalAs<JSType.String>] string fileName,
        [JSMarshalAs<JSType.String>] string fileMimeType,
        [JSMarshalAs<JSType.Array<JSType.Number>>]
        byte[] fileContents);
}