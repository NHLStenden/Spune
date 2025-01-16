//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Avalonia;
using Spune.UIShared;
using Spune.UIShared.Core;

RunningStory.EmailSenderCreator = () => new Spune.ServiceBase.Service.EmailSender();

AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace().StartWithClassicDesktopLifetime(args);