//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using Spune.UIShared;

[assembly: SupportedOSPlatform("browser")]

await AppBuilder.Configure<App>().WithInterFont().StartBrowserAppAsync("out");