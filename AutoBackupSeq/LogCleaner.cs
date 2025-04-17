

namespace AutoBackupSeq;

public static class LogCleaner
{
    public static void CleanupOldFiles(string folderPath, int days = 30)
    {
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("❌ Log folder does not exist.");
            return;
        }

        var cutoff = DateTime.Now.AddDays(-days);
        var files = Directory.GetFiles(folderPath, "*.json")
            .Concat(Directory.GetFiles(folderPath, "*.csv"))
            .Concat(Directory.GetFiles(folderPath, "*.html"))
            .Where(f => File.GetLastWriteTime(f) < cutoff)
            .ToList();

        int count = 0;
        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
                count++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to delete {Path.GetFileName(file)}: {ex.Message}");
            }
        }

        Console.WriteLine($"🧹 Cleaned up {count} old file(s) older than {days} days.");
    }
}