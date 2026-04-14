using System;
using System.Collections.Generic;

namespace Plants;

public enum MailRewardType
{
    StarterSeed,
    Essence,
    Leaves
}

public class MailReward
{
    public MailRewardType type { get; set; }
    public int amount { get; set; } = 1;
}

public class MailMessage
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string templateId { get; set; } = "";
    public string title { get; set; } = "";
    public string description { get; set; } = "";
    public string senderName { get; set; } = "";
    public DateTime receivedAt { get; set; } = DateTime.Now;
    public bool claimed { get; set; } = false;
    public DateTime? claimedAt { get; set; } = null;
    public bool recurring { get; set; } = false;
    public List<MailReward> rewards { get; set; } = new();
}

public class MailInboxData
{
    public List<MailMessage> inbox { get; set; } = new();
    public Dictionary<string, DateTime> cooldowns { get; set; } = new();
}
