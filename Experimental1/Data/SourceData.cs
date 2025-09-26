using System;

namespace Experimental1.Data;

public class SourceData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public bool IsActive { get; set; }
    public double Score { get; set; }
    public decimal Balance { get; set; }
    public Guid Token { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime? Expiration { get; set; }
    public bool IsDeleted { get; set; }
    public long TotalCount { get; set; }
    public float Ratio { get; set; }
    public short Level { get; set; }
    public byte Status { get; set; }
    public char Initial { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Uri Website { get; set; } = new Uri("http://localhost");
}
