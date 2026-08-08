using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Services;

public class InterludeCalculator : IInterludeCalculator
{
    public InterludeProjection Project(
        GuildDerivedStats derived,
        IReadOnlyList<ResearchProject> research,
        IReadOnlyList<CraftingOrder> crafting,
        int days)
    {
        var indicators = new List<InterludeIndicator>
        {
            new()
            {
                Kind = "Maintenance",
                Label = "Maintenance",
                SilverDelta = ClampToInt(-(long)derived.DailyMaintenance * days),
                Description = $"{days}d × {derived.DailyMaintenance}/d",
            },
            new()
            {
                Kind = "Income",
                Label = "Income",
                SilverDelta = ClampToInt((long)derived.WorkerIncomePerDay * days),
                Description = $"{days}d × {derived.WorkerIncomePerDay}/d",
            },
        };

        foreach (var r in research.Where(r => !r.IsComplete))
        {
            var perDay = Math.Min(Math.Max(1, r.Researchers), 2);          // min(R,2), floor 1
            var remaining = Math.Max(0, r.RequiredDays - r.ProgressDays);
            var added = (int)Math.Min(remaining, (long)perDay * days);
            var willComplete = r.ProgressDays + added >= r.RequiredDays;
            indicators.Add(new InterludeIndicator
            {
                Kind = "ResearchProgress", Label = r.Name, TargetId = r.Id,
                DaysAdded = added, WillComplete = willComplete,
                PointsAwarded = willComplete ? r.Points : 0,
                Description = $"+{added}d ({r.ProgressDays + added}/{r.RequiredDays})",
            });
        }

        foreach (var c in crafting.Where(c => c.Status == CraftingStatus.EmAndamento))
        {
            var remaining = Math.Max(0, c.RequiredDays - c.ProgressDays);
            var added = (int)Math.Min(remaining, (long)days);
            indicators.Add(new InterludeIndicator
            {
                Kind = "CraftingProgress", Label = c.ItemName, TargetId = c.Id,
                DaysAdded = added, WillComplete = c.ProgressDays + added >= c.RequiredDays,
                Description = $"+{added}d ({c.ProgressDays + added}/{c.RequiredDays})",
            });
        }

        return new InterludeProjection { Days = days, Indicators = indicators };
    }

    private static int ClampToInt(long v) => (int)Math.Clamp(v, int.MinValue, int.MaxValue);
}
