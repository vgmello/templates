# CLAUDE.md

## API

### Docker Compose (Billing Service)
```bash
# Run from repo root
docker compose -f Billing/compose.yml --profile api up
```

## UI Integration

### **Frontend Architecture**
- **SvelteKit UI** located at `Billing/web/billing-ui/`
- **gRPC Communication** with Billing API using `@grpc/grpc-js`
- **Responsive Design** with Tailwind CSS and Lucide icons

### **UI Development Commands**
```bash
# Install UI dependencies
cd Billing/web/billing-ui && npm install

# Run UI in development mode
npm run dev

# Build UI for production
npm run build

# Run UI tests (requires backend to be running - see below)
npm run test:ui

# Run UI tests with mocked API (standalone, no backend required)
npm run test:mock
```

**Important:** 
- All Playwright tests should use `npm run test:ui` as the command prefix
- Backend must be running on port 8102 (gRPC) for UI tests to pass
- Use `npm run test:mock` for tests without backend dependency

## Development Notes

- **Package Management**: Centralized via Directory.Packages.props
- **Code Quality**: SonarAnalyzer enabled, warnings as info (not errors)
- **Environment**: .NET 9 with nullable reference types and implicit usings

If you need to download any tools save them on the _temp folder

# important-instruction-reminders
ALWAYS do a git pull before starting any work to ensure I'm using the latest version.
Keep the project documentation (README.md (s)) updated
Update memory frequently (CLAUDE.md) on how to better navigate this project
ALWAY Commit and push before output the value