# AllAccessApp 🌐

A modern cloud storage platform built with:
- 🔹 **ASP.NET Core 8** (Backend API)
- 🔹 **Angular 18** (Frontend)
- 🔹 **Cloudflare R2** (File Storage)
- 🔹 **JWT Authentication**
- 🔹 **Responsive Design**

🔐 Secure, fast, and user-friendly — just like Google Drive.

## ⚠️ Setup Required

This app uses external services. To run it:

### 1. Cloudflare R2
- Sign up at [https://www.cloudflare.com/products/r2/](https://www.cloudflare.com/products/r2/)
- Create bucket & API keys
- Update `appsettings.example.json` → `appsettings.json`

### 2. Gmail App Password (for Contact & Forgot Password)
- Enable 2FA + generate 16-digit app password

See `appsettings.example.json` for config format.

## 🛠️ Run Locally

### Backend
```bash
cd AllAccessApp.API
dotnet restore
dotnet ef database update
dotnet run
