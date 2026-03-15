---
name: webapp-testing
description: Test local Interview Coach web flows with Playwright-style browser automation, focusing on rendered UI behavior and diagnostics.
---

## Instructions

Use this skill for validating `src/InterviewCoach.WebUI` behavior end-to-end.

1. Start the app stack first:

```powershell
aspire run --file .\apphost.cs
```

2. Test against the running WebUI endpoint (from Aspire dashboard), not static source.
3. On dynamic pages, always wait for rendered state before interacting.
4. Follow reconnaissance-then-action:
   - inspect rendered DOM / accessibility snapshot
   - identify robust selectors
   - execute interactions
   - capture screenshot + console/network evidence when failures occur
5. Report failures with repro steps, expected vs actual behavior, and likely root cause location.

Prefer flows that match this repo’s user journeys: start interview, continue multi-turn chat, and validate handoff-visible behavior.
