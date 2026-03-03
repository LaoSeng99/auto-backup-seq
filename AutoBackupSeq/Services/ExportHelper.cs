using AutoBackupSeq.Models;
using System.Text;

namespace AutoBackupSeq.Services;

public interface IExportHelper
{
    void ExportToCsv(List<RequestLog> data, string filePath);
    void ExportToHtml(List<RequestLog> data, string filePath);
    string ZipExportDirectory(string exportDirectory);
}

public class ExportHelper : IExportHelper
{
    public void ExportToCsv(List<RequestLog> data, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,Route,Method,StatusCode,Duration,StaffId,CompanyId");

        foreach (var item in data)
        {
            sb.AppendLine($"{item.Timestamp:yyyy-MM-dd HH:mm:ss},{item.Route},{item.Method},{item.StatusCode},{item.Duration},{item.StaffId},{item.CompanyId}");
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    public void ExportToHtml(List<RequestLog> data, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='UTF-8'><style>table{border-collapse:collapse;width:100%;}th,td{border:1px solid #ccc;padding:8px;text-align:left;}th{background:#f4f4f4;}</style></head><body>");
        sb.AppendLine("<h2>Request Log Analysis Report</h2>");
        sb.AppendLine("<table><thead><tr><th>Timestamp</th><th>Route</th><th>Method</th><th>Status</th><th>Duration (ms)</th><th>StaffId</th><th>CompanyId</th></tr></thead><tbody>");

        foreach (var item in data)
        {
            sb.AppendLine($"<tr><td>{item.Timestamp:yyyy-MM-dd HH:mm:ss}</td><td>{item.Route}</td><td>{item.Method}</td><td>{item.StatusCode}</td><td>{item.Duration}</td><td>{item.StaffId}</td><td>{item.CompanyId}</td></tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    public string ZipExportDirectory(string exportDirectory)
    {
        if (!Directory.Exists(exportDirectory))
        {
            Console.WriteLine("❌ Export directory not found.");
            return string.Empty;
        }

        var zipFile = Path.Combine(exportDirectory, $"analysis_export_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

        try
        {
            var tempDir = Path.Combine(exportDirectory, "_tempZip");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            foreach (var file in Directory.GetFiles(exportDirectory))
            {
                var name = Path.GetFileName(file);
                if (!name.EndsWith(".zip"))
                    File.Copy(file, Path.Combine(tempDir, name), overwrite: true);
            }

            System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, zipFile);
            Directory.Delete(tempDir, true);

            Console.WriteLine($"📦 Exported ZIP: {zipFile}");
            return zipFile;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to create zip: {ex.Message}");
            return string.Empty;
        }
    }
}
