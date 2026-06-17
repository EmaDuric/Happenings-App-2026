# Happenings App 2026

Event management application built with ASP.NET Core, Flutter, and WinForms.

## Architecture

- **Happenings.WebAPI** - REST API (ASP.NET Core 8)
- **Happenings.Subscriber** - RabbitMQ worker service
- **Happenings.WinUI** - Desktop admin panel (WinForms)
- **UI** - Mobile/Web Flutter application

---

## Prerequisites

- Docker & Docker Compose
- .NET 8 SDK
- Flutter SDK
- Android Studio (AVD emulator) — za mobilnu verziju
- SQL Server — za lokalni razvoj

---

## Running with Docker (preporučeno)

1. Kloniraj repozitorij
2. Raspakiraj `.env-tajne.zip` (šifra: **fit**) i postavi `.env` u root folder
3. Pokreni:

```bash
docker-compose up --build
```

API dostupan na: `http://localhost:5000`  
Swagger: `http://localhost:5000/swagger`

---

## Running Locally

### API

# Vrijednosti se postavljaju iz .env (raspakovan iz .env-tajne.zip, sifra: fit).
# NE hardkodirati tajne ovdje ni u appsettings.json.
```powershell
cd Happenings.WebAPI
$env:Jwt__Key = "<JWT_KEY iz .env>"
$env:Jwt__Issuer = "<JWT_ISSUER iz .env>"
$env:Jwt__Audience = "<JWT_AUDIENCE iz .env>"
$env:PayPal__ClientId = "<PAYPAL_CLIENT_ID iz .env>"
$env:PayPal__Secret = "<PAYPAL_SECRET iz .env>"
$env:Stripe__PublishableKey = "<STRIPE_PUBLISHABLE_KEY iz .env>"
$env:Stripe__SecretKey = "<STRIPE_SECRET_KEY iz .env>"
dotnet run --urls "http://localhost:5000"
```

> Jednostavnije: `docker-compose up --build` automatski učita `.env` i ne traži ručno postavljanje varijabli.

### RabbitMQ (Docker)

```powershell
docker start rabbitmq
# ili
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.12-management
```

### Subscriber

```powershell
cd Happenings.Subscriber
dotnet run
```

### Desktop (WinUI)

```powershell
cd Happenings.WinUI
dotnet run
```

ili otvori `Happenings.sln` u Visual Studiju i pokreni `Happenings.WinUI`.

### Mobile (Flutter) — Android Emulator

Pokreni Android emulator u Android Studio, pa:

```powershell
cd UI
flutter run -d emulator-5554 --dart-define=API_BASE_URL=http://10.0.2.2:5000/api
```

### Mobile (Flutter) — Web

```powershell
cd UI
flutter run -d chrome --web-port=64020 --dart-define=API_BASE_URL=http://localhost:5000/api
```

---

## Login Credentials

| Kontekst | Email | Lozinka |
|----------|-------|---------|
| Desktop (Admin) | admin@mail.com | admin2026 |
| Mobile User | mobile@mail.com | mobile2026 |
| Mobile Organiser | organiser@mail.com | organiser2026 |

---

## Test Payment Credentials

### PayPal Sandbox (Buyer account)
- **Email**: sb-qzd1n51409087@personal.example.com
- **Password**: SDT6i2$R

### Stripe test kartica
- **Card number**: 4242 4242 4242 4242
- **Expiry**: 12/28
- **CVV**: 123

---

## API Documentation

Swagger: `http://localhost:5000/swagger`

---

## Environment

Konfiguracijski podaci (JWT key, PayPal, Stripe, RabbitMQ) nalaze se u `.env-tajne.zip`.  
Šifra za raspakiranje: **fit**