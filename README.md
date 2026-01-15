# DAIM System

A modern inventory and sales management system built with **ASP.NET Core MVC**, **Entity Framework Core**, and **QuestPDF**.  
Designed for small and medium businesses to manage stock, customers, orders, and generate professional PDF reports.

---

##  Features

### Inventory
- Add, edit, and deactivate inventory items
- Track quantity and reorder levels
- Automatic stock updates on order creation and deletion

### Customers & Suppliers
- Customer management with contact details
- Supplier management
- Clean CRUD workflows

### Orders
- Create orders with multiple inventory items
- Automatic price calculation per line item
- Stock deduction and restoration
- Order status tracking (Pending / Completed / Cancelled)

### Reports (PDF)
- Order Summary Report
- Inventory Stock Report
- Sales Register (Completed Orders)
- Date-range filtering
- Professional PDF layout using QuestPDF


### UI & UX
- Modern Bootstrap 5 layout
- Responsive tables and forms
- Icons and clean visual hierarchy
- Validation and error handling

---

## Tech Stack

| Layer | Technology |
|-----|-----------|
| Backend | ASP.NET Core MVC (.NET 9) |
| ORM | Entity Framework Core |
| Database | SQL Server |
| PDF Reports | QuestPDF |
| Frontend | Razor Views + Bootstrap 5 |


---

##  Project Structure

InventoryManagementSystem/
├── Controllers/
│ ├── InventoryController.cs
│ ├── CustomerController.cs
│ ├── SupplierController.cs
│ ├── OrderController.cs
│ └── ReportController.cs
├── Models/
│ ├── Inventory.cs
│ ├── Customer.cs
│ ├── Supplier.cs
│ ├── Order.cs
│ └── OrderItem.cs
├── Views/
│ ├── Inventory/
│ ├── Customers/
│ ├── Suppliers/
│ ├── Orders/
│ └── Reports/
└── Data/
└── InventoryDbContext.cs



---

## Reports (QuestPDF)

Reports are generated dynamically using **LINQ projections** and rendered with **QuestPDF**.

Supported reports:
- Order Summary
- Inventory Stock
- Sales Register

> QuestPDF is configured using the Community license:
```csharp
QuestPDF.Settings.License = LicenseType.Community;(Free Version)


Setup Instructions

Clone the repository

git clone https://github.com/DouglasHutchful1/InventoryManagement.git


Restore dependencies

dotnet restore


Update database connection string in appsettings.json

Apply migrations

dotnet ef database update


Run the application

dotnet run

Use Cases

Small retail businesses

Wholesale distributors

Inventory tracking demos

Portfolio showcase




License

This project is for educational and portfolio purposes.
