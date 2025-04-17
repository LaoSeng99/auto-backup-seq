using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoBackupSeq.Models;

public class RequestLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // 方便入 DB 使用
    public DateTime Timestamp { get; set; }

    public string TraceId { get; set; } = "-";
    public string Route { get; set; } = "-";
    public string Method { get; set; } = "-";
    public string Query { get; set; } = "-";
    public string RequestBody { get; set; } = "";

    public string UserName { get; set; } = "-";
    public string StaffId { get; set; } = "-";
    public string CompanyId { get; set; } = "-";
    public string CompanyDepartmentId { get; set; } = "-";

    public string RemoteIP { get; set; } = "-";
    public string UserAgent { get; set; } = "-";

    public int StatusCode { get; set; }
    public long Duration { get; set; }
    public long RequestSize { get; set; }
    public long ResponseSize { get; set; }
}
