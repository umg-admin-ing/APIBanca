## Deploying on Render

### Required environment variables

Set these in the Render Web Service:

- `ASPNETCORE_ENVIRONMENT=Production`
- `PORT=<provided by Render>`
- `ConnectionStrings__DefaultConnection=<optional override if you do not want to use appsettings>`

### Health check

Set the health check path to:

- `/healthz`

### Notes

- The app listens on Render's `PORT` environment variable.
- Swagger is available in production at `/swagger`.
- CORS is open for this homework API: any origin, header, and method is allowed.
- Because CORS is fully open, no Render CORS environment variables are required.
- The repository currently keeps a hardcoded connection string in appsettings for your requested workflow.
- EF Core migrations run on startup.