namespace FinanceDataTool
{
    // Maps the existing "System" table. Named SystemInfo because a class called
    // System would collide with the System namespace.
    public class SystemInfo
    {
        public long Id { get; set; }
        public long? DbVersion { get; set; }
        public long? LastUpdated { get; set; }
        public long? LastTimestamp { get; set; }
    }
}
