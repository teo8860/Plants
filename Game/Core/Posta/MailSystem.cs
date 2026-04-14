using System;
using System.Collections.Generic;
using System.Linq;

namespace Plants;

public static class MailSystem
{
    private const string SAVE_FILE = "mail.json";
    private static MailInboxData _data;

    public static MailInboxData Data
    {
        get
        {
            if (_data == null) Load();
            return _data;
        }
    }

    public static List<MailMessage> Inbox => Data.inbox;

    public static void Load()
    {
        _data = SaveHelper.Load<MailInboxData>(SAVE_FILE) ?? new MailInboxData();
        if (_data.inbox == null) _data.inbox = new();
        if (_data.cooldowns == null) _data.cooldowns = new();
    }

    public static void Save()
    {
        SaveHelper.Save(SAVE_FILE, Data);
    }

    public static int UnreadCount => Inbox.Count(m => !m.claimed);

    /// <summary>
    /// Chiamare all'apertura del popup posta. Gestisce le mail ricorrenti:
    /// - Se il cooldown non e' ancora scaduto, non fa nulla.
    /// - Se scaduto e la mail esiste gia' in inbox come riscattata, la riattiva.
    /// - Se scaduto e non esiste, la crea nuova.
    /// </summary>
    public static void RefreshRecurringMails()
    {
        RefreshDailyMail(
            MailTemplates.DAILY_STARTER_SEED,
            MailTemplates.CreateDailyStarterSeed);

        Save();
    }

    private static void RefreshDailyMail(string templateId, Func<MailMessage> factory)
    {
        DateTime today = DateTime.Now.Date;

        if (Data.cooldowns.TryGetValue(templateId, out DateTime lastClaimed)
            && lastClaimed.Date >= today)
        {
            return;
        }

        MailMessage existing = Inbox.FirstOrDefault(m => m.templateId == templateId);

        if (existing != null)
        {
            if (existing.claimed)
            {
                existing.claimed = false;
                existing.claimedAt = null;
                existing.receivedAt = DateTime.Now;
            }
            return;
        }

        Inbox.Add(factory());
    }

    /// <summary>
    /// Riscatta una singola mail e applica le ricompense.
    /// Ritorna la lista delle ricompense applicate (vuota se gia' riscattata).
    /// </summary>
    public static List<MailReward> ClaimMail(MailMessage mail)
    {
        List<MailReward> applied = new();
        if (mail == null || mail.claimed) return applied;

        foreach (MailReward reward in mail.rewards)
        {
            ApplyReward(reward);
            applied.Add(reward);
        }

        mail.claimed = true;
        mail.claimedAt = DateTime.Now;

        if (mail.recurring && !string.IsNullOrEmpty(mail.templateId))
        {
            Data.cooldowns[mail.templateId] = DateTime.Now;
        }

        Save();
        return applied;
    }

    /// <summary>
    /// Riscatta tutte le mail non riscattate. Ritorna tutte le ricompense applicate.
    /// </summary>
    public static List<MailReward> ClaimAll()
    {
        List<MailReward> all = new();
        foreach (MailMessage mail in Inbox.Where(m => !m.claimed).ToList())
        {
            all.AddRange(ClaimMail(mail));
        }
        return all;
    }

    /// <summary>
    /// Rimuove tutte le mail gia' riscattate dall'inbox.
    /// I cooldown restano attivi.
    /// </summary>
    public static int ClearClaimed()
    {
        int removed = Inbox.RemoveAll(m => m.claimed);
        if (removed > 0) Save();
        return removed;
    }

    private static void ApplyReward(MailReward reward)
    {
        switch (reward.type)
        {
            case MailRewardType.StarterSeed:
                for (int i = 0; i < reward.amount; i++)
                    Inventario.get().AddSeed(StarterSeedSystem.CreateStarterSeed());
                break;

            case MailRewardType.Essence:
                SeedUpgradeSystem.SetEssence(SeedUpgradeSystem.Essence + reward.amount);
                break;

            case MailRewardType.Leaves:
                if (Game.pianta != null)
                    Game.pianta.Stats.FoglieAccumulate += reward.amount;
                break;
        }
    }

    public static string FormatReward(MailReward reward)
    {
        return reward.type switch
        {
            MailRewardType.StarterSeed => $"Seme Iniziale x{reward.amount}",
            MailRewardType.Essence => $"{reward.amount} Essenza",
            MailRewardType.Leaves => $"{reward.amount} Foglie",
            _ => $"Ricompensa x{reward.amount}"
        };
    }
}
