using System.ComponentModel.DataAnnotations;

namespace Test1.Models.DTOs;

public class ServiceRequestDto
{
    [Required]
    public string ServiceName { get; set; } = string.Empty;

    [Required]
    public decimal ServiceFee { get; set; }
}