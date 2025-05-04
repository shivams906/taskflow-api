# 🚀 TaskFlow – Project & Task Management System

**TaskFlow** is a modern task and project management system built with **ASP.NET Core** and **Vue.js**. It enables teams to efficiently manage projects, assign tasks, track time, and collaborate seamlessly. Designed with simplicity, role-based access, and productivity in mind.

---

## ✨ Features

### 🔧 Backend – ASP.NET Core 8

- 🔐 **Authentication & Authorization**  
  JWT-based authentication with role-based access control (`Creator`, `Admin`, `Assigned User`).

- 📁 **Project Management**  
  Create, update, delete projects. Assign and manage project admins.

- 📋 **Task Management**  
  Create, update, delete tasks. Assign users, update task status.

- ⏱️ **Time Logging**  
  Start/stop timer-based time tracking. View per-user time logs.

- ⚙️ **Architecture & Clean Code**  
  - DTOs and AutoMapper for clean separation
  - Audit logging with interceptors  
  - Modular code organization

- 🛡️ **Security**  
  APIs secured with JWT Bearer Tokens. Role checks enforced.

### 💻 Frontend – Vue 3 + Tailwind

- 🔐 **Auth Pages**  
  Login & Register with secure token management.

- 🧭 **Project Dashboard**  
  View, create, and manage projects for authorized users.

- ✅ **Task Panel**  
  Filter, view, and update tasks.

- 🕒 **Time Logs**  
  Track hours worked. Users can log time manually or via timer.

- 💡 **UX Enhancements**  
  - Toast notifications  
  - Modal dialogs  
  - Fully responsive design  
  - Clean, minimal UI

---

## 🛠️ Tech Stack

| Layer     | Stack                          |
|-----------|--------------------------------|
| Backend   | ASP.NET Core 8, EF Core, JWT   |
| Frontend  | Vue 3, TailwindCSS, Vite       |
| Database  | SQL Server |
