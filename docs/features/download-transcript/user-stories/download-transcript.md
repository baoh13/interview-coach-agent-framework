# Download Interview Transcript

## User Story

**As an** interview candidate,
**I want to** download my interview transcript as a Markdown file,
**so that** I can review my answers and coach feedback offline after the session ends.

---

## Description

The chat UI currently has no way to preserve a session after it ends. This story delivers a
"Download Transcript" button in `ChatHeader.razor` that serializes the current `ChatMessage`
list into a `.md` file with speaker labels and triggers a browser file download via JS interop.
No backend, MCP, or AppHost changes are required.

**Business value:** Candidates can revisit their performance and coach feedback without
relying on an active session, improving the long-term utility of the tool.

---

## Scenario

### Scenario 1: Candidate downloads transcript after an interview

**Given** a candidate has completed an interview session with at least one message in the chat  
**When** they click the "Download Transcript" button in the chat header  
**Then** a `.md` file is downloaded by the browser  
**And** the file contains each message labelled with the speaker (`**User:**` or `**Coach:**`)  
**And** the filename is formatted as `interview-YYYY-MM-DD.md` using the current date

### Scenario 2: Download button is disabled with no messages

**Given** the chat is empty (no messages exchanged yet)  
**When** the chat header is rendered  
**Then** the "Download Transcript" button is visible but disabled  
**And** no download is triggered

### Scenario 3: New Chat does not break after download

**Given** a candidate has downloaded a transcript  
**When** they click "New Chat"  
**Then** the chat resets as normal  
**And** the transcript download history is unaffected

---

## Acceptance Criteria

- [ ] A "Download" button appears in the chat header (disabled when chat is empty)
- [ ] Clicking it produces a `.md` file with speaker-labelled turns (`**User:**` / `**Coach:**`)
- [ ] The filename includes the session date, e.g. `interview-2026-03-21.md`
- [ ] The button does not break existing chat or New Chat functionality
- [ ] No changes to Agent, MCP, or AppHost projects

---

## Blocked by

None — can start immediately.

---

## Implementation Notes

- Changes confined to `src/InterviewCoach.WebUI`
- Add a JS interop helper (e.g. `downloadFile(filename, content)`) to trigger the browser download
- Serialize `ChatMessage` list in `ChatHeader.razor` or passed via a callback from the parent `Chat.razor`
- Follow existing JS interop patterns already used in `ChatInput.js`
