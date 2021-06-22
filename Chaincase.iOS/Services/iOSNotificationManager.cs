﻿using System;
using Chaincase.Common.Contracts;
using UserNotifications;
using NotificationEventArgs = Chaincase.Common.Contracts.NotificationEventArgs;

namespace Chaincase.iOS.Services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "iOS is iOS")]
    public class iOSNotificationManager : INotificationManager
    {
        int messageId = -1;

        bool hasNotificationsPermission;

        public event EventHandler NotificationReceived;

        public void RequestAuthorization()
        {
            // request the permission to use local notifications
            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert, (granted, _) =>
            {
                hasNotificationsPermission = granted;

                //important because the user can, at any time, go into the Settings app and change their notification permissions
                if (granted)
                    AppDelegate.GetNotificationSettings();
            });
        }

        public int ScheduleNotification(string title, string message, double timeInterval)
        {
            // EARLY OUT: app doesn't have permissions
            //if (!hasNotificationsPermission)
            //{
            //	return -1;
            //}

            messageId++;

            var content = new UNMutableNotificationContent()
            {
                Title = title,
                Subtitle = "",
                Body = message,
                Badge = 1
            };

            // Local notifications can be time or location based
            // Create a time-based trigger, interval is in seconds and must be greater than 0
            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(timeInterval, false);

            var request = UNNotificationRequest.FromIdentifier(messageId.ToString(), content, trigger);
            UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
            {
                if (err != null)
                {
                    throw new Exception($"Failed to schedule notification: {err}");
                }
            });
            return messageId;
        }

        public void ReceiveNotification(string title, string message)
        {
            var args = new NotificationEventArgs()
            {
                Title = title,
                Message = message
            };
            NotificationReceived?.Invoke(null, args);
        }

        public void RemoveAllPendingNotifications()
        {
            UNUserNotificationCenter.Current.RemoveAllPendingNotificationRequests();
        }
    }
}
