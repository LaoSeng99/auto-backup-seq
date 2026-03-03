using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using System.Text;
using Xunit;

namespace AutoBackupSeq.Tests;

public class ExportHelperTests
{
    [Fact]
    public void ExportToCsv_CreatesValidCsvFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var data = new List<RequestLog>
        {
            new RequestLog { Timestamp = DateTime.Now, Route = "/test", Method = "GET", StatusCode = 200, Duration = 100, StaffId = "S1", CompanyId = "C1" }
        };
        var helper = new ExportHelper();

        try
        {
            // Act
            helper.ExportToCsv(data, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var content = File.ReadAllText(tempFile);
            Assert.Contains("Timestamp,Route,Method,StatusCode,Duration,StaffId,CompanyId", content);
            Assert.Contains("/test,GET,200,100,S1,C1", content);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToHtml_CreatesValidHtmlFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var data = new List<RequestLog>
        {
            new RequestLog { Timestamp = DateTime.Now, Route = "/test", Method = "GET", StatusCode = 200, Duration = 100, StaffId = "S1", CompanyId = "C1" }
        };
        var helper = new ExportHelper();

        try
        {
            // Act
            helper.ExportToHtml(data, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var content = File.ReadAllText(tempFile);
            Assert.Contains("<html>", content);
            Assert.Contains("<td>/test</td>", content);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
