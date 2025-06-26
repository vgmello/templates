# Billing Service UI

A modern SvelteKit web application for managing the Billing Service cashiers, built with Svelte 5, Tailwind CSS, and shadcn-svelte components.

## Features

- **Dashboard**: Overview of billing service features
- **Cashier Management**: Create, view, and manage cashiers
- **Elegant UI**: Built with shadcn-svelte components and Tailwind CSS
- **Responsive Design**: Works on desktop and mobile devices
- **Real-time Data**: Connects to the Billing API for live data

## Tech Stack

- **Framework**: SvelteKit with Svelte 5
- **Styling**: Tailwind CSS
- **Components**: shadcn-svelte
- **Icons**: Lucide Svelte
- **API**: REST API communication with Billing Service

## Prerequisites

- Node.js 18+ 
- npm
- Billing Service API running on http://localhost:8101 (HTTP) and localhost:8102 (gRPC)

## Getting Started

1. **Install dependencies**
   ```bash
   npm install
   ```

2. **Start development server**
   ```bash
   npm run dev
   ```

3. **Open in browser**
   ```
   http://localhost:3001
   ```

## Project Structure

```
src/
├── lib/
│   ├── components/ui/     # shadcn-svelte components
│   ├── api.js            # API service layer
│   └── utils.js          # Utility functions
├── routes/
│   ├── +layout.svelte    # App layout
│   ├── +page.svelte      # Dashboard
│   └── cashiers/         # Cashier management pages
└── app.css              # Global styles
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run check` - Type checking

## API Integration

The app connects to the Billing Service API via gRPC at `localhost:8102`. If the API is unavailable, the app will fall back to mock data for development purposes.

### Endpoints Used

- `GET /cashiers` - List all cashiers
- `GET /cashiers/{id}` - Get cashier details
- `POST /cashiers` - Create new cashier
- `PUT /cashiers/{id}` - Update cashier
- `DELETE /cashiers/{id}` - Delete cashier

## Features Overview

### Dashboard
- Service overview with quick navigation
- Feature cards for different service areas

### Cashier Management
- **List View**: Grid of cashier cards with key information
- **Create Form**: Multi-step form for adding new cashiers
- **Detail View**: Complete cashier information and payment configurations
- **Edit/Delete**: Manage existing cashiers

### UI Components
- Responsive design with mobile-first approach
- Loading states and error handling
- Form validation and user feedback
- Elegant card layouts and typography