# Toy Robot Simulator - Setup & Running Instructions

## Overview
This is a toy robot simulator web application with:
- A React + Vite frontend (robotsim.client)
- An ASP.NET Core 8 backend (RobotSim.Server)
- A 6x6 grid-based robot simulator
- Green ultra-tech UI theme with light/dark mode support
- Mobile-responsive design

## Prerequisites
- Node.js 18+ (for npm)
- .NET 8 SDK
- Visual Studio or Visual Studio Code

## Backend Setup & Running

### From Visual Studio
1. Open the solution in Visual Studio
2. Right-click the solution and select "Set Startup Projects"
3. Select both "RobotSim.Server" and "robotsim.client" as startup projects
4. Press F5 to run (will launch backend on https://localhost:7249)

### From Command Line
```powershell
cd RobotSim.Server
dotnet run
# Backend runs on http://localhost:5120 (dev mode uses HTTP to avoid SSL cert issues)
```

## Frontend Setup & Running

### From Command Line
```powershell
cd robotsim.client
npm install
npm run dev
# Frontend runs on http://localhost:5173
```

The Vite dev server is configured to proxy `/robot/` API calls to the backend (http://127.0.0.1:5120).

## Quick Start (Recommended for local development)
A convenience PowerShell script is included at the repository root to start the backend first, verify it's healthy, then start the frontend dev server and verify it is serving the site.

Usage (from repository root):
```powershell
PowerShell.exe -ExecutionPolicy Bypass -File .\start-dev.ps1
```
The script will:
- Run `dotnet run --launch-profile http` in `RobotSim.Server`
- Wait until `http://127.0.0.1:5120/robot/health` responds
- Run `npm run dev` in `robotsim.client`
- Wait until `http://127.0.0.1:5173/` responds
- Print the two URLs and keep running while both processes are active

This is the recommended way to run both services locally when you're not using Visual Studio's SPA proxy.

## Important Notes

### Robot Image
- Place a file named `robot.jpg` in `robotsim.client/public/` folder
- If the folder doesn't exist, create it: `robotsim.client/public/`
- The image will be displayed at 120px height in the UI

### Port Configuration
- Backend HTTPS: `https://localhost:7249` (from launchSettings.json)
- Frontend dev: `http://localhost:5173`
- Vite proxy automatically forwards `/robot/...` requests to the backend

### CORS
The backend has CORS enabled for the Vite dev server (http://localhost:5173) to allow API requests.

## Running Tests

From the solution root or RobotSim.Server.Tests folder:

```powershell
dotnet test ./RobotSim.Server.Tests
```

Or from Visual Studio:
1. Open Test Explorer (Test > Test Explorer)
2. Run all tests in `RobotSim.Server.Tests`

All 12 tests should pass (examples a-d, plus edge cases).

## Key Features

### Simulator Commands
- `PLACE X,Y,DIRECTION` - Place robot at (X,Y) facing NORTH|SOUTH|EAST|WEST
- `MOVE` - Move one unit forward
- `LEFT` - Rotate 90° left
- `RIGHT` - Rotate 90° right
- `REPORT` - Output current position and direction

### UI Features
- Command input with validation hint
- Output log showing all command results with timestamps
- Command reference panel with example
- "Fill example" button to auto-fill: `PLACE 0,0,NORTH`
- Dark/light theme toggle
- Mobile-responsive design
- Green ultra-tech theme with neon accents

## Troubleshooting

### HTTP 404 Error on `/robot/command`
- Ensure the backend is running on https://localhost:7249
- Check that Vite proxy in `vite.config.js` points to the correct backend URL
- Verify the RobotController is present in RobotSim.Server/Controllers/

### HTTP 500 Error
- The backend now returns detailed error messages in JSON format
- Check the browser DevTools Network tab to see full error response
- Check the Visual Studio Output window for server logs

### Log text not white in dark mode
- This has been fixed with strong CSS overrides using `!important`
- If still not white, clear browser cache (Ctrl+Shift+Delete) and reload

### Robot image not showing
- Ensure `robot.jpg` exists in `robotsim.client/public/`
- The path in the HTML is `/robot.jpg` (served from public folder)

## Recent Changes

### CSS & UI
- Green ultra-tech theme with neon green (#00ff88) and cyan (#00d9ff) accents
- All text is now white in dark mode with `!important` enforcements
- Input and output in separate bordered panels
- Responsive grid layout (3 columns on desktop, 1 column on mobile)
- Enhanced spacing between panels (24px gap)
- Consistent font sizing throughout

### Backend
- Added CORS support for Vite dev server
- Added global exception handler returning JSON errors
- All HTTP errors now include detailed messages
- Better error visibility in the frontend log

### Frontend
- Full HTTP error details shown in output log (status + body)
- Better error handling and response parsing
- Fill example button at top of command reference
- Improved mobile responsiveness

## Development Workflow

1. Start backend: Visual Studio (F5) or `dotnet run`
2. Start frontend: `npm run dev` in robotsim.client
3. Open http://localhost:5173 in browser
4. Type commands and press Go or Enter
5. Check Output Log for results

## Building for Production

### Backend
```powershell
cd RobotSim.Server
dotnet publish -c Release -o ./publish
```

### Frontend
```powershell
cd robotsim.client
npm run build
# Output goes to dist/ folder
```

Then copy the `dist/` contents to `RobotSim.Server/wwwroot/` for serving as static files.
