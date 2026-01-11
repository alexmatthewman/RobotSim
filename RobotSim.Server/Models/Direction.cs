namespace RobotSim.Server.Models
{
    public enum Direction
    {
        NORTH,
        EAST,
        SOUTH,
        WEST
    }

    public static class DirectionExtensions
    {
        public static Direction RotateLeft(this Direction d) =>
            (Direction)(((int)d + 3) % 4);

        public static Direction RotateRight(this Direction d) =>
            (Direction)(((int)d + 1) % 4);

        public static bool TryParse(string s, out Direction direction)
        {
            direction = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            return Enum.TryParse<Direction>(s.Trim().ToUpperInvariant(), out direction);
        }
    }
}