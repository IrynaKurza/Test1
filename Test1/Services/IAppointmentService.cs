using Test1.Models.DTOs;

namespace Test1.Services;

public interface IAppointmentService
{
    Task<AppointmentResponseDto> GetAppointmentByIdAsync(int id);
    Task CreateAppointmentAsync(NewAppointmentDto dto);
}