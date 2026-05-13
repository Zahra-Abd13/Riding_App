# 🚗 RidingAppDB — Riding-App Management System

A full-featured **desktop application** for managing a riding platform (similar to Uber/Careem), built with **C# WinForms** and **SQL Server**, connected via **ADO.NET**.

---

## 🖥️ Screenshots

> Dashboard → navigate to any module with one click.

| Dashboard
<img width="687" height="591" alt="Screenshot 2026-05-13 035912" src="https://github.com/user-attachments/assets/6c3486ed-9bdb-493c-b318-1e68510732de" />
| Users Management 
<img width="1915" height="760" alt="image" src="https://github.com/user-attachments/assets/1753cee6-ca94-480f-8560-e8a5dcc05b3f" />
| Drivers Management 
<img width="1918" height="831" alt="image" src="https://github.com/user-attachments/assets/d998e0f1-9b74-4bdc-9988-0a4f1dcb68aa" />|
|Rides Management
<img width="1918" height="795" alt="image" src="https://github.com/user-attachments/assets/29f7c40a-b731-4c70-92e5-4c7af25aa4a8" />
|Reviews Management
<img width="1918" height="647" alt="image" src="https://github.com/user-attachments/assets/6847763b-67a0-48b0-92c9-962acf8d7a53" />
|Payments Management
<img width="1918" height="635" alt="image" src="https://github.com/user-attachments/assets/cf82c691-1ca4-4fcf-9a7e-749c9d7c555b" />


---

## ✨ Features

- **Dashboard** — central navigation hub with buttons for each module
- **Users** — full CRUD with linked phone numbers (Users + UsersPhones)
- **Drivers** — full CRUD with linked vehicle info (Drivers + Vehicles + Owns)
- **Rides** — book, view, update, and cancel rides
- **Payments** — record and manage payments with method selection (Cash / Card / Wallet)
- **Reviews** — add and manage ride ratings and comments (1–5 stars)
- **Search** — search any record by ID within each form
- **Validation** — all inputs validated before any database operation
- **Dark UI** — modern dark theme across all forms

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# (.NET 10) |
| UI Framework | Windows Forms (WinForms) |
| Database | Microsoft SQL Server |
| Data Access | ADO.NET (Connected Mode) |
| SQL Library | Microsoft.Data.SqlClient v6.1.2 |

---

## 🗄️ Database Schema

9 relational tables with proper primary and foreign keys:

```
Users ──< UsersPhones
Users ──< Rides >── Drivers
Drivers ──< Owns >── Vehicles
Rides ──< Payments
Rides ──< Reviews
Rides >── Locations (Pickup & Dropoff)
```

**Tables:** `Users`, `UsersPhones`, `Drivers`, `Vehicles`, `Owns`, `Locations`, `Rides`, `Payments`, `Reviews`

---

## ⚙️ ADO.NET Implementation

All database access is centralized in `Database.cs` using **ADO.NET Connected Mode**:

```csharp
// Centralized query method using SqlConnection + SqlCommand + SqlDataReader
internal static async Task<DataTable> QueryAsync(string sql, params SqlParameter[] parameters)
{
    using SqlConnection connection = new(ConnectionString);
    using SqlCommand command = new(sql, connection);
    command.Parameters.AddRange(parameters);

    DataTable table = new();
    await connection.OpenAsync();
    using SqlDataReader reader = await command.ExecuteReaderAsync();
    table.Load(reader);
    return table;
}
```

- `SqlConnection` — manages the connection to SQL Server
- `SqlCommand` — sends SELECT / INSERT / UPDATE / DELETE commands
- `SqlDataReader` — reads query results row by row
- `SqlParameter` — fully parameterized queries (no SQL injection)
- `ExecuteNonQuery` — for INSERT, UPDATE, DELETE operations
- `DataTable` + `DataGridView` — displays results in the UI

---

## 🚀 Getting Started

### Prerequisites

- Windows OS
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Microsoft SQL Server (any edition)
- Visual Studio 2022 or later

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/Zahra-Abd13/Riding_App.git
   cd RideSharingDB
   ```

2. **Create the database**

   Open SQL Server Management Studio (SSMS), connect to your server, and run:
   ```
   project/WinFormsApp1/WinFormsApp1/SQLQuery5.sql
   ```

3. **Update the connection string**

   Open `project/WinFormsApp1/WinFormsApp1/Database.cs` and update line 5:
   ```csharp
   private const string DefaultConnectionString =
       "Data Source=YOUR_SERVER_NAME;Initial Catalog=RideSharingDB;Integrated Security=True;TrustServerCertificate=True;";
   ```
   Replace `YOUR_SERVER_NAME` with your SQL Server instance name.

4. **Build and run**
   ```bash
   cd project/WinFormsApp1
   dotnet run --project WinFormsApp1
   ```
   Or open `WinFormsApp1.slnx` in Visual Studio and press **F5**.

---

## 📁 Project Structure

```
WinFormsApp1/
├── Database.cs          # ADO.NET layer — all SQL logic
├── FormUtilities.cs     # Shared helpers — validation, grid config
├── Program.cs           # Entry point
├── Form1.cs             # Dashboard (main navigation)
├── UsersForm.cs         # Users + Phones CRUD
├── DriversForm.cs       # Drivers + Vehicles CRUD
├── RidesForm.cs         # Rides CRUD
├── PaymentsForm.cs      # Payments CRUD
├── ReviewsForm.cs       # Reviews CRUD
└── SQLQuery5.sql        # Database creation + seed data script
```
---

## 📄 License

This project is for educational purposes.
