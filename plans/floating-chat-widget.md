# Plan: Floating Chat Widget on Landing Page

> Source PRD: `./docs/PRDs/PRD-floating-chat-widget.md`

## Architectural Decisions

Durable decisions that apply across all phases:

- **Routes**: 
  - Landing page: `GET /` (modified to include widget)
  - Full-page chat: `GET /interview` (unchanged, kept as fallback)
- **Schema**: No database changes; all state is in-memory, conversation scoped to current browser session.
- **Key components**: 
  - `ChatShell.razor`: stateful logic container (messages, session ID, streaming)
  - `FloatingChatWidget.razor`: UI wrapper with open/close state and responsive layout
  - Existing `ChatInput`, `ChatMessageList`, `ChatMessageItem` components reused
- **Styling approach**: 
  - CSS custom properties for colors, inherited from landing page theme
  - Scoped component styles in `.razor` files
  - Media query breakpoint at 768px for mobile bottom sheet
- **Integration**: Widget instantiated in `LandingPage.razor` below contact CTA section
- **Dependencies**: No backend or service changes; reuses existing `IChatClient` and agent service

---

## Phase 1: Extract Chat Core Logic into Reusable Component

**User stories**: US-2 (Create Reusable ChatShell Component)

### What to Build

Refactor the stateful chat logic from the existing full-page `Chat.razor` component into a new, reusable `ChatShell.razor` component. This component will own all message state, conversation ID management, streaming response handling, and session lifecycle. It exposes a public API (methods and properties) but renders no UI—acting as a state container and orchestrator that both mini-chat and full-page chat can use.

The component accepts parameters for conversation ID override and file upload toggle (for future flexibility), and fires a callback event after each message operation to notify parents of state changes.

### Acceptance Criteria

- [ ] `ChatShell.razor` created with `@implements IDisposable`
- [ ] Public property `Messages` exposes `List<ChatMessage>`
- [ ] Public property `CurrentResponseMessage` exposes `ChatMessage?` for in-flight response
- [ ] Public property `SessionId` exposes current `string` session identifier
- [ ] Public method `AddUserMessageAsync(ChatMessage)` accepts user message, sends to agent, streams response
- [ ] Public method `ResetConversationAsync()` clears messages, generates new session ID
- [ ] Public method `CancelAnyCurrentResponse()` stops cancellation token and clears current response
- [ ] Parameter `ConversationId` (string?) allows override of default session ID
- [ ] Parameter `IncludeFileUpload` (bool, default `true`) prepares for file handling variants
- [ ] Event `OnMessageAdded` callback fires after `AddUserMessageAsync()` completes
- [ ] Streaming response uses same `IChatClient.GetStreamingResponseAsync()` pattern as current `Chat.razor`
- [ ] System prompt and session ID management identical to current full-page chat
- [ ] Log messages include session ID for debugging
- [ ] Existing full-page `Chat.razor` remains unchanged (no integration yet, Phase 2 concern)
- [ ] Build succeeds: `dotnet build InterviewCoach.slnx`
- [ ] No console errors when scoped to isolated test; component hydrates cleanly

---

## Phase 2: Build Mini-Chat Widget UI with Responsive Layout

**User stories**: US-3 (Create FloatingChatWidget Component), US-4 (Responsive Behavior and Animations)

### What to Build

Create `FloatingChatWidget.razor` component that provides the complete mini-chat UI experience: floating button (closed), and chat window (open). The component manages toggle state between button and window, handles minimize/close interactions, and provides responsive layouts for desktop (fixed 400px window) and mobile (bottom sheet 90vh). Internally instantiate and reference `ChatShell` to power conversations.

The widget auto-opens 500ms after first render, but users can minimize or close. Styling inherits landing page design tokens (colors, shadows, border radius) for visual consistency.

### Acceptance Criteria

- [ ] `FloatingChatWidget.razor` created with internal `ChatShell` instance (`IncludeFileUpload="false"`)
- [ ] Floating button visible when closed: 3.5rem diameter, circular, chat icon (💬), bottom-right positioned
- [ ] Button styling uses `var(--color-accent-gradient)` for background, inherits landing colors
- [ ] Button hover effect: scale 1.1, shadow lift; active: scale 0.95
- [ ] Clicking floating button opens chat window
- [ ] Chat window displays header with "Interview Coach" title
- [ ] Header includes minimize (−) and close (✕) buttons
- [ ] Minimize button: hides window, shows floating button again (state toggles)
- [ ] Close button: same behavior as minimize (context decision: no distinction for MVP)
- [ ] Window embeds `<ChatMessageList>` showing `chatShell.Messages` and `chatShell.CurrentResponseMessage`
- [ ] Window embeds `<ChatInput>` component, sends messages via `chatShell.AddUserMessageAsync()`
- [ ] Opening animation: slideUp 300ms, opacity 0→1 + translateY 20px→0
- [ ] Desktop layout: fixed position bottom 2rem, right 2rem; 400px width; max-height 600px
- [ ] Mobile (max-width 768px): fixed position bottom 0, right 0; 100% width; max-height 90vh
- [ ] Mobile animation: slideUpMobile, translateY 100%→0
- [ ] Windows 10/11 smooth transitions on all interactions
- [ ] Auto-open logic: `OnAfterRenderAsync(firstRender)` waits 500ms then sets `isOpen = true`
- [ ] User can re-open by clicking floating button after minimizing
- [ ] `StateHasChanged()` called after message send to refresh UI
- [ ] Component references `ChatInput` with `@ref`, calls `FocusAsync()` on open
- [ ] Build succeeds: `dotnet build InterviewCoach.slnx`
- [ ] Desktop/mobile layouts tested in browser dev tools; no overflow or layout shift

---

## Phase 3: Integrate Widget into Landing Page and Remove Technologies Section

**User stories**: US-1 (Remove Technologies Section), US-5 (Maintain Full-Page Chat Route)

### What to Build

Modify `LandingPage.razor` to remove the Technologies section (markup, footer link, and code-behind fetch/field) and add the `<FloatingChatWidget />` component. Ensure the footer and hero CTA remain functional and the page layout is clean. Verify that the `/interview` full-page chat route is untouched and that no navigation regressions occur.

This phase glues the new components into the product surface.

### Acceptance Criteria

- [ ] Technologies section markup removed from `LandingPage.razor` (lines ~103–130)
- [ ] Footer navigation link to `#technologies` removed
- [ ] `OnInitializedAsync()` no longer fetches `/data/technologies.json`
- [ ] `TechnologiesData` field removed from code-behind
- [ ] `technologiesData` local variable removed from `OnInitializedAsync()`
- [ ] `<FloatingChatWidget />` component added to landing page (suggested: after contact section, before footer)
- [ ] FloatingChatWidget renders with no console errors
- [ ] Landing page loads without hydration warnings or missing reference errors
- [ ] Footer links correctly point to: Services (#services), How We Work (#how-we-work), Contact (#contact)
- [ ] Static "Get in Touch" CTA button still present and functional
- [ ] Hero headline, value chips, services cards, and process steps all render correctly
- [ ] No layout shift or overflow after Technologies removal
- [ ] Full-page `/interview` chat route still accessible and unmodified
- [ ] Floating widget appears on landing page with 2rem bottom/right margin
- [ ] Widget auto-opens on first visit as designed
- [ ] Build succeeds: `dotnet build InterviewCoach.slnx`
- [ ] No browser console errors on landing page load

---

## Phase 4: Verify Responsive Behavior and Cross-Browser Compatibility

**User stories**: US-4 (Responsive Behavior and Animations)

### What to Build

Test the floating widget across desktop and mobile viewports, verify CSS media queries apply correctly, and validate animations and interactions in multiple browsers. Ensure touch events work on mobile, keyboard events work on desktop, and no layout regressions occur at any breakpoint.

This phase is validation-focused and may surface CSS tweaks.

### Acceptance Criteria

- [ ] Desktop layout (>768px): widget appears 400px wide, fixed bottom-right, no overlap with page content
- [ ] Desktop: minimize/close buttons responsive to click; window animations smooth
- [ ] Mobile layout (≤768px): widget expands full width, appears as bottom sheet, max-height 90vh
- [ ] Mobile: animations use slideUpMobile keyframes; no jank or stuttering
- [ ] Mobile: touch interactions on buttons and input work without lag
- [ ] Desktop: hover states on buttons (minimize, close, floating button) work as designed
- [ ] Tablet (iPad-like, 768px to 1024px): layout correct; widget positioning stable
- [ ] Chat messages scroll within constrained window; no horizontal scroll on mobile
- [ ] Input field remains at bottom and sticky within mobile layout
- [ ] Tested on Chrome, Firefox, Safari (desktop and mobile browser equivalents or emulators)
- [ ] Focus outlines visible and keyboard navigation works on desktop
- [ ] No layout shift on message arrival or response streaming
- [ ] Minimize and re-open preserves message history; state stable
- [ ] Close and re-open by clicking floating button starts fresh conversation (expected behavior)
- [ ] No unintended CSS conflicts with landing page styles
- [ ] Build succeeds: `dotnet build InterviewCoach.slnx`

---

## Phase 5: Acceptance and Readiness for Production

**User stories**: All (US-1 through US-5)

### What to Build

Final smoke testing, README updates if needed, and deployment readiness checklist. Ensure all acceptance criteria from prior phases are met, no regressions in full-page chat, and documentation reflects the new landing page structure.

### Acceptance Criteria

- [ ] All Phase 1–4 acceptance criteria verified and passing
- [ ] Full-page `/interview` chat tested and working (file upload, header, message history)
- [ ] Landing page loads without errors; Technologies section confirmed removed
- [ ] Floating widget auto-opens on first landing page visit
- [ ] Floating widget can be minimized and re-opened by button click
- [ ] Conversation history persists across minimize/re-open in same session
- [ ] Desktop and mobile responsiveness confirmed in staging environment
- [ ] No new console warnings or errors in browser dev tools
- [ ] Build pipeline passes: `dotnet build InterviewCoach.slnx`
- [ ] All commit messages follow Conventional Commits (feat, fix, refactor, etc.)
- [ ] Feature ready for merge to main (staging deploy follows separately)
- [ ] Floating widget visual design approved by product/design stakeholder
- [ ] Code review completed (if required by repo policy)

---

## Next Steps After Phase 5

Once Phase 5 is complete, the following work is deferred to future iterations:

### Phase 6: Refactor Full-Page Chat to Use ChatShell (Future)
- Migrate existing `/interview` `Chat.razor` to use `ChatShell` backend.
- Ensure no behavioral change.
- Reduce code duplication.

### Phase 7: Add Session Persistence (Future)
- Implement browser session storage (localStorage) for multi-page navigation continuity.
- Restore conversation when user returns within same session.
- Define session timeout policy.

### Phase 8: Add Keyboard Shortcuts and Accessibility (Future)
- Ctrl+K to open/close widget.
- Escape to close.
- Focus trap within widget while open.
- ARIA labels and roles for screen readers.

### Phase 9: Data Cleanup (Future)
- Remove unused `/data/technologies.json` file.
- Remove `TechnologiesData` and `TechnologyBand` models.
- Remove Technologies CSS rules from landing page stylesheet.

### Phase 10: Add Analytics and Instrumentation (Future)
- Track widget opens, message count, conversion to full-page chat.
- Measure engagement metrics.
- Optimize auto-open timing if needed based on user behavior.

### Phase 11: Test Suite (Future)
- Add bUnit tests for `ChatShell` and `FloatingChatWidget`.
- Add Playwright UI tests for landing page floating chat interactions.
- Test mobile bottom sheet behavior.
- Verify accessibility compliance.
