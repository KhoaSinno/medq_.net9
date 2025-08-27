namespace Medq.Domain.Entities;

public sealed class Clinic
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
}
