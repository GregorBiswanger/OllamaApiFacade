using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace OllamaApiFacade.DemoWebApi.Plugins;

public class TimeInformationPlugin
{
    [KernelFunction]
    [Description("Retrieves the current time and day in CET from germany.")]
    public string GetCurrentTimeAndDay()
    {
        // Zeitzone für Deutschland (Central European Time)
        TimeZoneInfo cetZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        DateTime germanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cetZone);

        // Formatierung der Ausgabe
        string time = germanTime.ToString("HH:mm:ss");
        string dayOfWeek = germanTime.ToString("dddd", new System.Globalization.CultureInfo("de-DE"));

        return $"Current time in germany: {time}, day: {dayOfWeek}";
    }
}