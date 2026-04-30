# Happenings App 2026

Event management application built with ASP.NET Core, Flutter, and WinForms.

## Architecture

- **Happenings.WebAPI** - REST API (ASP.NET Core 8)
- **Happenings.Subscriber** - RabbitMQ worker service
- **Happenings.WinUI** - Desktop admin panel (WinForms)
- **UI** - Mobile Flutter application

## Prerequisites

- Docker & Docker Compose
- .NET 8 SDK
- Flutter SDK
- SQL Server (for local development)

## Running with Docker

1. Clone the repository
2. Create `.env` file from `.env-tajne.zip` (password: fit)
3. Run:

```bash
docker-compose up --build
```

API will be available at: `http://localhost:5000`

## Running Locally

### API
```bash
cd Happenings.WebAPI
dotnet run
```

### Subscriber
```bash
cd Happenings.Subscriber
dotnet run
```

### Desktop (WinUI)
Open `Happenings.sln` in Visual Studio and run `Happenings.WinUI`.

### Mobile (Flutter)
```bash
cd UI
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5000
```

## Login Credentials

| Context | Username | Password |
|---------|----------|----------|
| Desktop (Admin) | admin@mail.com | admin2026 |
| Mobile-User | mobile@mail.com | mobile2026 |
| Mobile-Organiser| organiser@mail.com | organiser2026 |

## API Documentation

Swagger available at: `http://localhost:5000/swagger`

password za .env-tajne je "fit".
