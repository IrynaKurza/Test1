using System.ComponentModel.DataAnnotations;

namespace Test1.Models.DTOs;

public class NewAppointmentDto
{
    [Required]
    public int AppointmentId { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public string Pwz { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<ServiceRequestDto> Services { get; set; } = new();
}