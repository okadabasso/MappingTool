using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Data
{
    internal class SourceData
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime Created { get; set; }
        public bool IsActive { get; set; }
        public double Score { get; set; }
        public decimal Balance { get; set; }
        public Guid Token { get; set; }
        public string Description { get; set; } = null!;
        public int Age { get; set; }
        public string Category { get; set; } = null!;
        public DateTime? Expiration { get; set; }
        public bool IsDeleted { get; set; }
        public long TotalCount { get; set; }
        public float Ratio { get; set; }
        public short Level { get; set; }
        public byte Status { get; set; }
        public char Initial { get; set; }
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public Uri Website { get; set; } = null!;
    }
}
