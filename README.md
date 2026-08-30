# 🎬 Cinema Reservation API

Backend API for a cinema ticket reservation system built with **ASP.NET Core**, **Entity Framework Core**, and **SQL Server**.

The API provides authentication, cinema hall management, movie management, screening management, seat availability, reservations, temporary seat holds, and payment processing.

The project is designed as the backend for the Angular frontend application:

**Frontend:** [Cinema Reservation Web](https://github.com/YOUR_USERNAME/cinema-reservation-web)

---

## 🚀 Features

The backend currently provides the foundation for:

* 🔐 User registration and authentication
* 🎟️ JWT-based authentication and authorization
* 👤 Customer and administrator roles
* 🏢 Cinema hall management
* 🎬 Movie management
* 📅 Movie screening/schedule management
* 💺 Seat availability and reservation
* ⏱️ Temporary seat holds
* 💳 Mock payment processing
* 🔔 Payment webhook handling
* ✅ Reservation confirmation after successful payment
* ❌ Reservation cancellation and expiration
* 📊 Health checks
* 📖 Swagger/OpenAPI documentation

---

## 🛠️ Technology Stack

* **ASP.NET Core Web API**
* **C#**
* **Entity Framework Core**
* **SQL Server**
* **JWT Authentication**
* **Swagger / OpenAPI**
* **RESTful APIs**
* **Dependency Injection**
* **EF Core Code First Migrations**

---

# ⚙️ Getting Started

## Prerequisites

Make sure the following are installed:

* [.NET SDK](https://dotnet.microsoft.com/download)
* SQL Server
* Git
* Optional: SQL Server Management Studio (SSMS)
* Optional: Visual Studio / Visual Studio Code

Verify the .NET installation:

```bash
dotnet --version
```

---

# 📥 Installation

Clone the repository:

```bash
git clone https://github.com/AS95Egypt/cinema-tickets-back.git
```

Navigate to the project:

```bash
cd cinema-tickets-back
```

Restore dependencies:

```bash
dotnet restore
```

---

# 🗄️ Database Configuration

The application uses **SQL Server** with Entity Framework Core.

Configure the connection string in:

```text
appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=dbname;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

# 🗃️ Entity Framework Core Migrations

The project uses **EF Core Code First migrations**.

## Apply Migrations

Create/update the database:

```bash
dotnet ef database update
```

This applies all pending migrations.

For a production deployment, migrations can also be generated as a script and reviewed before execution.

---

# ▶️ Running the API

Run the application using:

```bash
dotnet run
```

For development:

```bash
dotnet watch run
```

The actual HTTP/HTTPS URLs are displayed in the terminal when the application starts.

---

# 📖 Swagger / OpenAPI

When running in the development environment, Swagger provides interactive API documentation.

Open the Swagger URL shown by the application, typically:

```text
https://localhost:<port>/swagger
```

Swagger can be used to:

* Explore available endpoints
* View request/response models
* Test APIs
* Test authentication-protected endpoints


---

# 🔗 Related Project

This API is consumed by the Angular frontend:

**Cinema Reservation Web**

https://github.com/YOUR_USERNAME/cinema-reservation-web

The frontend repository contains the customer and administrator user interfaces.

---

# 🧪 Testing

Run automated tests with:

```bash
dotnet test
```

---

## 📌 Project Status

🚧 **Under Development**

The project is being developed incrementally, with customer reservation and administrator management features being implemented as separate user stories.

---

## 📄 License

This project is intended for learning and development purposes.
