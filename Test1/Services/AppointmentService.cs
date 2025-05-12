using Microsoft.Data.SqlClient;
using Test1.Exceptions;
using Test1.Models.DTOs;
using Test1.Services;

public class AppointmentService : IAppointmentService
{
    private readonly string _connectionString;
    public AppointmentService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }
    
    // GET
    public async Task<AppointmentResponseDto> GetAppointmentByIdAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand();
        
        var query = @"SELECT a.date, p.first_name, p.last_name, p.date_of_birth, d.doctor_id, d.pwz, s.name, sa.service_fee
                        FROM Appointment a 
                        JOIN Patient p ON p.patient_id = a.patient_id
                        JOIN Doctor d ON d.doctor_id = a.doctor_id
                        JOIN Appointment_Service sa ON sa.appointment_id = a.appointment_id
                        JOIN Service s ON s.service_id = sa.service_id 
                        WHERE appointment_Id = @id";

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);
        
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        AppointmentResponseDto? appointment = null;

        while (await reader.ReadAsync())
        {
            if (appointment == null)
            {
                appointment = new AppointmentResponseDto
                {
                    Date = reader.GetDateTime(0),
                    Patient = new PatientInfoDto
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3)
                    },
                    Doctor = new DoctorInfoDto
                    {
                        DoctorId = reader.GetInt32(4),
                        Pwz = reader.GetString(5)
                    },
                    AppointmentServices = new List<ServiceInfoDto>()
                };
            }

            appointment.AppointmentServices.Add(new ServiceInfoDto
            {
                Name = reader.GetString(6),
                ServiceFee = reader.GetDecimal(7)
            });
        }

        if (appointment is null)
            throw new NotFoundException("Appointment not found");

        return appointment;
        
    }
    
    
    
    //POST
    public Task CreateAppointmentAsync(NewAppointmentDto dto)
    {
        return null;
    }
    
}