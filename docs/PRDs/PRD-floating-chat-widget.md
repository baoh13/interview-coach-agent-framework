# PRD: Floating Chat Widget on Landing Page

**Date**: March 21, 2026  
**Feature**: Add mini-chat floating widget to landing page  
**Priority**: Medium  
**Status**: Approved

---

## Executive Summary

Replace the static "Technologies" section on the landing page with a dynamic floating chat widget. This widget will launch an embedded mini chat experience anchored to the bottom-right corner, allowing visitors to interact with the interview coach without navigating away from the landing page or opening a full-screen chat interface.

The full-page chat at `/interview` remains available as a fallback and for power users. This feature increases engagement by providing a low-friction entry point to the coaching experience.

---

## Problem Statement

- **Current state**: Landing page is read-only marketing material with static Technologies section.
- **Limitation**: Users must navigate to `/interview` or click "Get in Touch" to interact with the coach.
- **Gap**: No lightweight, in-context interaction path from landing page.
- **Opportunity**: Floating widget allows immediate ad-hoc questions without page navigation.

---

## Goals

1. **Increase engagement**: Provide immediate access to chat without friction.
2. **Improve UX**: Small, non-intrusive widget that auto-opens on first visit, can be minimized.
3. **Maintain full-page option**: Keep `/interview` route for users who want full-screen chat.
4. **Clean architecture**: Refactor chat logic into reusable `ChatShell` component used by both mini-chat and full-page routes.
5. **Mobile-first responsive**: Adapt to screen sizes—desktop fixed size (400px), mobile bottom sheet (90vh).

---

## Scope

### In Scope

- Remove Technologies section from landing page (UI only; data fetch removal deferred to next phase).
- Create `ChatShell` reusable component encapsulating core chat state and logic.
- Create `FloatingChatWidget` component with auto-open behavior, minimize/close buttons, responsive sizing.
- Desktop: fixed 400px wide widget at bottom-right.
- Mobile: bottom sheet expanding to 90% viewport height.
- File upload disabled in mini-chat to keep UI compact.
- Header visible in mini-chat (not collapsed).
- Chat history persists during browser session; session-scoped continuity only.
- Conversation history remains if user navigates away and returns in same session.
- Floating button styling inherits landing page design tokens (color, typography, shadows).
- Visual consistency with existing landing UI.
- Auto-open on first visit; user can minimize to single button.

### Out of Scope

- Multi-session persistence (future iteration).
- Keyboard shortcuts (Ctrl+K) and focus traps (future iteration).
- Cross-session storage of conversation history.
- Data model cleanup (Technologies JSON file fetch removal deferred).
- Unit or UI tests (ship UI first, tests in follow-up phase).
- File upload in mini-chat.
- Header collapse/menu variants.
- Alternative entry points (CTA buttons opening widget instead of scroll/contact).

---

## User Stories

### US-1: Remove Technologies Section

**As a** landing page visitor,  
**I want to** see no Technologies section,  
**So that** the page is more focused on core services and coaching entry point.

**Acceptance Criteria**:
- [ ] Technologies section markup removed from landing page.
- [ ] Footer navigation link to Technologies removed.
- [ ] Page renders without broken layout or missing CSS.
- [ ] Services and Contact sections remain intact.

---

### US-2: Create Reusable ChatShell Component

**As a** developer,  
**I want to** extract chat core logic into a reusable `ChatShell` component,  
**So that** mini-chat, full-page, and future chat surfaces can share state/behavior without duplication.

**Acceptance Criteria**:
- [ ] `ChatShell.razor` component created with public API for messages, current response, session ID.
- [ ] `AddUserMessageAsync()` method handles message addition and streaming response.
- [ ] `ResetConversationAsync()` method clears messages and starts new session.
- [ ] Component accepts `ConversationId` parameter for session override.
- [ ] Component accepts `IncludeFileUpload` parameter (unused in mini-chat, enabled in full-page).
- [ ] `OnMessageAdded` event callback fires after each message operation.
- [ ] Chat logic from existing `Chat.razor` migrated to `ChatShell` without behavioral change.

---

### US-3: Create FloatingChatWidget Component

**As a** landing page visitor,  
**I want to** see a floating chat button that opens a mini conversation window,  
**So that** I can quickly ask the coach questions without leaving the landing page.

**Acceptance Criteria**:
- [ ] Floating button appears bottom-right, 3.5rem diameter, circular, with chat icon (💬).
- [ ] Button styling uses landing page accent gradient and color tokens.
- [ ] On first page visit, mini-chat window auto-opens (after 500ms delay).
- [ ] Window displays: header with "Interview Coach" title, minimize (−) and close (✕) buttons, ChatMessageList, and ChatInput.
- [ ] Minimize button hides window, shows only floating button again.
- [ ] Close button hides window, shows only floating button again.
- [ ] Opening button repositions to bottom-right, displays chat container.
- [ ] File upload disabled in ChatInput when used in FloatingChatWidget.
- [ ] Desktop: fixed 400px wide, 600px max-height, positioned bottom-right with 2rem margin.
- [ ] Mobile (max-width 768px): bottom sheet, 100% width, up to 90vh height, smooth slide-up animation.
- [ ] Chat history persists while widget stays open; closes only when user explicitly closes.
- [ ] `ChatShell` instantiated internally with `IncludeFileUpload="false"`.

---

### US-4: Responsive Behavior and Animations

**As a** mobile user,  
**I want to** see the chat adapt to my screen,  
**So that** the experience is usable on any device.

**Acceptance Criteria**:
- [ ] Desktop: fixed window 400px wide, appears bottom-right, non-blocking.
- [ ] Mobile: bottom sheet layout, full screen width, scrollable content area.
- [ ] Slide-up animation on open (desktop: 300ms, mobile: 300ms).
- [ ] Minimize/close instantly hides widget, shows floating button.
- [ ] All buttons, inputs, and message text remain readable and interactive.
- [ ] No overflow or layout shift on any breakpoint.

---

### US-5: Maintain Full-Page Chat Route

**As a** power user or existing workflow,  
**I want** `/interview` full-page chat to remain unchanged,  
**So that** my existing bookmarks and workflows still work.

**Acceptance Criteria**:
- [ ] `/interview` route still accessible and functional.
- [ ] Full-page chat layout, header, and file upload remain available.
- [ ] No behavioral change to full-page chat from this feature.
- [ ] Full-page `Chat.razor` can later refactor to use `ChatShell` (future phase).

---

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Landing-only for now** | Validates feature value before global rollout. Reduces risk of broken experiences on other pages. |
| **Keep /interview route** | Maintains user choice; fallback for full-feature needs (file upload). |
| **Reusable ChatShell** | Prevents logic duplication; enables future surfaces (chat in sidebar, etc.). |
| **Auto-open on first visit** | Immediately engages new visitors; they can minimize if not interested. |
| **Minimize ≠ Destroy** | Conversations persist while session is alive; low cost to re-open. |
| **No file upload in mini-chat** | Keeps UI compact; users needing uploads navigate to full-page chat. |
| **Header visible** | Maintains context ("Interview Coach" title); full feature set transparent. |
| **Visual consistency** | Inherit landing colors/tokens; feels native, not tacked-on. |
| **Session-scoped history** | Simple for MVP; no backend session storage complexity. Full persistence in next iteration. |
| **Desktop fixed, mobile bottom sheet** | Follows modern mobile patterns (chat bots, help widgets); predictable, non-disruptive. |

---

## Technical Approach

### Components

1. **ChatShell.razor** (logic container, no UI)
   - Encapsulates chat state: messages, current response, session ID.
   - Public methods: `AddUserMessageAsync()`, `ResetConversationAsync()`.
   - Public properties: `Messages`, `CurrentResponseMessage`.
   - Parameters: `ConversationId`, `IncludeFileUpload`, `OnMessageAdded` callback.

2. **FloatingChatWidget.razor** (UI container for mini-chat)
   - Hosts `ChatShell` internally.
   - Renders floating button (closed state) or chat window (open state).
   - Manages open/minimize/close state.
   - Embeds `ChatMessageList` and `ChatInput` in chat window.
   - Styles: desktop fixed 400px, mobile bottom sheet.

3. **LandingPage.razor** (refactored)
   - Remove Technologies section markup (lines ~103–130).
   - Remove Technologies data fetch from `OnInitializedAsync()`.
   - Remove footer Technologies nav link.
   - Add `<FloatingChatWidget />` component at bottom of page (below contact section).
   - Remove `TechnologiesData` field and related code.

### Data Flow

- `FloatingChatWidget` → `ChatShell` → `IChatClient` → Agent service.
- User message in `ChatInput` → `FloatingChatWidget.SendMessage()` → `ChatShell.AddUserMessageAsync()` → streaming response.
- `ChatShell` notifies parent via `OnMessageAdded` callback; parent calls `StateHasChanged()`.

### Styling Approach

- Define `.floating-*` CSS classes in `FloatingChatWidget.razor` (scoped).
- Use CSS custom properties for colors: `var(--color-accent-gradient)`, `var(--color-text-primary)`, `var(--color-surface-card)`, etc. (already defined in landing page).
- Media queries for desktop vs. mobile breakpoints (768px).
- Animations: `slideUp` (desktop), `slideUpMobile` (mobile).

---

## Acceptance Criteria (Feature-Level)

- [ ] Landing page renders without Technologies section.
- [ ] Floating button visible bottom-right, auto-opens mini-chat on first visit.
- [ ] Mini-chat conversation populates with user and assistant messages.
- [ ] Minimize and close buttons work; widget state persists during session.
- [ ] Desktop layout: 400px fixed, positioned bottom-right.
- [ ] Mobile layout: bottom sheet, 100% width, scrollable.
- [ ] File upload disabled in mini-chat input.
- [ ] Full-page `/interview` chat unaffected.
- [ ] No console errors or broken component references.
- [ ] Page build succeeds: `dotnet build InterviewCoach.slnx`.

---

## Success Metrics

1. **Engagement**: Mini-chat opens on >50% of landing page visits (future analytics).
2. **Conversion**: Visit to `/interview` funnels through floating widget (future tracking).
3. **Quality**: No increase in error logs related to chat component.
4. **Simplicity**: Chat logic reused by 2+ surfaces without modification.

---

## Dependencies

- Existing `IChatClient` service and agent endpoint (no changes).
- Existing `ChatInput`, `ChatMessageList`, `ChatMessageItem` components.
- CSS custom properties defined in landing page stylesheet (colors, shadows, radius).
- Blazor component lifecycle and event system.

---

## Open Questions / Deferred

1. **Multi-session persistence**: When to implement browser storage for conversations? (Next iteration)
2. **Keyboard shortcuts**: Ctrl+K to open chat? (Future)
3. **Data cleanup**: Remove unused Technologies JSON and data models? (Future; currently UI-only removal)
4. **Full-page Chat refactor**: Migrate existing `Chat.razor` to use `ChatShell`? (Phase 2)
5. **Analytics**: Track widget opens, message count, conversion to full-page chat? (Future feature)

---

## Risks & Mitigations

| Risk | Mitigation |
|------|-----------|
| **Mobile layout breaks chat**. | Test on multiple devices; use bottom sheet pattern (proven mobile UX). |
| **Chat logic duplication causes bugs**. | Extract to `ChatShell` early; test both mini and full-page surfaces. |
| **Auto-open annoying to users**. | Provide easy minimize; can be toggled in future if usage data suggests otherwise. |
| **Component ref collision (two chats on page)**. | Keep separate `FloatingChatWidget` and `/interview` page; avoid global chat instance. |
| **CSS scoping breaks landing styles**. | Use CSS custom properties; scoped component styles minimize conflicts. |

---

## Rollout Plan

### Phase 1: Core reusable component
- Extract chat logic into `ChatShell.razor`.
- Ensure existing `/interview` chat still works (no behavior change).
- Build and verify locally.

### Phase 2: Floating widget component
- Create `FloatingChatWidget.razor` with desktop layout, open/close state, auto-open logic.
- Style with landing page design tokens.
- Build and verify locally.

### Phase 3: Landing page integration
- Remove Technologies section.
- Integrate `FloatingChatWidget` into landing page.
- Verify footer and nav links are correct.
- Build and verify locally; no regressions.

### Phase 4: Responsive mobile design
- Add media queries for mobile bottom sheet layout.
- Test on mobile emulator / real device.
- Verify animations and touch interactions.

### Phase 5: Ship UI
- All components and styles complete, no tests yet.
- Ready for staging deployment.

### Phase 6: Test suite (future)
- Add bUnit tests for `ChatShell` and `FloatingChatWidget`.
- Add Playwright UI tests for landing page floating chat.
- Verify all acceptance criteria.

---

## Files to Create / Modify

### New Files
- `src/InterviewCoach.WebUI/Components/Pages/Chat/ChatShell.razor`
- `src/InterviewCoach.WebUI/Components/Pages/Chat/FloatingChatWidget.razor`

### Modified Files
- `src/InterviewCoach.WebUI/Components/Pages/LandingPage.razor` (remove Technologies section, integrate widget)
- `src/InterviewCoach.WebUI/Components/Pages/LandingPage.razor.css` (remove `.technologies-*` rules if needed; add scoped styles optional)

### No Changes
- `src/InterviewCoach.WebUI/Components/Pages/Chat/Chat.razor` (full-page chat unchanged)
- `/interview` route (unchanged)
- Data models, services, or backend logic

---

## Appendix: Decision Log

From 16-point grill-me session (2026-03-21):

1. ✅ Scope: Landing page only (not global).
2. ✅ Routes: Keep `/interview` separate.
3. ✅ Architecture: Reusable `ChatShell` component.
4. ✅ Header: Visible (not collapsed).
5. ✅ Initial state: Auto-open, minimizable.
6. ✅ Size: Proportioned for desktop and mobile.
7. ✅ Mobile: Expand to bottom sheet (90vh).
8. ✅ Uploads: Remove from mini-chat.
9. ✅ Session continuity: Not yet (next iteration).
10. ✅ Chat history: Persists in session.
11. ✅ Entry points: Mini-chat opens from floating button.
12. ✅ Accessibility: Deferred (Ctrl+K, focus traps future).
13. ✅ Visual style: Inherit landing page tokens.
14. ✅ Placement conflicts: None identified.
15. ✅ Data cleanup: UI only (fetch removal deferred).
16. ✅ Test bar: Ship UI first, tests later.
