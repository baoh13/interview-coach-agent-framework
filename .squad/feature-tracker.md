# Feature Tracker

Tracking significant feature decisions and routing/architecture changes.

---

## Feature Decisions

### 1. WebUI Landing Page as Default (2026-03-15)
**Decision:** Make `/landing` the default homepage when WebUI starts.

**Changes:**
- LandingPage.razor: `/landing` → `/` (now default)
- Chat.razor: `/` → `/interview` (moved to new route)

**Rationale:** Users see the landing page (company/services info) first, then navigate to `/interview` for chat.

**Affected Files:**
- `src/InterviewCoach.WebUI/Components/Pages/Chat/Chat.razor`
- `src/InterviewCoach.WebUI/Components/Pages/LandingPage.razor`

**Status:** Implemented

---
