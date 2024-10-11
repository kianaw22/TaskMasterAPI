
# TaskMaster API - README

## Project Overview

The **TaskMaster API** is a RESTful API for managing tasks, integrating with GitHub issues, and handling user authentication. It supports role-based authorization (Admin and User) and uses JWT for secure authentication.

## Features

- **User Management**: Register, authenticate, and manage user profiles.
- **Task Management**: Create, view, update, and delete tasks; assign tasks to other users.
- **GitHub Integration**: Link tasks to GitHub issues and fetch issue details.
- **Security**: JWT-based authentication and role-based access control.

## Setup Instructions

1. **Clone the Repository**:
    ```bash
    git clone <your-repo-url>
    cd TaskMasterAPI
    ```

2. **Install Dependencies**:
    ```bash
    dotnet restore
    ```

3. **Database Setup**:
    The project uses SQLite. Ensure the connection string is correctly set in `appsettings.json`.

4. **Run Migrations**:
    ```bash
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```

5. **Run the Application**:
    ```bash
    dotnet run
    ```

6. **Access the API**:
    The API will be available at `http://localhost:5041/`.

## Using Swagger UI

1. **Access Swagger**: Open `http://localhost:5041/` in your browser.
2. **Authenticate**:
   - Use the `/api/users/authenticate` endpoint to generate a JWT token.
   - Click the **"Authorize"** button in Swagger UI and enter `Bearer <your-token>` to authenticate.
3. **Test API Endpoints**: Swagger UI allows you to test all the API endpoints.

## Key API Endpoints

### User Endpoints:
- `POST /api/users/register`: Register a new user.
- `POST /api/users/authenticate`: Login and get a JWT token.
- `GET /api/users/{id}`: View user profile (Admin or self).

### Task Endpoints:
- `POST /api/tasks`: Create a new task.
- `GET /api/tasks`: View tasks.
- `PUT /api/tasks/{id}`: Update a task.
- `DELETE /api/tasks/{id}`: Delete a task.

### GitHub Integration:
- `POST /api/tasks/link-github-issue`: Link a task to a GitHub issue.
- `GET /api/github/issues/{issueId}`: Fetch issue details from GitHub.

## JWT Configuration

JWT settings (issuer, audience, and key) are in `appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "YourIssuer",
    "Audience": "YourAudience",
    "Key": "YourSecretKey"
  }
}
```

## Conclusion

TaskMaster API provides a simple and secure way to manage tasks and integrate with GitHub issues. It's secured with JWT and designed for easy customization and extension.
