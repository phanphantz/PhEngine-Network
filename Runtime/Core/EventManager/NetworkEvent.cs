using System;
using UnityEngine;

namespace PhEngine.Network
{
    public static class NetworkEvent
    {
        public static event Action<ClientRequest> OnAnyRequestSend;
        public static event Action<ClientRequest> OnShowLoading;
        public static event Action<ClientRequest> OnHideLoading;
        
        public static event Action<ServerResult> OnAnyResultReceived;
        public static event Action<ServerResult> OnAnyConnectionFail;
        public static event Action<ServerResult> OnShowConnectionFailError;
        public static event Action<ServerResult> OnAnyServerFail;
        public static event Action<ServerResult> OnShowServerFailError;
        public static event Action<ServerResult> OnAnyServerSuccess;
        
        public static event Action<DateTime> OnDayChanged;
        public static event Action<DateTime> OnTimeChanged;
        public static event Action<TimeSpan> OnCompensateTime;
        
        public static DateTime LatestServerTime { get; private set; }
        public static bool IsServerRepliedAtLeastOnce { get; private set; }
        
        internal static void InvokeOnShowLoading(ClientRequest request)
        {
            OnShowLoading?.Invoke(request);
        }

        internal static void InvokeOnHideLoading(ClientRequest request)
        {
            OnHideLoading?.Invoke(request);
        }

        internal static void InvokeOnAnyRequestSend(ClientRequest request)
        {
            OnAnyRequestSend?.Invoke(request);
        }

        internal static void InvokeOnAnyResultReceived(ServerResult result)
        {
            OnAnyResultReceived?.Invoke(result);
        }
        
        internal static void InvokeOnAnyConnectionFail(ServerResult result)
        {
            OnAnyConnectionFail?.Invoke(result);
        }
        
        internal static void InvokeOnAnyServerFail(ServerResult result)
        {
            OnAnyServerFail?.Invoke(result);
        }
        
        internal static void InvokeOnAnyServerSuccess(ServerResult result)
        {
            OnAnyServerSuccess?.Invoke(result);
        }
        
        internal static void InvokeOnShowConnectionFailError(ServerResult result)
        {
            OnShowConnectionFailError?.Invoke(result);
        }
        
        internal static void InvokeOnShowServerFailError(ServerResult result)
        {
            OnShowServerFailError?.Invoke(result);
        }

        public static void SetLatestServerTimeByString(string timeString)
        {
            if (!ValidateTimeString(timeString))
                return;

            var newTime = DateTime.Parse(timeString);
            SetLatestServerTime(newTime);
        }
        
        static bool ValidateTimeString(string timeString)
        {
            if (string.IsNullOrEmpty(timeString))
            {
                Debug.LogError("Cannot Set Update Latest Server Time. Received string is null or empty.");
                return false;
            }
            
            if (!DateTime.TryParse(timeString, out _))
            {
                Debug.LogError("Cannot Set Update Latest Server Time. Received string cannot be parsed to DateTime.");
                return false;
            }

            return true;
        }

        static void SetLatestServerTime(DateTime newTime)
        {
            NotifyTimeChange(newTime);
            TryCompensateTime(newTime);
            NotifyNewDayIfNeeded(newTime);
            NotifyTimeUpdateFinished(newTime);
        }

        static void NotifyTimeChange(DateTime newTime)
        {
            Debug.Log($"Receive Time From Server : {newTime}");
            OnTimeChanged?.Invoke(newTime);
        }

        static void TryCompensateTime(DateTime newTime)
        {
            var timeDiff = newTime - LatestServerTime;
            if (IsServerRepliedAtLeastOnce)
                CompensateTime(timeDiff);
        }

        static void NotifyNewDayIfNeeded(DateTime newTime)
        {
            var isNewDay = IsServerRepliedAtLeastOnce && IsNewDay(newTime);
            if (isNewDay)
                OnDayChanged?.Invoke(LatestServerTime);
        }
        
        static void NotifyTimeUpdateFinished(DateTime newTime)
        {
            IsServerRepliedAtLeastOnce = true;
            LatestServerTime = newTime;
        }

        static bool IsNewDay(DateTime dateTime)
        {
            return LatestServerTime.DayOfYear != dateTime.DayOfYear;
        }

        static void CompensateTime(TimeSpan timeSpanToAdd)
        {
            Debug.Log("Compensate Time from Server : +" + timeSpanToAdd);
            OnCompensateTime?.Invoke(timeSpanToAdd);
        }
    }
}