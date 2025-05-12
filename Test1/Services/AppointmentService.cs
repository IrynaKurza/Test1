using Microsoft.Data.SqlClient;
using Test1.Exceptions;
using Test1.Models.DTOs;

namespace Test1.Services;

public class AppointmentService : IAppointmentService
{
    private readonly string _connectionString;

    public AppointmentService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }

    public async Task<AppointmentResponseDto> GetAppointmentByIdAsync(int id)
    {
        var query = @"
            SELECT a.date, p.first_name, p.last_name, p.date_of_birth, d.doctor_id, d.PWZ, s.name, aps.service_fee
            FROM appointment a
            JOIN patient p ON a.patient_id = p.patient_id
            JOIN doctor d ON a.doctor_id = d.doctor_id
            JOIN appointment_service aps ON a.appointment_id = aps.appointment_id
            JOIN service s ON aps.service_id = s.service_id
            WHERE a.appointment_id = @id";

        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await conn.OpenAsync();
        var reader = await cmd.ExecuteReaderAsync();

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

    public async Task CreateAppointmentAsync(NewAppointmentDto dto)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var transaction = await conn.BeginTransactionAsync();

        try
        {
            var checkCmd = new SqlCommand("SELECT 1 FROM appointment WHERE appointment_id = @id", conn, (SqlTransaction)transaction);
            checkCmd.Parameters.AddWithValue("@id", dto.AppointmentId);
            if (await checkCmd.ExecuteScalarAsync() is not null)
                throw new ConflictException("Appointment already exists");

            var checkPatient = new SqlCommand("SELECT 1 FROM patient WHERE patient_id = @id", conn, (SqlTransaction)transaction);
            checkPatient.Parameters.AddWithValue("@id", dto.PatientId);
            if (await checkPatient.ExecuteScalarAsync() is null)
                throw new NotFoundException("Patient not found");

            var getDoctorId = new SqlCommand("SELECT doctor_id FROM doctor WHERE PWZ = @pwz", conn, (SqlTransaction)transaction);
            getDoctorId.Parameters.AddWithValue("@pwz", dto.Pwz);
            var doctorIdObj = await getDoctorId.ExecuteScalarAsync();
            if (doctorIdObj is null)
                throw new NotFoundException("Doctor not found");
            int doctorId = (int)doctorIdObj;

            var insertAppointment = new SqlCommand(@"
                INSERT INTO appointment (appointment_id, patient_id, doctor_id, date)
                VALUES (@id, @patientId, @doctorId, GETDATE())", conn, (SqlTransaction)transaction);
            insertAppointment.Parameters.AddWithValue("@id", dto.AppointmentId);
            insertAppointment.Parameters.AddWithValue("@patientId", dto.PatientId);
            insertAppointment.Parameters.AddWithValue("@doctorId", doctorId);
            await insertAppointment.ExecuteNonQueryAsync();

            foreach (var service in dto.Services)
            {
                var getServiceId = new SqlCommand("SELECT service_id FROM service WHERE name = @name", conn, (SqlTransaction)transaction);
                getServiceId.Parameters.AddWithValue("@name", service.ServiceName);
                var serviceIdObj = await getServiceId.ExecuteScalarAsync();
                if (serviceIdObj is null)
                    throw new NotFoundException($"Service '{service.ServiceName}' not found");
                int serviceId = (int)serviceIdObj;

                var insertService = new SqlCommand(@"
                    INSERT INTO appointment_service (appointment_id, service_id, service_fee)
                    VALUES (@aid, @sid, @fee)", conn, (SqlTransaction)transaction);
                insertService.Parameters.AddWithValue("@aid", dto.AppointmentId);
                insertService.Parameters.AddWithValue("@sid", serviceId);
                insertService.Parameters.AddWithValue("@fee", service.ServiceFee);
                await insertService.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}