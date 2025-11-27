# Blazor WebAssembly + API JWT Authentication

## 1. Project Summary

This project is an implementation of a **Blazor WebAssembly (Client)** application that authenticates against an **ASP.NET Core Web API (Server)** using **JSON Web Tokens (JWT)**.

The main goal is to establish a **robust authentication and authorization flow** based on tokens and roles, ensuring a clear separation of responsibilities between the frontend (Blazor) and the backend (API).

---

## 2. Project Structure and Technologies

| Component        | Description                                                     | Key Technologies                                              |
| ---------------- | --------------------------------------------------------------- | ------------------------------------------------------------- |
| **Client**       | Handles the UI, session state, and authentication               | Blazor WASM, Blazored.SessionStorage, CustomAuthStateProvider |
| **Server (API)** | Handles business logic, authentication, and token/role issuance | .NET 8/9, JWT Bearer, CORS                                    |

---

## 3. Security and Authentication Implementation

### A. Login Flow

1. `Login.razor` sends credentials to `/api/auth/login`.
2. The `AuthController` validates the credentials (currently mock data).
3. If valid, the API generates a **JWT** containing the claim `role = "Admin"`.
4. The client receives the token and stores it in **Session Storage** (`ISessionStorageService`).
5. The `CustomAuthStateProvider` reads and decodes the token, extracts claims, and notifies Blazor using `NotifyAuthenticationStateChanged`.

### B. Request Interception

A `DelegatingHandler` called **TokenHandler** intercepts all `HttpClient` requests and automatically adds:

```
Authorization: Bearer <token>
```

whenever a token is present in session storage.

### C. Authorization and Roles

* `App.razor` uses `<CascadingAuthenticationState>` to propagate authentication state.
* API endpoints use `[Authorize(Roles="Admin")]`.
* Blazor controls UI access with:

```razor
<AuthorizeView Roles="Admin">
```

---

## 4. Identified Vulnerabilities and Applied Mitigations

| Vulnerability                  | Mitigation                                                                 | Reason                                                     |
| ------------------------------ | -------------------------------------------------------------------------- | ---------------------------------------------------------- |
| **SQL Injection**              | Use of ORM (EF Core) and strongly typed DTOs                               | Prevents malicious input from being executed as SQL        |
| **XSS (Cross-Site Scripting)** | Data Annotations (`[EmailAddress]`, `[MaxLength]`) and JSON-only responses | Strict validation and prevents execution of malicious HTML |
| **Insecure Token Persistence** | Migration from LocalStorage → SessionStorage                               | Reduces risk of token theft via XSS                        |
| **Missing CORS Configuration** | Explicit CORS policy configuration in the API                              | Enables secure communication between Blazor and the API    |
| **Token Exposure**             | Implementation of `TokenHandler`                                           | Sends the token only when needed and always securely       |

---

## 5. Copilot Assistance During Debugging

| Task / Issue                    | Provided Assistance                                                                                                                             | Result                                        |
| ------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------- |
| **InvalidOperationException**   | Identified the error *Authorization requires a cascading parameter...* and suggested wrapping `App.razor` with `<CascadingAuthenticationState>` | Issue resolved immediately                    |
| **CORS Block Issue**            | Analyzed preflight errors and suggested full CORS configuration                                                                                 | Restored communication between Blazor and API |
| **JWT Implementation**          | Guidance for implementing `CustomAuthStateProvider` and Base64 URL-safe decoding                                                                | Correct JWT decoding on the client side       |
| **Migration to SessionStorage** | Detailed instructions on which Client files to modify                                                                                           | Smooth and secure migration                   |

---

## Conclusion

This project demonstrates a solid architecture for handling **authentication and authorization** between a Blazor WebAssembly client and an ASP.NET Core API using JWT. The implementation prioritizes security, best practices, and a clear separation of responsibilities.

You can use this project as a foundation for future applications involving real databases, more advanced authentication flows, or modular architecture.

