using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using Event = Unity.Services.Analytics.Event;

namespace Analytics
{
    public static class AnalyticsManager
    {
        private static readonly List<Event> OnInitialized = new();

        public static void Init()
        {
            var options = new InitializationOptions();
#if UNITY_EDITOR
            options.SetEnvironmentName("development");
#endif
            UnityServices.InitializeAsync(options);
            
            UnityServices.Initialized += () =>
            {
                AnalyticsService.Instance.StartDataCollection();

                OnInitialized.ForEach(AnalyticsService.Instance.RecordEvent);
            };
        }
        
        public static void LogEvent(Event evt)
        {
            if (UnityServices.State != ServicesInitializationState.Initialized) OnInitialized.Add(evt);
            else AnalyticsService.Instance.RecordEvent(evt);
        }
    }
}