# SkillSnap

A portfolio management application built with Blazor WebAssembly and ASP.NET Core API.

## Project Structure

- `SkillSnap_API/` - ASP.NET Core Web API backend
- `SkillSnap_Client/` - Blazor WebAssembly frontend
- `SkillSnap_Shared/` - Shared models and interfaces
- `SkillSnap_API_Test/` - API unit tests
- `SkillSnap_Client_Test/` - Client unit tests

## Prerequisites

- .NET 9.0 SDK
- Visual Studio 2025 or VS Code with C# extension

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/RJPalmer/SkillSnap.git
   cd SkillSnap
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Update database:
   ```bash
   cd SkillSnap_API
   dotnet ef database update
   ```

4. Start the API (from SkillSnap_API directory):
   ```bash
   dotnet run
   ```

5. In a new terminal, start the Blazor client (from SkillSnap_Client directory):
   ```bash
   dotnet run
   ```

6. Open your browser to:
   - API: https://localhost:5000
   - Client: https://localhost:5001

## Development Setup

### Visual Studio Code
1. Install the C# Dev Kit extension
2. Open the solution folder
3. Use the built-in debugging support

### Visual Studio 2025
1. Open `SkillSnapSolution.sln`
2. Set startup projects:
   - SkillSnap_API
   - SkillSnap_Client

## Configuration

### API Settings (SkillSnap_API/appsettings.json)
- Database connection string
- CORS settings
- Logging levels

### Client Settings (SkillSnap_Client/wwwroot/appsettings.json)
- API base address
- Azure AD settings (if using authentication)

## Running Tests

```bash
dotnet test
```

## Contributing

1. Create a feature branch
2. Make your changes
3. Run tests
4. Submit a pull request