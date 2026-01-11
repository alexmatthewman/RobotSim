namespace RobotSim.Server.Models
{
    public class CommandResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? Report { get; init; }
    }
}