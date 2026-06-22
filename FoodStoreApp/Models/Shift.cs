using System;

namespace MMarketApp.Models
{
    public class Shift
    {
        public int ShiftID { get; set; }
        public int UserID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}