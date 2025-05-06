using System.ComponentModel.DataAnnotations;

namespace lab08.Models.DTOs;

public class TripDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<CountryDto> Countries { get; set; } = new();
}

public class CountryDto
{
    public string Name { get; set; }
}

public class ClientTripDto : TripDto
{
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}

public class CreateClientDto
{
    [Required]
    [StringLength(120)]
    public string FirstName { get; set; }
    [Required]
    [StringLength(120)]
    public string LastName { get; set; }
    [Required]
    [StringLength(120)]
    public string Email { get; set; }
    [Required]
    [StringLength(120)]
    public string Telephone { get; set; }
    [Required]
    [StringLength(120)]
    public string Pesel { get; set; }
}