using AutoBackupSeq;
using AutoBackupSeq.Models;
var config = ConfigLoader.Load("config.json");


while (true)
{
    CancellationTokenSource? _schedulerCts = null;
Task? _schedulerTask = null;

    Console.Clear();
    Console.OutputEncoding = System.Text.Encoding.UTF8;

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("""
╔════════════════════════════════════════════╗
║      🚀 AutoBackupSeq Console Toolkit      ║
╚════════════════════════════════════════════╝
Welcome to your centralized Seq log manager ✨
""");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Choose an option:");
    Console.ResetColor();

    Console.WriteLine("[1] 🔍 Filter logs from server (by time range)");
    Console.WriteLine("[2] 📂 Read and analyze local files");
    Console.WriteLine("[3] 🛠️ Start background scheduler (based on config)");
    Console.WriteLine("[4] 🧹 Clean old export/backup files");
    Console.WriteLine("[5] 🧪 Test Webhook (Send latest data file)");
    Console.WriteLine("[0] ❌ Exit application");
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await FilterByTime();
            break;

        case "2":
            var readPath = Path.Combine(config.BackupDirectory);
            if (!Directory.Exists(readPath) || !Directory.EnumerateFiles(readPath, "*.json").Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️  No log files found in the specified backup directory.");
                Console.ResetColor();
            }
            else
            {
                await SchedulerService.ReadFilesByDate(config);
            }
            break;

        case "3":
            if (_schedulerTask == null || _schedulerTask.IsCompleted)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("🟢 Scheduler is not running.");
                Console.Write("👉 Start background scheduler now? (Y/N): ");
                Console.ResetColor();

                var start = Console.ReadLine()?.Trim().ToLower();
                if (start == "y")
                {
                    _schedulerCts = new CancellationTokenSource();
                    _schedulerTask = Task.Run(() => SchedulerService.StartAsync(config, _schedulerCts.Token));
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Scheduler started in background.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("🟡 Scheduler is currently running.");
                Console.Write("👉 Do you want to stop it? (Y/N): ");
                Console.ResetColor();

                var stop = Console.ReadLine()?.Trim().ToLower();
                if (stop == "y")
                {
                    _schedulerCts?.Cancel();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("🛑 Scheduler stopping...");
                    Console.ResetColor();
                }
            }
            break;
        case "4":
            Console.Write("🧹 Delete files older than how many days? [default: 30]: ");
            var input = Console.ReadLine();
            int days = 30;
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out var parsed)) days = parsed;

            LogCleaner.CleanupOldFiles(config.BackupDirectory, days);
            LogCleaner.CleanupOldFiles(Path.Combine(AppContext.BaseDirectory, "exports"), days);
            break;
        case "5":
            await SchedulerService.TestWebhookAsync(config);
            break;
        case "0":
            Console.WriteLine("👋 Exiting the application...");
            Environment.Exit(0);
            break;
        default:
            Console.WriteLine("❌ Invalid selection.");
            break;
    }

    Console.ResetColor();

    Console.WriteLine("");
    Console.WriteLine("====================================");
    Console.WriteLine("Enter any key to continue, not 'esc'");
    Console.ReadKey();
}

async Task FilterByTime()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n📅 Please choose a time range to query:");

    var now = DateTime.UtcNow;
    var presets = new Dictionary<string, (DateTime Start, DateTime End)>
{
    { "1", (now.Date, now.Date.AddDays(1)) },                      // Today
    { "2", (now.Date.AddDays(-1), now.Date) },                     // Yesterday
    { "3", (now.AddHours(-1), now) },                              // Last 1 hour
    { "4", (now.AddHours(-6), now) },                              // Last 6 hours
    { "5", (now.AddHours(-12), now) }                              // Last 12 hours
};

    Console.WriteLine("[1] Today");
    Console.WriteLine("[2] Yesterday");
    Console.WriteLine("[3] Last 1 hour");
    Console.WriteLine("[4] Last 6 hours");
    Console.WriteLine("[5] Last 12 hours");
    Console.WriteLine("[M] Manually input start & end time");

    Console.Write("Choose preset (1-5 or M): ");
    var presetChoice = Console.ReadLine()?.Trim().ToLower();

    DateTime startTime, endTime;

    if (presetChoice != null && presets.TryGetValue(presetChoice, out var range))
    {
        startTime = range.Start;
        endTime = range.End;
    }
    else
    {
        Console.Write("Enter start time (e.g., 2025-04-17T00:00:00): ");
        if (!DateTime.TryParse(Console.ReadLine(), out startTime))
        {
            Console.WriteLine("❌ Invalid start time.");
            return;
        }

        Console.Write($"Enter end time (default +1 day = {startTime.AddDays(1):yyyy-MM-ddTHH:mm:ss}): ");
        var inputEnd = Console.ReadLine();
        endTime = string.IsNullOrWhiteSpace(inputEnd) ? startTime.AddDays(1) : DateTime.Parse(inputEnd);
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n⏳ Will fetch logs from {startTime:yyyy-MM-dd HH:mm} to {endTime:yyyy-MM-dd HH:mm}");
    Console.ResetColor();

    await SeqQuery.QueryAsync(config, startTime, endTime);
}

