using Microsoft.AspNetCore.Mvc;
using Series.DocumentDB.Repository;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Series.DocumentDB.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // POST api/<UserController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string value)
        {
            await _userRepository.AddAsync(new Data.Entities.UserEntity { Id = Guid.NewGuid().ToString() });
            return Ok(value);
        }
    }
}
