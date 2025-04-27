# Renework

Renework is a web-based platform designed for professional retraining and career change.  
It allows users to register, log in, and access information related to educational programs, training courses, and career resources.

This project is built with:
- **ASP.NET Core Razor Pages**
- **MongoDB** (for database)
- **BCrypt.Net** (for password hashing)
- **Swagger** (for API documentation)
- **JWT Authentication** (future implementation planned)
- **Docker** (for deployment, optional)

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Endpoints](#endpoints)
- [Database Schema](#database-schema)
- [Security](#security)
- [Development Guidelines](#development-guidelines)
- [Future Work](#future-work)
- [Contributors](#contributors)

---

## Features

- 🔐 User Registration and Authentication (email, username)
- 📚 Display educational programs (planned)
- 📈 Track learning progress (future feature)
- 🧹 Modular, clean code architecture
- ☁️ MongoDB for cloud-hosted database
- 🔒 Secure password handling with BCrypt

---

## Architecture

- **Frontend**: Razor Pages (dynamic HTML generated on the server)
- **Backend**: ASP.NET Core (.NET 8.0)
- **Database**: MongoDB Atlas (or local MongoDB server)
- **Authentication**: 
  - Registration/login is implemented.
  - Passwords are securely hashed using `BCrypt`.
  - JWT-based auth planned for API endpoints.

---

## Technology Stack

| Layer         | Technology                        |
| ------------- | --------------------------------- |
| Frontend      | Razor Pages (ASP.NET Core)         |
| Backend       | ASP.NET Core Web App (.NET 8.0)    |
| Database      | MongoDB (NoSQL)                   |
| Password Hash | BCrypt.Net-Next                    |
| Cloud Hosting | DigitalOcean / Render (optional)  |
| API Docs      | Swagger (Swashbuckle.AspNetCore)   |
| Deployment    | Docker (optional)                 |

---

## Getting Started

### Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/en-us/download)
- [MongoDB Server](https://www.mongodb.com/try/download/community) (local) or [MongoDB Atlas](https://www.mongodb.com/atlas)
- (Optional) [Docker](https://www.docker.com/products/docker-desktop/)

### Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/chiIlin/renework.git
   cd renework
   ```

2. **Configure MongoDB:**
   - Create a `.env` or configure `appsettings.json` with your MongoDB connection string.
   - Example:
     ```json
     {
       "MongoDB": {
         "ConnectionString": "your-mongodb-connection-string",
         "DatabaseName": "ReneworkDB"
       }
     }
     ```

3. **Run the project:**
   ```bash
   dotnet restore
   dotnet run
   ```
   The app will be available at `https://localhost:5001/`.

4. **Access Swagger (if configured):**
   ```
   https://localhost:5001/swagger
   ```

---

## Project Structure

```
renework/
├── Pages/
│   ├── Register.cshtml
│   ├── Register.cshtml.cs
│   ├── Login.cshtml (planned)
│   └── Index.cshtml (landing page)
├── Dto/
│   ├── RegisterDto.cs
│   └── LoginDto.cs
├── MongoDB/
│   ├── Collections/
│   │   └── User.cs
│   ├── Interfaces/
│   │   └── IUserRepository.cs
│   └── Repositories/
│       └── UserRepository.cs
├── Program.cs
├── appsettings.json
├── Dockerfile (optional)
└── README.md
```

---

## Endpoints

| Route             | Method | Description                    | Authentication |
| ----------------- | ------ | ------------------------------ | -------------- |
| `/Register`       | GET    | Render registration page       | ❌             |
| `/Register`       | POST   | Handle new user registration    | ❌             |
| `/Login` (future) | GET    | Render login page               | ❌             |
| `/Login` (future) | POST   | Handle user login and token gen | ❌             |

---

## Database Schema

**Collection**: `Users`

```json
{
  "_id": "ObjectId",
  "Username": "string",
  "Email": "string",
  "HashedPassword": "string",
  "Role": "User"
}
```

| Field          | Type    | Notes            |
|----------------|---------|------------------|
| `_id`          | ObjectId | MongoDB internal |
| `Username`     | string  | Unique           |
| `Email`        | string  | Unique           |
| `HashedPassword` | string  | Stored hashed    |
| `Role`         | string  | "User" or "Admin" |

---

## Security

- **Passwords** are never stored in plain text — they are hashed using `BCrypt`.
- **Duplicate validation**: Registration ensures unique `Email` and `Username`.
- **Planned**: Add JWT Authentication for protected API access.
- **Input Validation**: Basic validation is already set up in DTOs.

---

## Development Guidelines

- Use **async/await** for all DB operations (MongoDB driver supports async).
- Follow **Repository Pattern** to abstract database operations.
- Add **Data Annotations** for field validation on DTOs.
- Write modular, clean, and documented code.

---

## Future Work

- [ ] Implement Login page and logic (`/Login`)
- [ ] Add JWT Token generation and storage
- [ ] Create a user profile page
- [ ] Implement course search and enrollment functionality
- [ ] Dashboard to track course completion
- [ ] Admin panel to manage courses and users
- [ ] Dockerize the project for easier deployment
- [ ] CI/CD integration (GitHub Actions)
- [ ] Caching frequently accessed data (maybe Redis)

---

## Contributors

- **[@chiIlin](https://github.com/chiIlin)** — Original Author

---

# License

This project is licensed under the **MIT License** — feel free to use, copy, modify, and distribute.

---

# Final Notes

If you have any ideas, feedback, or want to contribute — feel free to open an issue or pull request 🚀
