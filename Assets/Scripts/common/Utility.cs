using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    /// <summary>A coroutine that delays the action for the amount of time</summary>
    public static IEnumerator Delay(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    /// <summary>Calculate PlayTime string</summary>
    public static string SecToTime(int totalSeconds)
    {
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        string timeString = string.Format("{0}:{1:D2}:{2:D2}", hours, minutes, seconds);
        return timeString;
    }
}