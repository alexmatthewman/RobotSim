# Toy Robot Simulator - Installation & Usage

## Requirements
- .NET 8 SDK
- Node.js 18+ and npm

## Installation
1. Clone the repository:

   git clone <repo-url>
   cd RobotSim

2. Install frontend dependencies:

   cd robotsim.client
   npm install

## Running locally

### Start backend

Open a terminal and run:

   cd RobotSim.Server
   dotnet run

The backend starts on HTTPS port (check output), commonly `https://localhost:7249`.

### Start frontend

Open a separate terminal and run:

   cd robotsim.client
   npm run dev

Vite dev server will proxy `/robot` API calls to the backend configured in `vite.config.js`.

Open the frontend at `http://localhost:5173`.

## Usage
- Type commands such as `PLACE 0,0,NORTH`, `MOVE`, `LEFT`, `RIGHT`, `REPORT` in the input box and press Go.
- The Output Log shows responses and errors with timestamps.

## Running tests

From solution root or test project folder:

   dotnet test ./RobotSim.Server.Tests

This runs the simulator unit tests (xUnit). Ensure the backend is not locking files when running tests.

