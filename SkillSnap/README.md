# SkillSnap - Developer Portfolio Application

A modern full-stack developer portfolio application built with .NET 9 and Blazor WebAssembly.

## 🚀 Features

- **Modern Tech Stack**: Built with .NET 9, Blazor WebAssembly, and Entity Framework Core
- **Responsive Design**: Mobile-first responsive UI components
- **Database Management**: SQLite database with Entity Framework Core
- **API Documentation**: Swagger/OpenAPI integration
- **Health Monitoring**: Built-in health checks and monitoring
- **Security**: Production-ready security headers and CORS configuration
- **Reusable Components**: Modular Blazor components for maintainability

## 🏗️ Architecture

### Backend (SkillSnap.Api)

- **ASP.NET Core 9.0** Web API
- **Entity Framework Core** with SQLite
- **Swagger/OpenAPI** documentation
- **Health Checks** for monitoring
- **Security Headers** middleware

### Frontend (SkillSnap.Client)

- **Blazor WebAssembly** with .NET 9
- **Reusable Razor Components**
- **Responsive CSS** with modern styling
- **Component-based architecture**

## 📦 Project Structure

```text
SkillSnap/
├── SkillSnap.Api/              # Backend Web API
│   ├── Controllers/            # API Controllers
│   ├── Data/                   # Entity Framework DbContext
│   ├── Models/                 # Data models
│   └── Program.cs              # Application configuration
├── SkillSnap.Client/           # Frontend Blazor WebAssembly
│   ├── Components/             # Reusable Blazor components
│   ├── Pages/                  # Main application pages
│   └── wwwroot/               # Static web assets
└── SkillSnap.sln              # Solution file
```

## 🛠️ Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- Git

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/rbom482/SkillSnap-Capstone-Project.git
   cd SkillSnap-Capstone-Project
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Build the solution**

   ```bash
   dotnet build
   ```

4. **Run the application**

   ```bash
   cd SkillSnap.Api
   dotnet run
   ```

5. **Access the application**
   - Web App: `http://localhost:5000`
   - API Documentation: `http://localhost:5000/api/docs`
   - Health Checks: `http://localhost:5000/health`

### Database Setup

1. **Create and seed the database**

   ```bash
   # The application will automatically create the database
   # To seed with sample data, make a POST request to:
   curl -X POST http://localhost:5000/api/seeddata
   ```

## 🎯 Core Components

### Backend Features

- **PortfolioUser Management**: User profiles and information
- **Project Showcase**: Display of developer projects
- **Skills Tracking**: Technical skills and proficiency levels
- **Database Seeding**: Sample data generation
- **API Documentation**: Comprehensive Swagger documentation

### Frontend Components

- **Profile Page**: Developer profile display with statistics
- **Projects Page**: Portfolio project showcase
- **Skills Page**: Technical skills visualization
- **ProfileCard Component**: Reusable profile display component

## 🔧 Configuration

### Database

The application uses SQLite by default with the following connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=skillsnap.db"
  }
}
```

### API Endpoints

- `GET /api/portfoliousers` - Get all users
- `GET /api/projects` - Get all projects  
- `GET /api/skills` - Get all skills
- `POST /api/seeddata` - Seed sample data
- `GET /health` - Health check endpoint

## 🚀 Deployment

The application is production-ready with:

- Security headers configured
- CORS policies implemented
- Health checks enabled
- Structured logging
- Error handling middleware

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is part of a capstone project and is available under the MIT License.

## 👤 Author

**Rebecca** - [GitHub Profile](https://github.com/rbom482)

---

Built with ❤️ using .NET 9 and Blazor WebAssembly
