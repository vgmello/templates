# Expert Requirements Questions

These detailed questions clarify specific implementation decisions based on the codebase analysis.

## Q6: Should the domain models (like Invoice and Cashier classes) live in a shared library that both client and server can use?
**Default if unknown:** No (server-side models in +page.server.ts are sufficient since we're using server-side rendering)

## Q7: Should we implement a repository pattern to abstract the API calls, or call the API client directly from +page.server.ts?
**Default if unknown:** Yes (repository pattern provides better testability and separation of concerns)

## Q8: Should the telemetry configuration be externalized to environment variables for different deployment environments?
**Default if unknown:** Yes (follows twelve-factor app principles and matches the backend approach)

## Q9: Should we implement optimistic updates in the UI for better perceived performance on mutations?
**Default if unknown:** No (server-side rendering with form actions provides sufficient performance)

## Q10: Should domain validation rules be duplicated on the client-side for immediate feedback, or rely solely on server validation?
**Default if unknown:** Yes (duplicate critical validations for better UX while keeping server as source of truth)