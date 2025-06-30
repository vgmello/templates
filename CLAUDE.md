# CLAUDE.md

## API

### Docker Compose (Billing Service)

#### Backend start command
From the root folder (templates)
```bash
docker compose -f Billing/compose.yml --profile api up -d
```

## UI Integration

### **Frontend Architecture**
- **SvelteKit UI** located at `Billing/web/billing-ui/`
- **Responsive Design** with Tailwind CSS and Lucide icons

### **UI Development Commands**
```bash
TODO: Fill this
```

## Development Notes

- **Package Management**: Centralized via Directory.Packages.props
- **Code Quality**: SonarAnalyzer enabled, warnings as info (not errors)
- **Environment**: .NET 9 with nullable reference types and implicit usings

If you need to download any tools save them on the _temp folder

# Important-instruction-reminders
ALWAYS do a git pull before starting any work to ensure I'm using the latest version.
Keep the project documentation (README.md (s)) updated
Update memory frequently (CLAUDE.md) on how to better navigate this project
ALWAY Commit and push before output the value