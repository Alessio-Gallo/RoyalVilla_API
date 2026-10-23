
using System.ComponentModel.DataAnnotations;

namespace RoyalVilla.DTO;

public class VillaAmenitiesCreateDTO
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    public string? Description { get; set; }
    [Required]
    public int VillaId { get; set; }
}
