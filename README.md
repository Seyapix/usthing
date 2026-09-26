# usthing

A personal timetable API. This service will let someone keep their own events. Events are not implemented yet.

## Run

```bash
docker compose up --build
```

The API listens on port 8080. `GET /health` returns `{"status":"ok"}`.

## Data

SQLite stores two tables, `users` and `timetable_events`. Each event belongs to one user. Event times are UTC. Locally the file is `timetable.db` in the working directory. In Docker the file is `/data/timetable.db` on the `timetable-data` volume.
