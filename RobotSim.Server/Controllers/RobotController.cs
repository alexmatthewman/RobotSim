using Microsoft.AspNetCore.Mvc;
using RobotSim.Server.Models;
using RobotSim.Server.Services;

namespace RobotSim.Server.Controllers
{
    // Expose endpoints under /robot
    [ApiController]
    [Route("robot")]
    public class RobotController : ControllerBase
    {
        private readonly RobotSimulator _sim;
        private readonly ILogger<RobotController> _logger;

        public RobotController(RobotSimulator sim, ILogger<RobotController> logger)
        {
            _sim = sim;
            _logger = logger;
        }

        public class CommandDto
        {
            public string? Command { get; set; }
        }

        // GET /robot/health - quick health check used during development
        [HttpGet("health")]
        public ActionResult<object> GetHealth()
        {
            return Ok(new { status = "ok", placed = _sim != null });
        }

        // POST /robot/command
        [HttpPost("command")]
        public ActionResult<CommandResult> PostCommand([FromBody] CommandDto dto)
        {
            _logger.LogInformation("POST /robot/command called. Body.Command='{cmd}'", dto?.Command);

            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Command))
                {
                    _logger.LogWarning("Missing command in request body.");
                    return BadRequest(new CommandResult { Success = false, Message = "Missing command." });
                }

                var result = _sim.ProcessCommand(dto.Command);
                _logger.LogInformation("Processed command '{cmd}' => Success={success} Message='{msg}'", dto.Command, result.Success, result.Message);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while processing command: {cmd}", dto?.Command);
                return StatusCode(500, new CommandResult { Success = false, Message = "Internal server error while processing command." });
            }
        }

        // POST /robot/commands  - convenience to send multiple commands (keeps state between calls)
        [HttpPost("commands")]
        public ActionResult<IEnumerable<CommandResult>> PostCommands([FromBody] IEnumerable<string> commands)
        {
            _logger.LogInformation("POST /robot/commands called. Count={count}", commands?.Count() ?? 0);
            try
            {
                if (commands == null) return BadRequest();
                var results = commands.Select(c => _sim.ProcessCommand(c)).ToArray();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while processing commands");
                return StatusCode(500, new[] { new CommandResult { Success = false, Message = "Internal server error while processing commands." } });
            }
        }
    }
}