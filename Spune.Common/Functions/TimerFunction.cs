//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Avalonia.Threading;

namespace Spune.Common.Functions;

/// <summary>This class contains a collection of timer level functions.</summary>
public static class TimerFunction
{
    /// <summary>
    /// List with invoke keys.
    /// </summary>
    static readonly List<object> InvokeKeys = [];

    /// <summary>
    /// List with delay invoke keys.
    /// </summary>
    static readonly List<(object, DispatcherTimer)> DelayInvokeKeys = [];

    /// <summary>
    /// Invokes a given action periodically.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <param name="milliseconds">Milliseconds for the period. Must be greater than 0ms and less than 1h.</param>
    /// <param name="key">Optional key identifier: used to not reinitialize the process.</param>
    /// <param name="startDirectly">Set to true to call the action directly.</param>
    public static void Invoke(Action action, double milliseconds, object? key = null, bool startDirectly = false)
    {
        if (key != null)
        {
            if (InvokeKeys.FindIndex(x => x == key) >= 0)
                return;
            InvokeKeys.Add(key);
        }

        if (double.IsInfinity(milliseconds) || double.IsNaN(milliseconds) || milliseconds is <= 0.0e+00 or >= 3.6e+06)
            return;
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        var timeTimer = new DispatcherTimer { Interval = timeSpan };
        timeTimer.Tick += (_, _) => action();
        timeTimer.IsEnabled = true;
        if (startDirectly) action();
    }

    /// <summary>
    /// Invokes a given action periodically.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <param name="milliseconds">Milliseconds for the period. Must be greater than 0ms and less than 1h.</param>
    /// <param name="key">Optional key identifier: used to not reinitialize the process.</param>
    /// <param name="startDirectly">Set to true to call the action directly.</param>
    public static async Task InvokeAsync(Func<Task> action, double milliseconds, object? key = null,
        bool startDirectly = false)
    {
        if (key != null)
        {
            if (InvokeKeys.FindIndex(x => x == key) >= 0)
                return;
            InvokeKeys.Add(key);
        }

        if (double.IsInfinity(milliseconds) || double.IsNaN(milliseconds) || milliseconds is <= 0.0e+00 or >= 3.6e+06)
            return;
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        var timeTimer = new DispatcherTimer { Interval = timeSpan };
        timeTimer.Tick += async (_, _) => await action();
        timeTimer.IsEnabled = true;
        if (startDirectly) await action();
    }

    /// <summary>
    /// Delay invokes a given action.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <param name="milliseconds">Milliseconds to delay. Must be greater than 0ms and less than 1h.</param>
    /// <param name="key">Optional key identifier: used to not restart process within time frame.</param>
    /// <param name="resetIfReentry">Reset the timer if this call is a reentry.</param>
    public static void DelayInvoke(Action action, double milliseconds, object? key = null, bool resetIfReentry = false)
    {
        int index;
        if (key != null && (index = DelayInvokeKeys.FindIndex(x => x.Item1 == key)) >= 0)
        {
            if (!resetIfReentry)
                return;
            var timer = DelayInvokeKeys[index].Item2;
            timer.Stop();
            timer.Start();
        }

        if (double.IsInfinity(milliseconds) || double.IsNaN(milliseconds) || milliseconds >= 3.6e+06) return;
        if (milliseconds <= 0.0e+00)
        {
            if (key != null)
                DelayInvokeKeys.RemoveAll(x => x.Item1 == key);
            action();
            return;
        }

        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        var timeTimer = new DispatcherTimer
        {
            Interval = timeSpan
        };
        timeTimer.Tick += (sender, _) =>
        {
            if (sender is not DispatcherTimer tT) return;
            tT.Stop();
            if (key != null)
                DelayInvokeKeys.RemoveAll(x => x.Item1 == key);
            action();
        };
        if (key != null)
            DelayInvokeKeys.Add((key, timeTimer));
        timeTimer.IsEnabled = true;
    }

    /// <summary>
    /// Delay invokes a given action.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <param name="milliseconds">Milliseconds to delay. Must be greater than 0ms and less than 1h.</param>
    /// <param name="key">Optional key identifier: used to not restart process within time frame.</param>
    /// <param name="resetIfReentry">Reset the timer if this call is a reentry.</param>
    public static async Task DelayInvokeAsync(Func<Task> action, double milliseconds, object? key = null,
        bool resetIfReentry = false)
    {
        int index;
        if (key != null && (index = DelayInvokeKeys.FindIndex(x => x.Item1 == key)) >= 0)
        {
            if (!resetIfReentry)
                return;
            var timer = DelayInvokeKeys[index].Item2;
            timer.Stop();
            timer.Start();
        }

        if (double.IsInfinity(milliseconds) || double.IsNaN(milliseconds) || milliseconds >= 3.6e+06) return;
        if (milliseconds <= 0.0e+00)
        {
            if (key != null)
                DelayInvokeKeys.RemoveAll(x => x.Item1 == key);
            await action();
            return;
        }

        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        var timeTimer = new DispatcherTimer
        {
            Interval = timeSpan
        };
        timeTimer.Tick += async (sender, _) =>
        {
            if (sender is not DispatcherTimer tT) return;
            tT.Stop();
            if (key != null)
                DelayInvokeKeys.RemoveAll(x => x.Item1 == key);
            await action();
        };
        if (key != null)
            DelayInvokeKeys.Add((key, timeTimer));
        timeTimer.IsEnabled = true;
    }
}