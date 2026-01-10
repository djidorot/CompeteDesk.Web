using System;
using System.Collections.Generic;
using System.Linq;

namespace CompeteDesk.ViewModels.Dashboard
{
    public sealed class DashboardViewModel
    {
        public string WorkspaceName { get; set; } = "My Workspace";
        public string UserDisplayName { get; set; } = "User";

        // Command header
        public string StrategyMode { get; set; } = "Growth"; // Offensive / Defensive / Growth / Stability
        public int ActiveStrategiesCount { get; set; }
        public int StrategyScore { get; set; } // 0-100
        public string HealthStatus { get; set; } = "On Track"; // On Track / At Risk / Off Track
        public string WeeklyFocus { get; set; } = "Build systems, not goals.";

        // Panels
        public List<TodayActionItem> TodayActions { get; set; } = new();
        public List<HabitItem> Habits { get; set; } = new();
        public List<StrategyCardItem> ActiveStrategies { get; set; } = new();
        public List<MetricKpiItem> KeyMetrics { get; set; } = new();

        // Weekly review (simple)
        public string? WeeklyReviewHighlight { get; set; }
        public string? WeeklyReviewFailure { get; set; }
        public string? WeeklyReviewAdjustment { get; set; }

        public static DashboardViewModel Sample(string userDisplayName)
        {
            var vm = new DashboardViewModel
            {
                WorkspaceName = "CompeteDesk Demo Workspace",
                UserDisplayName = string.IsNullOrWhiteSpace(userDisplayName) ? "Strategist" : userDisplayName,
                StrategyMode = "Growth",
                ActiveStrategiesCount = 4,
                StrategyScore = 82,
                HealthStatus = "On Track",
                WeeklyFocus = "Make the right actions easy — and automatic."
            };

            vm.TodayActions.AddRange(new[]
            {
                new TodayActionItem { Title = "Publish 1 high-leverage post", Principle = "Atomic Habits • Make it obvious", Impact = "High", Minutes = 25 },
                new TodayActionItem { Title = "Review competitor pricing page", Principle = "33 Strategies • Intelligence", Impact = "Medium", Minutes = 15 },
                new TodayActionItem { Title = "Send 10 outreach messages", Principle = "Atomic Habits • Reduce friction", Impact = "High", Minutes = 30 },
            });

            vm.Habits.AddRange(new[]
            {
                new HabitItem { Name = "Daily KPI Review", StreakDays = 7, Status = "Stable", Cue = "Open dashboard", Environment = "Pinned tab + homepage", Notes = "Keep the KPI widget visible." },
                new HabitItem { Name = "Content Publishing", StreakDays = 14, Status = "Improving", Cue = "After lunch", Environment = "Draft templates ready", Notes = "Batch ideas on Sunday." },
                new HabitItem { Name = "Customer Feedback Loop", StreakDays = 2, Status = "Weak", Cue = "After support tickets", Environment = "Feedback form link", Notes = "Add frictionless 1-question survey." },
            });

            vm.ActiveStrategies.AddRange(new[]
            {
                new StrategyCardItem { Name = "Systems Over Goals", SourceBook = "Atomic Habits", CorePrinciple = "Design the system", ExecutionRate = 83, Effectiveness = "High" },
                new StrategyCardItem { Name = "Habit Stacking for Sales", SourceBook = "Atomic Habits", CorePrinciple = "After X, I do Y", ExecutionRate = 76, Effectiveness = "Medium" },
                new StrategyCardItem { Name = "War Room Intelligence", SourceBook = "33 Strategies of War", CorePrinciple = "Gather intel before acting", ExecutionRate = 65, Effectiveness = "Medium" },
                new StrategyCardItem { Name = "Defensive Positioning", SourceBook = "33 Strategies of War", CorePrinciple = "Secure key territory", ExecutionRate = 92, Effectiveness = "High" },
            });

            vm.KeyMetrics.AddRange(new[]
            {
                new MetricKpiItem { Name = "Leads", Value = "48", Trend = "+12%", TrendDirection = "up", Subtext = "Last 7 days" },
                new MetricKpiItem { Name = "Conversion", Value = "6.2%", Trend = "+0.8%", TrendDirection = "up", Subtext = "Landing page" },
                new MetricKpiItem { Name = "Revenue", Value = "₱12,400", Trend = "-3%", TrendDirection = "down", Subtext = "Week to date" },
                new MetricKpiItem { Name = "Output", Value = "9", Trend = "+2", TrendDirection = "up", Subtext = "Assets shipped" },
            });

            vm.WeeklyReviewHighlight = "Outreach system worked when it was scheduled immediately after KPI review.";
            vm.WeeklyReviewFailure = "Feedback loop broke because it required too many steps for customers.";
            vm.WeeklyReviewAdjustment = "Add a 1-click feedback link in every receipt email + track responses weekly.";

            return vm;
        }
    }

    public sealed class TodayActionItem
    {
        public string Title { get; set; } = "";
        public string Principle { get; set; } = "";
        public string Impact { get; set; } = "Medium"; // Low/Medium/High
        public int Minutes { get; set; }
        public bool Done { get; set; }
    }

    public sealed class HabitItem
    {
        public string Name { get; set; } = "";
        public int StreakDays { get; set; }
        public string Status { get; set; } = "Stable"; // Stable/Improving/Weak
        public string Cue { get; set; } = "";
        public string Environment { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    public sealed class StrategyCardItem
    {
        public string Name { get; set; } = "";
        public string SourceBook { get; set; } = "";
        public string CorePrinciple { get; set; } = "";
        public int ExecutionRate { get; set; } // 0-100
        public string Effectiveness { get; set; } = "Medium"; // Low/Medium/High
    }

    public sealed class MetricKpiItem
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
        public string Trend { get; set; } = "";
        public string TrendDirection { get; set; } = "flat"; // up/down/flat
        public string Subtext { get; set; } = "";
    }
}
