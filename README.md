# Clinical Dentist System

A **RESTful Web API** built with **ASP.NET Core (C#)** designed to manage the complete clinical workflow of a dental clinic — from patient registration and appointments to AI-assisted clinical documentation and supply inventory.

---

## Project Idea

The **Clinical Dentist System** was built to solve a real operational problem in dental clinics: fragmented and manual clinical processes. Instead of managing patients, appointments, records, and supplies across separate tools or paper, this system centralizes everything into one secure, role-based API.

The core idea is to give dental staff a **single backend system** that:
- Tracks the full patient journey — from booking to completed treatment
- Stores rich **Electronic Health Records (EHR)** with structured dental data (teeth, procedures, X-rays, medications)
- Introduces **AI assistance** (via a local LLaMA model through Ollama) to help doctors write clinical notes, look up dental terminology, suggest treatments, and extract structured data from free-text
- Enforces **role-based access** so that doctors and nurses each have appropriate permissions
- Manages **clinic supplies and stock** with full transaction history

This project is intended to serve as a backend API, designed to integrate with a frontend (e.g., a Next.js application).

---

## Features

### 👤 Authentication & Authorization
- Separate **Doctor** and **Nurse** registration and login flows
- Registration protected by a **clinic-issued registration key** (configured server-side)
- **JWT-based authentication** with role claims (`Doctor` / `Nurse`)
- **Role-based access control** — some endpoints are restricted to doctors only (`DoctorOnly` policy)

### 🧑‍⚕️ Patient Management
- Create, read, update, and delete patient profiles
- Patient fields: first name, middle name, last name, gender, date of birth, phone

### 📅 Appointment Scheduling
- Create and manage appointments linked to a patient, doctor, and nurse
- Auto-generated reference numbers per appointment
- Appointment types, dates, and times tracked

### 🗂️ Electronic Health Records (EHR)
- Full EHR per appointment/patient including:
  - **Medical info**: allergies, medical alerts
  - **Dental info**: diagnosis, X-ray findings, periodontal status, clinical notes, recommendations, treatment history
  - **Structured collections**: medications, procedures (with procedure codes) and  per-tooth records
- **Audit trail / change log**: every field-level change is logged with who changed it, when, and from which appointment

### 🤖 AI-Assisted Clinical Documentation (LLaMA via Ollama)
- **Auto-complete** for partial clinical note text
- **Dental terminology suggestions** from partial terms
- **Generate full clinical notes** from bullet points
- **Treatment suggestions** based on diagnosis and patient history
- **Structured data extraction** from free-form clinical text
- **Full EHR parsing** — doctor writes a large block of text, and the AI extracts all EHR fields automatically

### 🏥 Staff Management
- Manage **Doctor** profiles (Doctor-only access)
- Manage **Nurse** profiles (create/update restricted to doctors)

### 📦 Supply & Inventory Management
- Manage dental supplies with categories, units, and quantities
- Filter supplies by category
- Track **stock transactions** (who used what and when)
- View transaction history per doctor or per supply item

### 🔒 Security
- Passwords stored as **hashed values** (never plain text)
- All core endpoints require a valid JWT token
- Sensitive management endpoints enforce the `DoctorOnly` authorization policy

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core (C#) |
| Database | SQL Server |
| ORM | Entity Framework Core (with migrations) |
| Authentication | JWT Bearer tokens |
| AI | LLaMA 3.1 8B via [Ollama](https://ollama.com) |
| API Docs | Swagger / OpenAPI |
| CORS | Configured for Next.js frontend (`localhost:3000`) |

---

## Project Structure

```
ClinicalDentistSystem/
├── Controllers/         # API endpoints (Patient, Appointment, EHR, Doctor, Nurse, Supply, StockTransaction, AI, Auth)
├── Models/              # Entity models (EHR, Patient, Appointment, Doctor, Nurse, Supply, etc.)
├── DTOs/                # Request and response data transfer objects
├── Services/            # Business logic & mapping services, JWT, password hashing, AI (LlamaService)
├── Data/                # AppDbContext (Entity Framework Core)
├── Migrations/          # EF Core database migrations
├── Tests/               # Automated tests
├── Program.cs           # App configuration and dependency injection
└── appsettings.json     # Configuration (JWT, DB connection, AI settings)
```

---

## API Endpoints Overview

| Area | Endpoints |
|---|---|
| Doctor Auth | `POST /api/DoctorAuth/Register`, `POST /api/DoctorAuth/Login` |
| Nurse Auth | `POST /api/NurseAuth/Register`, `POST /api/NurseAuth/Login` |
| Patients | `GET/POST/PUT/DELETE /Patient` |
| Appointments | `GET/POST/PUT/DELETE /Appointment` |
| EHR | `GET/POST/PUT /EHR`, `GET /EHR/{id}/history` |
| Doctors | `GET/POST/PUT/DELETE /Doctor` *(Doctor-only)* |
| Nurses | `GET/POST/PUT/DELETE /Nurse` |
| Supplies | `GET/POST/PUT/DELETE /Supply`, `GET /Supply/Category/{category}` *(Doctor-only)* |
| Stock Transactions | `GET/POST /StockTransaction` *(Doctor-only)* |
| AI | `POST /api/AI/autocomplete`, `/terminology`, `/generate-notes`, `/suggest-treatments`, `/extract-clinical-data`, `/parse-ehr` |

---

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (check `clinical.APIs.csproj` for target version)
- **SQL Server** (local or remote)
- [Ollama](https://ollama.com) with the `llama3.1:8b` model pulled (for AI features):
  ```bash
  ollama pull llama3.1:8b
  ```
