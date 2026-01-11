using System;
using RobotSim.Server.Services;
using Xunit;

namespace RobotSim.Server.Tests
{
    public class RobotSimulatorTests
    {
        [Fact]
        public void ExampleA_PlaceMoveReport()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            sim.ProcessCommand("PLACE 0,0,NORTH");
            sim.ProcessCommand("MOVE");
            var r = sim.ProcessCommand("REPORT");
            Assert.True(r.Success);
            Assert.Equal("0,1,NORTH", r.Report);
        }

        [Fact]
        public void ExampleB_PlaceLeftReport()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            sim.ProcessCommand("PLACE 0,0,NORTH");
            sim.ProcessCommand("LEFT");
            var r = sim.ProcessCommand("REPORT");
            Assert.True(r.Success);
            Assert.Equal("0,0,WEST", r.Report);
        }

        [Fact]
        public void ExampleC_MovesAndReport()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            sim.ProcessCommand("PLACE 1,2,EAST");
            sim.ProcessCommand("MOVE");
            sim.ProcessCommand("MOVE");
            sim.ProcessCommand("LEFT");
            sim.ProcessCommand("MOVE");
            var r = sim.ProcessCommand("REPORT");
            Assert.True(r.Success);
            Assert.Equal("3,3,NORTH", r.Report);
        }

        [Fact]
        public void ExampleD_PlaceOmittedDirectionBehavior()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            sim.ProcessCommand("PLACE 1,2,EAST");
            sim.ProcessCommand("MOVE");
            sim.ProcessCommand("LEFT");
            sim.ProcessCommand("MOVE");
            sim.ProcessCommand("PLACE 3,1"); // omitted direction -> should keep previous direction (NORTH)
            sim.ProcessCommand("MOVE");
            var r = sim.ProcessCommand("REPORT");
            Assert.True(r.Success);
            Assert.Equal("3,2,NORTH", r.Report);
        }

        [Fact]
        public void IgnoreCommandsBeforePlace()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            var res = sim.ProcessCommand("MOVE");
            Assert.False(res.Success);
            Assert.Contains("not yet placed", res.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void PreventFallingOffTable()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            sim.ProcessCommand("PLACE 5,5,NORTH");
            var r = sim.ProcessCommand("MOVE");
            Assert.False(r.Success);
            Assert.Contains("fall", r.Message, StringComparison.OrdinalIgnoreCase);
        }

        // New edge cases:

        [Fact]
        public void InvalidPlace_OutOfBounds_IsRejected()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            var r = sim.ProcessCommand("PLACE 9,9,NORTH");
            Assert.False(r.Success);
            Assert.Contains("outside", r.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void PlaceWithoutDirectionBeforePlaced_IsRejected()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            var r = sim.ProcessCommand("PLACE 2,2");
            Assert.False(r.Success);
            Assert.Contains("without direction", r.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CommandsAreCaseInsensitive()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            sim.ProcessCommand("place 0,0,north");
            sim.ProcessCommand("MoVe");
            var r = sim.ProcessCommand("report");
            Assert.True(r.Success);
            Assert.Equal("0,1,NORTH", r.Report);
        }

        // Additional edge cases:

        [Fact]
        public void InvalidDirection_IsRejected()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            var r = sim.ProcessCommand("PLACE 1,1,UPWARD");
            Assert.False(r.Success);
            Assert.Contains("Invalid", r.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void MultiplePlaceWithoutDirection_UsesPreviousDirection()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            sim.ProcessCommand("PLACE 0,0,EAST");
            sim.ProcessCommand("PLACE 2,2");
            var r = sim.ProcessCommand("REPORT");
            Assert.True(r.Success);
            Assert.Equal("2,2,EAST", r.Report);
        }

        [Fact]
        public void RotateRightFourTimes_ReturnsSameDirection()
        {
            var sim = new RobotSimulator();
            sim.Reset();
            sim.ProcessCommand("PLACE 1,1,NORTH");
            sim.ProcessCommand("RIGHT");
            sim.ProcessCommand("RIGHT");
            sim.ProcessCommand("RIGHT");
            sim.ProcessCommand("RIGHT");
            var r = sim.ProcessCommand("REPORT");
            Assert.True(r.Success);
            Assert.Equal("1,1,NORTH", r.Report);
        }
    }
}