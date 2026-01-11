using System.Text.RegularExpressions;
using RobotSim.Server.Models;

namespace RobotSim.Server.Services
{
    /*
     * RobotSimulator
     *
     * - Simulates a toy robot on a 6x6 table (coordinates 0..5).
     * - Accepts textual commands via ProcessCommand and returns a CommandResult.
     *
     * Notes:
     * - PLACE X,Y,DIRECTION or PLACE X,Y (direction optional if robot already placed).
     * - All other commands are ignored until a valid PLACE has been executed.
     * - Prevents moves that would fall off the table (they are ignored).
     *
     * Implementation details:
     * - A simple regex is used to parse PLACE; this keeps the interface textual and flexible.
     * - The class is intentionally small and synchronous — easy to test.
     * - If you register this type as a singleton in DI, state persists across requests (useful for the SPA).
     *   If you want stateless operation (every API call independent) register transient and pass an identifier for session.
     */

    public class RobotSimulator
    {
        private const int MaxX = 5;
        private const int MaxY = 5;

        // Whether robot has been placed already. Other commands ignored until true.
        private bool _placed = false;

        // Current position & direction (valid once _placed == true)
        private Position _pos = new Position(0, 0);
        private Direction _dir = Direction.NORTH;

        // Allow optional whitespace and optional direction.
        // Example matches: "PLACE 1,2,NORTH" or "PLACE 3,4"
        private static readonly Regex PlaceRegex =
            new Regex(@"^PLACE\s+(-?\d+)\s*,\s*(-?\d+)(?:\s*,\s*([A-Za-z]+))?$",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public CommandResult ProcessCommand(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new CommandResult { Success = false, Message = "Empty command." };
            }

            var cmd = raw.Trim();

            // Handle PLACE specially because it can include parameters.
            var m = PlaceRegex.Match(cmd);
            if (m.Success)
            {
                if (!int.TryParse(m.Groups[1].Value, out var x) || !int.TryParse(m.Groups[2].Value, out var y))
                {
                    return new CommandResult { Success = false, Message = "Invalid PLACE coordinates." };
                }

                // Reject placements outside table boundaries.
                if (x < 0 || x > MaxX || y < 0 || y > MaxY)
                {
                    return new CommandResult { Success = false, Message = "PLACE would put robot outside the table; command discarded." };
                }

                // If a direction token was provided, it must be valid.
                var dirGroup = m.Groups[3];
                if (dirGroup.Success)
                {
                    // Direction token provided explicitly — require it to parse.
                    if (DirectionExtensions.TryParse(dirGroup.Value, out var parsedDir))
                    {
                        _dir = parsedDir;
                    }
                    else
                    {
                        // Explicitly reject invalid direction tokens per spec ("discard invalid commands/parameters").
                        return new CommandResult { Success = false, Message = "Invalid PLACE direction." };
                    }
                }
                else
                {
                    // No direction token present.
                    if (!_placed)
                    {
                        // If the robot has never been placed with a direction, a direction-less PLACE is invalid.
                        return new CommandResult { Success = false, Message = "PLACE without direction ignored until robot has been placed with direction." };
                    }
                    // else keep existing _dir
                }

                _pos = new Position(x, y);
                _placed = true;

                return new CommandResult { Success = true, Message = $"Placed at {_pos} facing {_dir}." };
            }

            // If robot is not placed, other commands are ignored.
            if (!_placed)
            {
                return new CommandResult { Success = false, Message = "Robot not yet placed. Commands ignored until a valid PLACE." };
            }

            switch (cmd.ToUpperInvariant())
            {
                case "MOVE":
                    return Move();
                case "LEFT":
                    _dir = _dir.RotateLeft();
                    return new CommandResult { Success = true, Message = $"Rotated left to {_dir}." };
                case "RIGHT":
                    _dir = _dir.RotateRight();
                    return new CommandResult { Success = true, Message = $"Rotated right to {_dir}." };
                case "REPORT":
                    var report = $"{_pos.X},{_pos.Y},{_dir}";
                    return new CommandResult { Success = true, Message = "REPORT", Report = report };
                default:
                    return new CommandResult { Success = false, Message = "Invalid command." };
            }
        }

        private CommandResult Move()
        {
            int nx = _pos.X, ny = _pos.Y;
            switch (_dir)
            {
                case Direction.NORTH: ny++; break;
                case Direction.SOUTH: ny--; break;
                case Direction.EAST: nx++; break;
                case Direction.WEST: nx--; break;
            }

            if (nx < 0 || nx > MaxX || ny < 0 || ny > MaxY)
            {
                return new CommandResult { Success = false, Message = "Move would fall off table; command ignored." };
            }

            _pos = new Position(nx, ny);
            return new CommandResult { Success = true, Message = $"Moved to {_pos}." };
        }

        // Helper for tests: reset state
        public void Reset()
        {
            _placed = false;
            _pos = new Position(0, 0);
            _dir = Direction.NORTH;
        }
    }
}