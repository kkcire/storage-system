# Storage System

A storage management application developed as a practical study project while learning C# and .NET.

## About

This is an educational project built to apply concepts learned while studying C# and .NET, including relational database relationships, object-relational mapping with Entity Framework Core, and layer separation.

The application manages products and brands through a console interface built with Spectre.Console. The project focuses on combining these concepts in a single application while developing a better understanding of application structure, data persistence, and business rules.

## Features

* Product registration and management
* Brand registration and management
* Products linked to brands
* CRUD operations for products and brands
* Persistence with Entity Framework Core and SQLite
* Business-rule validation

## Technologies

* C#
* .NET 8
* Entity Framework Core
* SQLite
* xUnit
* Spectre.Console

## Structure

```text
StorageSystem/
├── src/
│   ├── StorageSystem.Domain/
│   │   ├── Entities/
│   │   ├── Services/
│   │   └── Data/
│   │
│   └── StorageSystem.Console/
│       └── UI/
│
└── test/
    └── StorageSystem.Tests/
        └── Services/
```

## What I Learned

This project was a great opportunity to apply theoretical concepts in a single, practical application. Main takeaways include:

* Data persistence, object-relational mapping, and managing database relationships with EF Core and SQLite.
* Applying OOP principles, dependency injection, and decoupling business rules from the UI layer.
* Writing tests with xUnit and using an in-memory database to test logic.

## Getting Started

### Requirements

* .NET 8 SDK or later.

### Clone the repository

```bash
git clone https://github.com/kkcire/storage-system.git
cd storage-system
```

### Setup

Before running the application for the first time, apply the database migrations:

```bash
dotnet ef database update --project src/StorageSystem.Domain
```

### Run

```bash
dotnet run --project src/StorageSystem.Console
```

### Run the tests

```bash
dotnet test
```
