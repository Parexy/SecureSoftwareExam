using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JournalController : ControllerBase
    {
        /*private IRepository<Order> ordersRepository;
        private IOrderManager ordersManager;

        public OrdersController(IRepository<Order> orderRepos, IOrderManager manager)
        {
            ordersRepository = orderRepos;
            ordersManager = manager;
        }*/

        // GET: journals
        [HttpGet(Name = "GetJournals")]
        public async Task<IActionResult> Get()
        {
            var journals = await ordersRepository.GetAllAsync();

            if (journals == null)
                return NotFound();

            var result = journals.Select(j => new JournalDTO
            {
                Id = j.Id,

                Documents = j.Documents.Select(oi => new PatientDocumentDTO
                {
                    Id = oi.Id,
                }).ToList()
            });

            return Ok(result);
        }

        
    }
}
