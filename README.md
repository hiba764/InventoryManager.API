# 📦 InventoryManager.API - Inventory Management System

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?logo=dotnet" alt="ASP.NET Core">
  <img src="https://img.shields.io/badge/Database-SQL%20Server-CC2927?logo=microsoftsqlserver" alt="SQL Server">
  <img src="https://img.shields.io/badge/ORM-Entity%20Framework%20Core-512BD4" alt="Entity Framework Core">
  <img src="https://img.shields.io/badge/Testing-xUnit-512BD4" alt="xUnit">
</p>

> **"Good inventory management is not just about knowing what you have — it's about knowing what changed and why."**

---

## 🌟 The Idea

**InventoryManager.API** is a RESTful backend system designed to manage products and track inventory changes through **Stock Movements**.

The system handles incoming and outgoing stock operations and automatically updates product quantities based on each movement.

The project focuses on practical **C# and .NET backend development**, with an emphasis on clean structure, business logic, authentication, database management, and automated testing.

---

## ✨ Features

* 📦 **Product Management** — Manage inventory products and their quantities.
* 🔄 **Stock Movements** — Record incoming and outgoing inventory operations.
* 📊 **Automatic Quantity Updates** — Update product quantities according to stock movements.
* 🔐 **JWT Authentication** — Secure protected API endpoints.
* 🗄️ **Database Management** — SQL Server with Entity Framework Core.
* 🛡️ **Error Handling** — Centralized exception handling through middleware.
* 🧪 **Automated Testing** — Test important business logic using xUnit.
* 📖 **Swagger / OpenAPI** — Explore and test the API endpoints.

---

## 🔄 The Core Logic

Stock quantity is managed through recorded movements:

```text
Incoming Stock  →  Quantity increases
Outgoing Stock  →  Quantity decreases
```

Example:

```text
Initial Quantity: 100
Incoming:         +50
Outgoing:         -30
-----------------------
Final Quantity:   120
```

This makes inventory changes clear and traceable.

---

## 🛠️ Tech Stack

| Layer                 | Technology             |
| :-------------------- | :--------------------- |
| **Language**          | C#                     |
| **Backend**           | ASP.NET Core 8 Web API |
| **Database**          | SQL Server             |
| **ORM**               | Entity Framework Core  |
| **Authentication**    | JWT                    |
| **Testing**           | xUnit                  |
| **API Documentation** | Swagger / OpenAPI      |

---

## 🗂️ Project Structure

```text
InventoryManager.API/
│
├── Controllers/        # API endpoints
├── Data/               # Database context
├── DTOs/               # Data Transfer Objects
├── Interfaces/         # Service contracts
├── Middleware/         # Global error handling
├── Migrations/         # EF Core migrations
├── Models/             # Application entities
├── Services/           # Business logic
├── Properties/
├── appsettings.json
└── Program.cs
│
InventoryManager.API.Tests/
│
├── Infrastructure/     # Test infrastructure
└── Tests/              # Automated tests
```

---

## 🧪 Testing

The project includes automated tests using **xUnit**, with a focus on the application's business logic.

For example, stock movement tests verify that:

* Incoming movements increase product quantity.
* Outgoing movements decrease product quantity.
* Inventory operations behave according to the defined business rules.

Run the tests with:

```bash
dotnet test
```

---

## 🚀 Getting Started

### Requirements

* .NET 8 SDK
* SQL Server
* Visual Studio 2022

Configure your SQL Server connection string in `appsettings.json`, then run the project.

Database migrations can be applied using Entity Framework Core.

---

## 🎯 Project Goals

This project was built to demonstrate practical backend development with **C# and .NET**, including:

* RESTful API development
* Layered project structure
* Business logic implementation
* SQL Server and Entity Framework Core
* JWT authentication
* Automated testing
* Clean and maintainable backend code

---

## 👩‍💻 Author

**Hiba Ajam**

**Software Engineer | Backend Development with C# & .NET**

