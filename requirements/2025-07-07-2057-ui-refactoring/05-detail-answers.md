# Expert Requirements Answers

**Date:** 2025-07-07
**Phase:** Detail

## Q6: Should the domain models (like Invoice and Cashier classes) live in a shared library that both client and server can use?
**Answer:** No
**Implications:** Domain models will be server-side only in +page.server.ts files, keeping the architecture simple

## Q7: Should we implement a repository pattern to abstract the API calls, or call the API client directly from +page.server.ts?
**Answer:** Call directly, simplicity = elegancy
**Implications:** No repository layer; +page.server.ts will use the API client directly for all backend calls

## Q8: Should the telemetry configuration be externalized to environment variables for different deployment environments?
**Answer:** Yes
**Implications:** Move hardcoded telemetry endpoints and configuration to environment variables

## Q9: Should we implement optimistic updates in the UI for better perceived performance on mutations?
**Answer:** No
**Implications:** Stick with server-side rendering and form actions; no complex client-side state synchronization

## Q10: Should domain validation rules be duplicated on the client-side for immediate feedback, or rely solely on server validation?
**Answer:** Rely on the server side for now
**Implications:** All validation happens server-side; form errors are returned from +page.server.ts actions