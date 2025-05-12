using Microsoft.AspNetCore.Mvc;
using Test1.Exceptions;
using Test1.Models.DTOs;
using Test1.Services;

namespace Test1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // GET api/appointments/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentByIdAsync(id);
                return Ok(result);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }

        
        // POST api/appointments
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] NewAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _appointmentService.CreateAppointmentAsync(dto);
                return Created($"/api/appointments/{dto.AppointmentId}", null);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }
    }
}