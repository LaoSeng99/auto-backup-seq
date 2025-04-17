using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBackupSeq.Models;

public class SeqEvent
{
    public string Id { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = default!;
    public List<SeqToken> MessageTemplateTokens { get; set; } = new();
    public List<SeqProperty> Properties { get; set; } = new();
}

public class SeqToken
{
    public string Text { get; set; } = default!;
}

public class SeqProperty
{
    public string Name { get; set; } = default!;
    public object? Value { get; set; }
}