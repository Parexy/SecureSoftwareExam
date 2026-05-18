using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PatientController : ControllerBase
    {
        private IRepository<Order> ordersRepository;
        private IOrderManager ordersManager;

        public PatientController(IRepository<Patient> patientRepos)
        {
            ordersRepository = orderRepos;
            ordersManager = manager;
        }

        // GET: patients
        [HttpGet(Name = "GetPatients")]
        public async Task<IActionResult> Get()
        {
            var patients = await patientsRepository.GetAllAsync();

            if (patients == null)
                return NotFound();

            var result = patients.Select(j => new PatientDTO
            {
                Id = j.Id,
                CPRNumber = j.CPRNumber,
                Name = j.Name,
                Address = j.Address,
                PhoneNumber = j.PhoneNumber,
                Email = j.Email,
                DateOfBirth = j.DateOfBirth
            });

            return Ok(result);
        }

        
    }
}
