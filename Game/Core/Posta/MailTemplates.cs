using System;

namespace Plants;

public static class MailTemplates
{
    public const string DAILY_STARTER_SEED = "daily_starter_seed";

    public static MailMessage CreateDailyStarterSeed()
    {
        return new MailMessage
        {
            templateId = DAILY_STARTER_SEED,
            title = "Seme Iniziale Giornaliero",
            description = "Un seme iniziale per ricominciare la tua coltura.",
            senderName = "Il Giardiniere",
            receivedAt = DateTime.Now,
            claimed = false,
            recurring = true,
            rewards = new()
            {
                new MailReward { type = MailRewardType.StarterSeed, amount = 1 }
            }
        };
    }
}
