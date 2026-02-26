using System;

namespace Plants;

public static class WaterSystem
{
    public static float Current = 100f;
    public static float Max = 100f;

    private const float ConsumeRate = 10; // 1 unità al secondo
    private const float RechargeRate = 10f / (5f * 60f); // 10 ogni 5 minuti = ~0.0333/sec

    public static float FillPercent => Max > 0 ? Math.Clamp(Current / Max, 0f, 1f) : 0f;
    public static bool CanWater => Current > 0.1;

    public static void Consume(float deltaTime)
    {
        Current = Math.Max(0, Current - ConsumeRate * deltaTime);
    }

    public static void Recharge(float deltaTime)
    {
        Current = Math.Min(Max, Current + RechargeRate * deltaTime);
    }

    public static void Update(float deltaTime)
    {
        Recharge(deltaTime);
    }

    public static void AddOfflineRecharge(double offlineSeconds)
    {
        float gained = (float)(RechargeRate * offlineSeconds);
        Current = Math.Min(Max, Current + gained);
    }
}
