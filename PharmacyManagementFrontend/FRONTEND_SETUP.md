# Pharmacy Management System - Frontend Setup

## Overview
Modern Angular frontend with authentication system for the Pharmacy Management System.

## Features Created
- ✅ **Landing Page** - Welcome page with feature overview
- ✅ **Login System** - Separate login for Doctors and Admin
- ✅ **Registration** - Doctor registration with validation
- ✅ **Dashboard** - Role-based dashboard with navigation cards
- ✅ **Authentication Service** - JWT token management
- ✅ **Route Guards** - Protected routes
- ✅ **HTTP Interceptor** - Automatic token injection
- ✅ **Modern UI** - Responsive design with gradient backgrounds

## Quick Start

### 1. Install Dependencies
```bash
cd PharmacyManagementFrontend
npm install
```

### 2. Start Development Server
```bash
npm start
```
The app will run on `http://localhost:4200`

### 3. Backend Connection
Make sure your .NET Core backend is running on `https://localhost:7297`

## User Flow

### For Doctors:
1. Visit landing page → Click "Register as Doctor"
2. Fill registration form → Account created
3. Login with credentials → Redirected to dashboard
4. Access: My Orders, Special Orders, Available Drugs

### For Admin:
1. Visit landing page → Click "Sign In"
2. Switch to "Admin Login" tab
3. Login with admin credentials (sakethram@gmail.com)
4. Access: Full system management

## Components Structure
```
src/app/
├── components/
│   ├── auth/
│   │   ├── login.component.ts
│   │   └── register.component.ts
│   ├── dashboard/
│   │   └── dashboard.component.ts
│   └── landing/
│       └── landing.component.ts
├── services/
│   └── auth.service.ts
├── models/
│   └── auth.models.ts
├── guards/
│   └── auth.guard.ts
└── interceptors/
    └── auth.interceptor.ts
```

## API Endpoints Used
- `POST /api/authentication/login_user` - Doctor login
- `POST /api/authentication/login_admin` - Admin login  
- `POST /api/authentication/register_user` - Doctor registration
- `GET /api/authentication/user` - Get user info

## Styling
- Modern gradient backgrounds
- Card-based layouts
- Responsive design
- Loading states and animations
- Form validation with error messages

## Next Steps
1. Run the frontend: `npm start`
2. Test login/register functionality
3. Verify backend integration
4. Add more feature components as needed

The authentication system is now complete and ready for use!