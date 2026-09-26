# usthing

A personal timetable API. Each person signs in and keeps their own events.

## Run

```bash
docker compose up --build
```

The API listens on port 8080. `GET /health` returns `{"status":"ok"}`.

## Data

SQLite stores two tables, `users` and `timetable_events`. Each event belongs to one user. Event times are UTC. A recurring event is one row: `recurrence_rule` holds the rule body (`FREQ=WEEKLY;BYDAY=MO`) and `time_zone_id` says which clock that rule uses. The default zone is `Asia/Hong_Kong`. An imported event also stores the calendar uid, so a later import updates that row. Locally the file is `timetable.db` in the working directory. In Docker the file is `/data/timetable.db` on the `timetable-data` volume.

## Events

Development seeds `alice` / `alice` and `bob` / `bob`.

```bash
curl -s -X POST http://localhost:8080/v1/auth/token \
  -H 'Content-Type: application/json' \
  -d '{"username":"alice","password":"alice"}'
```

Create a lecture every Monday at 10:00 in Hong Kong (02:00 UTC):

```bash
curl -s -X POST http://localhost:8080/v1/events \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"title":"Lecture","start":"2026-09-07T02:00:00Z","end":"2026-09-07T03:00:00Z","recurrenceRule":"FREQ=WEEKLY;BYDAY=MO","timeZoneId":"Asia/Hong_Kong"}'
```

`GET /v1/events` returns stored rows, including the rule, so a client can edit the series. `GET /v1/events?from=&to=` expands that series into occurrences, and only when both bounds are set. The range cannot be longer than 366 days.

```bash
curl -s "http://localhost:8080/v1/events?from=2026-09-14T00:00:00Z&to=2026-09-28T00:00:00Z" \
  -H "Authorization: Bearer $TOKEN"
```

That window contains two Mondays. Each item uses the series id and that Monday's start and end.

Bob cannot read Alice's event. `GET /v1/events/{id}` with his token returns 404, and his list is empty.

```bash
curl -s http://localhost:8080/v1/events/1 \
  -H "Authorization: Bearer $BOB"
```

`GET /v1/events/export.ics?from=&to=` writes one `VEVENT` per series, including its rule. `from` and `to` are required, with the same 366-day limit.

```bash
curl -s "http://localhost:8080/v1/events/export.ics?from=2026-09-14T00:00:00Z&to=2026-09-28T00:00:00Z" \
  -H "Authorization: Bearer $TOKEN" \
  -o timetable.ics
```

`POST /v1/events/import.ics` reads that file. A uid this user already has is updated, so posting it again does not add a row.

```bash
curl -s -X POST http://localhost:8080/v1/events/import.ics \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: text/calendar' \
  --data-binary @timetable.ics
```

## MCP

`POST /mcp` uses the same bearer token. Tools: `list_events`, `create_event`, `update_event`, `delete_event`.

```bash
curl -s -X POST http://localhost:8080/mcp \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"list_events","arguments":{}}}'
```
