namespace Progetto
{
    /// <summary>
    /// Gestisce le preferenze dell'utente per le notifiche Windows.
    /// Permette di abilitare/disabilitare tipi specifici di notifiche e configurare intervalli.
    /// </summary>
    public class NotificationPreferences
    {
        // Preferenze per ogni tipo di notifica
        public bool EnableWaterNotifications { get; set; } = true;
        public bool EnableHealthNotifications { get; set; } = true;
        public bool EnableParasiteNotifications { get; set; } = true;
        public bool EnableMilestoneNotifications { get; set; } = true;
        public bool EnableWeatherNotifications { get; set; } = true;
        public bool EnableSeedPackNotifications { get; set; } = true;
        public bool EnableWorldUnlockNotifications { get; set; } = true;
        public bool EnableInactivityReminders { get; set; } = true;

        // Configurazioni generali
        public int CheckIntervalSeconds { get; set; } = 30;
        public int InactivityReminderMinutes { get; set; } = 120; // 2 ore
        public bool ShowNotificationsWhenGameActive { get; set; } = false;

        // Configurazioni cooldown (minuti tra notifiche dello stesso tipo)
        public int WaterNotificationCooldownMinutes { get; set; } = 30;
        public int HealthNotificationCooldownMinutes { get; set; } = 20;
        public int ParasiteNotificationCooldownMinutes { get; set; } = 45;
        public int GeneralNotificationCooldownMinutes { get; set; } = 60;

        /// <summary>
        /// Resetta tutte le preferenze ai valori di default
        /// </summary>
        public void ResetToDefaults()
        {
            var defaults = new NotificationPreferences();

            EnableWaterNotifications = defaults.EnableWaterNotifications;
            EnableHealthNotifications = defaults.EnableHealthNotifications;
            EnableParasiteNotifications = defaults.EnableParasiteNotifications;
            EnableMilestoneNotifications = defaults.EnableMilestoneNotifications;
            EnableWeatherNotifications = defaults.EnableWeatherNotifications;
            EnableSeedPackNotifications = defaults.EnableSeedPackNotifications;
            EnableWorldUnlockNotifications = defaults.EnableWorldUnlockNotifications;
            EnableInactivityReminders = defaults.EnableInactivityReminders;

            CheckIntervalSeconds = defaults.CheckIntervalSeconds;
            InactivityReminderMinutes = defaults.InactivityReminderMinutes;
            ShowNotificationsWhenGameActive = defaults.ShowNotificationsWhenGameActive;

            WaterNotificationCooldownMinutes = defaults.WaterNotificationCooldownMinutes;
            HealthNotificationCooldownMinutes = defaults.HealthNotificationCooldownMinutes;
            ParasiteNotificationCooldownMinutes = defaults.ParasiteNotificationCooldownMinutes;
            GeneralNotificationCooldownMinutes = defaults.GeneralNotificationCooldownMinutes;

        }
    }
}