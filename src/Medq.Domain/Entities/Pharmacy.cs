namespace Medq.Domain.Entities
{
    public sealed class Pharmacy
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Address { get; set; }
        public bool OpenNow { get; set; }
    }
}