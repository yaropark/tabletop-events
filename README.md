# Tabletop Events

A calendar scheduler for tabletop game events. Create events and let players register for them with a QR code. Built with ASP.NET Core 10, Angular 20 and SQLite.

## Run it locally

Requires [Docker Desktop](https://www.docker.com/products/docker-desktop/), started. From the repository root:

```
docker compose up --build
```

Open http://localhost:8080. Five demo events are seeded on first start; `docker compose down -v` resets the data.

**Windows:** if the build fails with `additional privileges requested`, the terminal path has a lowercase drive letter. Run `cd /d C:\path\to\tabletop-events` and try again.

**Scanning with a phone (optional):** clicking the link under the QR code needs no setup. To scan with a phone, open the app through your computer's network address (e.g. `http://192.168.0.10:8080`; on Windows the Wi-Fi must be set to Private), or run `docker compose --profile public up --build` for a temporary public link (Cloudflare quick tunnel; there's no login, so anyone with the link can use it).

**Without Docker** (.NET 10 SDK and Node.js 20.19+), in two terminals:

```
# Terminal 1, from the repository root (API on :5080)
dotnet run --project api/TabletopEvents.Api

# Terminal 2, from the web folder (http://localhost:4200)
npm install
npm start
```

Use `npm run start:lan` instead of `npm start` to reach the dev server from a phone. Run the tests with `dotnet test`.

## Design

### Capacity

The organizer sets the capacity per event, defaulting to the game template's value. The API accepts values between the template's minimum players and maximum capacity, with 30 as a hard limit that the database also enforces (`CHECK (Capacity BETWEEN 1 AND 30)`). Capacity is copied onto the `Events` row when the event is created, so later template changes don't affect existing events.

The capacity check and the insert are one SQL statement, so two requests can't both take the last seat:

```sql
INSERT INTO Registrations (...)
SELECT ...
WHERE (SELECT COUNT(*) FROM Registrations WHERE EventId = @EventId)
    < (SELECT Capacity FROM Events WHERE Id = @EventId AND StartUtc > @Now)
```

SQLite takes the write lock before the statement runs, so the losing request inserts nothing and gets `409 Sorry, this event is full.` A test sends 40 registrations at once for 10 seats and expects exactly 10 to succeed; a naive count-then-insert version let 31 people in. Duplicates are blocked by unique indexes (the email if given, otherwise the name, ignoring case and extra spaces) and get `409 You're already registered for this event.` Registration closes when the event starts.

### Game templates

Each game is one entry in `api/TabletopEvents.Api/game-templates.json`: formats, default duration, default and maximum capacity, minimum players and calendar color. On startup the app validates the file and upserts it into the `GameTemplates` table. The event form and the API read everything from the template; no code checks which game it is. Adding a game, card game or not, is one JSON entry and a restart. The commit "Add The Lord of the Rings: The Card Game as a 4th template" did exactly that and touches only the JSON file.

### What I cut and what I would build next

**Cut per instruction:** login and roles, an admin area for templates, emails and unregistering, and editing, cancelling or waitlists for events.
**My own cuts:** a single store configured in `appsettings.json`, minimum players is display-only, and tests cover the API only.

**Next:** organizer login and then a template admin area; a confirmation email with the .ics attached; cancellation for registered users and from the admin side; reminders the day before; and multiple stores. For multiple stores I'd add a `Stores` table with a `StoreId` on events and per-store login. Times are already stored in UTC and capacity is per event, so the core logic wouldn't change; with many stores I'd also move to SQL Server or PostgreSQL and add schema migrations.

## AI usage

I used Claude Code (Claude Opus 5.5) in VS Code to plan the design, write most of the code, tests and Docker setup, debug my local environment, and draft parts of this README from my answers. I made the decisions and tested the phone flows myself. Three things I had to fix: its first version used a flat structure, which I had restructured into `api/` and `web/` with controllers, services and repositories; its first run instructions failed when I followed them (they needed two terminals and a LAN option for phones); and its first plan let games cap capacity below 30, which would have broken the requirement until I asked it to re-check the plan against the brief.
