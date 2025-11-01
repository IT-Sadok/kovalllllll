using System.ComponentModel.DataAnnotations.Schema;

namespace DroneBuilder.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}