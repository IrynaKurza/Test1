namespace Test1.Models.DTOs;

public class AppointmentResponseDto
{
    public DateTime Date { get; set; }
    public PatientInfoDto Patient { get; set; } = null!;
    public DoctorInfoDto Doctor { get; set; } = null!;
    public List<ServiceInfoDto> AppointmentServices { get; set; } = new();
}