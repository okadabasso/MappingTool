using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Data
{
    internal class DestinationData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public bool IsActive { get; set; }
        public double Score { get; set; }
        public decimal Balance { get; set; }
        public Guid Token { get; set; }
        public string Description { get; set; }
        public int Age { get; set; }
        public string Category { get; set; }
        public DateTime? Expiration { get; set; }
        public bool IsDeleted { get; set; }
        public long TotalCount { get; set; }
        public float Ratio { get; set; }
        public short Level { get; set; }
        public byte Status { get; set; }
        public char Initial { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Uri Website { get; set; }
    }
}
