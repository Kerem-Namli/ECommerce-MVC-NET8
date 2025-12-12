# 🛒 E-Commerce MVC .NET 8 Platform

A robust, enterprise-grade E-Commerce application built with **ASP.NET Core 8 MVC**, following the **N-Layer Architecture** principles. This solution provides a scalable foundation for modern online retail platforms with a dedicated Admin Panel and a responsive Customer facing storefront.

## 🚀 Features

### 🛍️ User & Customer Features
- **Product Browsing**: Dynamic product catalog with category filtering.
- **Shopping Cart**: Real-time cart management using Session storage.
- **Secure Checkout**: Integrated order processing flows.
- **Responsive Design**: Optimized for Desktop and Mobile viewing.

### 🔧 Admin Panel & Management
- **Dashboard**: Overview of key metrics (Users, Products, Orders).
- **Role-Based Access Control (RBAC)**: Fine-grained permissions for Admin, Manager, Sales Specialist, and User roles.
- **Product Management**: Full CRUD operations for Products with image support.
- **Stock Tracking**: Automated passive status updates for out-of-stock items.
- **Category Management**: Organize products into hierarchical categories.

## 🛠️ Technology Stack

- **Framework**: .NET 8 (ASP.NET Core MVC)
- **Database**: SQL Server with Entity Framework Core (Code-First)
- **Authentication**: ASP.NET Core Identity
- **Frontend**: HTML5, CSS3, Bootstrap 5, JavaScript
- **Architecture**: N-Layer (Entity, Data, Business, Web, Admin)

## 📂 Project Structure

- **ETicaret.Entity**: Core domain entities and models.
- **ETicaret.Data**: Data access layer, DbContext, and repositories.
- **ETicaret.Business**: Business logic, services, and validation.
- **ETicaret.Web**: Customer-facing storefront application.
- **ETicaret.Admin**: Back-office management dashboard.
- **ETicaret.API**: RESTful API endpoints (if applicable).

## 📦 Getting Started

1. **Clone the repository**
2. **Update Connection String**: Configure your `DefaultConnection` in `appsettings.json`.
3. **Database Migration**:
   ```bash
   dotnet ef database update --project ETicaret.Data --startup-project ETicaret.Web
   ```
4. **Run the Application**:
   Start `ETicaret.Web` for the storefront and `ETicaret.Admin` for the dashboard.
