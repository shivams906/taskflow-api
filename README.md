# TaskFlow - Project Management System

TaskFlow is a task and project management system built with **ASP.NET Core** for the backend and **Vue.js** for the frontend. This application allows users to manage projects, tasks, and time logs in a simple and intuitive way. It features role-based access control, time tracking, and an easy-to-use UI focused on usability and simplicity.

## Features

### Backend (ASP.NET Core)

- **Authentication & Authorization**: JWT token-based authentication with role-based authorization (Creator, Admin, Assigned User).
- **Project Management**: Create, Edit, Delete projects and assign admins.
- **Task Management**: Create, Edit, Delete tasks, assign/unassign tasks to users, and update task status.
- **Time Logging**: Users can log time for tasks with a start/stop timer system.
- **API Improvements**: DTO usage for clean data exposure, Automapper for mappings, and exception handling.
- **Security**: API secured with JWT bearer tokens.

### Frontend (Vue.js)

- **User Authentication Pages**: Login, Register.
- **Project Management Pages**: Admins can view, create, edit, and delete projects.
- **Task Management Pages**: Admins can manage tasks, assigned users can view and update tasks.
- **Time Logs Pages**: Users can view and add time logs.
- **UX Features**: Toast notifications, modal dialogs, and responsive design.

## Tech Stack

- **Backend**:

  - ASP.NET Core 8
  - Entity Framework Core
  - SQLite Database
  - JWT Authentication

- **Frontend**:
  - Vue 3
  - TailwindCSS
  - Vite (for fast development build)
