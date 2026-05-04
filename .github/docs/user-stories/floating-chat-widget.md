# User Stories: Floating Chat Widget on Landing Page

> Source PRD: `./docs/PRDs/PRD-floating-chat-widget.md`

## Scope

**In scope**:
- Remove Technologies section from landing page markup and code.
- Create reusable `ChatShell` component encapsulating chat state and logic.
- Create `FloatingChatWidget` component with open/close UI, auto-open on load, and responsive layouts.
- Integrate widget into landing page.
- Maintain full-page `/interview` chat route as-is.
- Desktop (400px fixed) and mobile (90vh bottom sheet) responsive design.
- Session-scoped conversation history (no cross-session persistence yet).
- Disable file upload in mini-chat UI.

**Out of scope**:
- Multi-session persistence (localStorage, cross-browser storage).
- Keyboard shortcuts (Ctrl+K, Escape) and focus management.
- Data model cleanup (Technologies JSON removal deferred).
- Refactor of full-page `Chat.razor` to use `ChatShell` (Phase 6 future work).
- Test suite implementation (deferred to Phase 11).
- Analytics and engagement tracking.
- Alternative entry points or CTA modifications.

---

## User Stories

### US-1: Remove Technologies Section from Landing Page

**As a** landing page product manager,  
**I want to** hide the outdated Technologies section,  
**So that** the landing page focuses on core services and establishes a clearer coaching entry point.

#### Acceptance Criteria

- [ ] Technologies section HTML removed from `LandingPage.razor` template.
- [ ] Footer navigation link to Technologies (anchor `#technologies`) removed.
- [ ] `OnInitializedAsync()` no longer fetches `/data/technologies.json`.
- [ ] `TechnologiesData` field and `technologiesTask` deleted from code-behind.
- [ ] Page renders without broken layout, missing styles, or console errors.
- [ ] Services, How We Work, and Contact sections remain fully functional.
- [ ] Footer navigation shows only: Services, How We Work, Contact (no Technologies link).
- [ ] Page build succeeds: `dotnet build InterviewCoach.slnx`.

---

### US-2: Create Reusable ChatShell Component

**As a** developer,  
**I want to** extract chat orchestration logic into a standalone `ChatShell` component,  
**So that** mini-chat, full-page chat, and future chat surfaces can share core state and behavior without duplication.

#### Acceptance Criteria

- [ ] New file `ChatShell.razor` created in Components/Pages/Chat/ directory.
- [ ] Component implements `IDisposable` and properly cleans up cancellation tokens.
- [ ] Public property `Messages` returns `List<ChatMessage>` for rendering.
- [ ] Public property `CurrentResponseMessage` returns `ChatMessage?` for in-flight response display.
- [ ] Public property `SessionId` returns current conversation ID as `string`.
- [ ] Public method `async Task AddUserMessageAsync(ChatMessage message)`:
  - Appends user message to `Messages`.
  - Creates streaming response via `IChatClient.GetStreamingResponseAsync()`.
  - Accumulates response tokens and updates `CurrentResponseMessage`.
  - Handles JSON parsing errors gracefully.
  - Fires `OnMessageAdded` callback after response completes.
- [ ] Public method `async Task ResetConversationAsync()`:
  - Clears all messages.
  - Generates new session ID (Guid.NewGuid()).
  - Calls `AddSessionSystemMessages()` to reinitialize system prompt and session.
  - Fires `OnMessageAdded` callback.
- [ ] Public method `void CancelAnyCurrentResponse()`:
  - Cancels active streaming if in progress.
  - Appends in-flight response to messages if partially sent.
  - Clears `CurrentResponseMessage` reference.
- [ ] Parameter `ConversationId` (string?, default null):
  - If provided, overrides default session ID generation.
  - Enables conversation resumption or session pinning.
- [ ] Parameter `IncludeFileUpload` (bool, default true):
  - Accepted but not used in current phase (future: passed to child components).
  - Allows mini-chat and full-page to configure upload behavior variant.
- [ ] Event `OnMessageAdded` (EventCallback):
  - Fires after `AddUserMessageAsync()` completes.
  - Fires after `ResetConversationAsync()` completes.
  - Allows parent to react to state changes via `StateHasChanged()`.
- [ ] System prompt identical to existing `Chat.razor`:
  - > "You are a professional interview coach who helps the user prepare for both behavioral and technical questions."
- [ ] Session ID message format: `"SessionId: {Guid}"`.
- [ ] All logging includes session ID for observability.
- [ ] Component renders no UI (pure logic container).
- [ ] Existing full-page `Chat.razor` component remains unmodified in this phase.
- [ ] Component integrates with existing `IChatClient` service (no service changes).
- [ ] Component integrates with existing `ChatMessageItem.NotifyChanged()` (no changes).
- [ ] Build succeeds: `dotnet build InterviewCoach.slnx`.
- [ ] No console warnings or hydration errors when instantiated.

---

### US-3: Create FloatingChatWidget Component with Open/Close UI

**As a** landing page visitor,  
**I want to** see a floating chat button that opens a compact conversation window,  
**So that** I can ask the interview coach quick questions without navigating away from the landing page.

#### Acceptance Criteria

- [ ] New file `FloatingChatWidget.razor` created in Components/Pages/Chat/ directory.
- [ ] Closed state displays single floating button:
  - Circular, 3.5rem diameter.
  - Chat icon emoji (💬) centered.
  - Positioned fixed bottom-right at 2rem from edges.
  - Background uses `var(--color-accent-gradient)` (inherits landing page theme).
  - Text color white or text-inverse.
  - Box shadow: default (`var(--shadow-xs)`), lift on hover.
  - Hover effect: scale 1.1, shadow upgrade to `var(--shadow-lift)`.
  - Active effect: scale 0.95 (press feedback).
  - Cursor pointer.
- [ ] Clicking floating button transitions to open state:
  - Sets internal `isOpen = true`.
  - Floating button hidden; chat window shown.
  - `StateHasChanged()` called to re-render.
- [ ] Open state displays chat window header:
  - Background: white (or `var(--color-surface-base)`).
  - Title text: "Interview Coach" (font-weight 600, 1rem size).
  - Two action buttons (minimize and close), right-aligned:
    - Minimize button (−): hides window, shows floating button.
    - Close button (✕): same behavior as minimize.
  - Border-bottom 1px solid `var(--color-surface-border)`.
- [ ] Chat window embeds `<ChatMessageList>`:
  - Displays `chatShell.Messages`.
  - Displays `chatShell.CurrentResponseMessage` via `InProgressMessage` prop.
  - Scrollable if message height exceeds container.
  - No horizontal scroll.
  - Padding and margins inherited from `ChatMessageList` component.
- [ ] Chat window embeds `<ChatInput>`:
  - File upload button hidden or disabled (pass parameter to ChatInput to disable upload).
  - Input field sticky at bottom of window.
  - Placeholder text: "Type a message..." (or inherited default).
  - On send, calls `SendMessage(message)` handler which calls `chatShell.AddUserMessageAsync(message)`.
  - On send, calls `chatInput.FocusAsync()` to restore focus.
  - Referenced via `@ref="@chatInput"`.
- [ ] Auto-open behavior on first page load:
  - `OnAfterRenderAsync(firstRender)` detects first render.
  - Waits 500ms (Task.Delay).
  - Sets `isOpen = true`, calls `StateHasChanged()`.
  - Does not auto-open on subsequent re-renders.
- [ ] Desktop layout (screens >768px):
  - Fixed position: bottom 2rem, right 2rem.
  - Width: 400px.
  - Max-height: 600px.
  - Border-radius: 0.75rem.
  - Box shadow: `var(--shadow-lift)` or equivalent (depth, visibility).
  - Background: `var(--color-surface-card)` (or light off-white matching landing).
  - Slide-up animation on open: 300ms, opacity 0→1, translateY +20px→0.
- [ ] Mobile layout (screens ≤768px):
  - Fixed position: bottom 0, right 0, left 0.
  - Width: 100%.
  - Max-height: 90vh.
  - Border-radius: 1rem on top corners only.
  - Slide-up animation: 300ms, opacity 0→1, translateY +100%→0.
  - Message list scrollable; input sticky at bottom.
- [ ] No messages placeholder:
  - Displays default text: "I'm your friendly interview coach. To get started, enter the link or text of your resume and job description."
  - Centered, light text color, inherited size.
- [ ] Internal `ChatShell` instantiation:
  - `<ChatShell @ref="chatShell" IncludeFileUpload="false" OnMessageAdded="StateHasChanged" />`.
  - Referenced as `chatShell` in code-behind.
- [ ] Minimize/close interaction:
  - Click either button sets `isOpen = false`.
  - Floating button re-appears.
  - Conversation history in `chatShell.Messages` is retained in memory (not cleared).
  - Reopening button does NOT reset conversation; history is preserved.
  - Next re-open displays same messages (same session).
- [ ] Component lifecycle:
  - Implements `IDisposable` to clean up `ChatShell` if needed (deferred to Phase 2+).
  - No memory leaks or dangling references.
- [ ] All styles scoped to component or use CSS custom properties (no global side effects).
- [ ] Build succeeds: `dotnet build InterviewCoach.slnx`.
- [ ] Desktop and mobile layouts render correctly in dev tools (Chrome, Firefox inspector).
- [ ] No console warnings or hydration errors.

---

### US-4: Responsive Design and Cross-Browser Compatibility

**As a** mobile and desktop user,  
**I want to** use the chat widget on any device without layout breakage,  
**So that** I can interact with the coach regardless of screen size or browser.

#### Acceptance Criteria

- [ ] Desktop screensize (>768px):
  - Floating widget appears bottom-right at 2rem margin.
  - Window is 400px wide, max-height 600px.
  - Does not overlap page content or footer.
  - No horizontal scroll on any viewport width >768px.
  - Animations smooth and responsive to clicks.
- [ ] Tablet screensize (768px boundary):
  - Correctly transitions between desktop (fixed 400px) and mobile (full width) layouts.
  - No jank or layout shift at breakpoint.
  - Touch interactions work smoothly.
- [ ] Mobile screensize (<768px):
  - Widget expands full screen width (100%).
  - Bottom sheet style: positioned bottom 0, slides up from bottom.
  - Height up to 90vh; scrollable if content exceeds.
  - Input field remains sticky at bottom; does not scroll away.
  - Message list scrollable independently.
  - No horizontal overflow on any width ≤768px.
  - Touch interactions responsive; no lag.
  - Slide-up animation smooth and not stuttering.
- [ ] Button interactions:
  - Floating button: clickable, responsive hover/active states.
  - Minimize/close buttons: keyboard accessible (Tab order), clickable.
  - Input field: keyboard input works; Enter sends message.
  - Focus outlines visible on keyboard navigation (accessibility).
- [ ] Cross-browser compatibility (tested on):
  - Chrome (desktop + mobile emulation).
  - Firefox (desktop + mobile emulation).
  - Safari (desktop + mobile emulation, if available).
  - Edge (desktop).
  - No layout errors or unsupported CSS features.
- [ ] CSS animations:
  - Use standard `@keyframes` (browser-supported).
  - No jank or GPU thrashing.
  - Smooth 60fps transitions on open/close.
  - No console errors for unsupported properties.
- [ ] Message streaming:
  - No layout shift as tokens arrive and accumulate.
  - Auto-scroll to latest message (if applicable via ChatMessageList behavior).
  - No horizontal scroll of message bubbles.
- [ ] No unintended cascading styles:
  - Floating widget CSS does not affect landing page layout.
  - Landing page styles do not break widget layout.
  - CSS specificity and scoping prevent conflicts.
- [ ] Final testing:
  - Minimize and re-open widget multiple times; history persists.
  - Scroll through long message history; no overflow or cutoff.
  - Resize browser window; layout responds correctly.
  - Load page on mobile device (or emulator); widget appears and functions.
- [ ] Build succeeds: `dotnet build InterviewCoach.slnx`.

---

### US-5: Maintain Full-Page Chat Route and Backward Compatibility

**As a** existing user or power user,  
**I want to** continue using the full-page `/interview` chat experience unchanged,  
**So that** my established workflows and bookmarks still work.

#### Acceptance Criteria

- [ ] `/interview` route remains accessible and loads the full-page `Chat.razor` component.
- [ ] Full-page chat header, title, and new-chat button remain visible and functional.
- [ ] File upload is enabled in full-page chat (not affected by mini-chat disabling uploads).
- [ ] Message history and session management behave identically to pre-feature state.
- [ ] No regressions: all existing `/interview` tests (if any) still pass.
- [ ] User navigating directly to `/interview` bypasses landing page; goes straight to full-page chat.
- [ ] Full-page chat can be accessed via link or direct URL without errors.
- [ ] Future refactor (Phase 6) to use `ChatShell` is isolated and does not break current behavior.
- [ ] Build succeeds: `dotnet build InterviewCoach.slnx`.
- [ ] No console errors when loading `/interview` page.

---

## Dependencies and Assumptions

### Dependencies
- Existing `IChatClient` service and agent endpoint (unchanged).
- Existing `ChatInput`, `ChatMessageList`, `ChatMessageItem` components (reused).
- Blazor component lifecycle and event callback system.
- CSS custom properties (colors, shadows, radius) from landing page stylesheet.
- .NET 10 SDK: `dotnet build`, `dotnet run` commands available.

### Assumptions
- Agent service endpoint returns valid JSON streaming responses (no changes needed).
- Current landing page CSS custom properties (e.g., `--color-accent-gradient`) are stable and available in child components.
- Mobile viewport breakpoint at 768px is appropriate for this product (standard assumption).
- Browser supports `position: fixed`, `CSS Grid`, `@keyframes`, and modern Flexbox (no IE11 support needed).
- First-visit detection via flag in component is sufficient for auto-open behavior (user preferences in localStorage deferred).
- Conversation history in memory is acceptable for MVP (session-scoped); no server-side persistence expected yet.

---

## Risks and Mitigations

### Risk 1: Mobile Layout Breaks Chat Input or Message Display
**Probability**: Medium | **Impact**: High (unusable on mobile)

**Mitigation**:
- Use proven bottom-sheet pattern (e.g., Material Design, common in Stripe/Intercom).
- Test on real iOS and Android devices early (Phase 4).
- Use CSS `position: fixed` with careful `bottom`, `left`, `right` values to avoid OS keyboard interference.
- Set `overflow-y: auto` on message container to allow scrolling within constrained height.

---

### Risk 2: Chat Logic Duplication Causes Divergent Behavior
**Probability**: Low | **Impact**: High (support burden, user confusion)

**Mitigation**:
- Extract logic immediately into `ChatShell` (Phase 1) before adding mini-chat.
- Both mini-chat and full-page instantiate same `ChatShell` logic container.
- Shared session ID and system prompt ensure consistency.
- Add assertion/logging to catch deviations early.

---

### Risk 3: Auto-Open Annoying to Returning Users
**Probability**: Medium | **Impact**: Medium (UX friction)

**Mitigation**:
- Auto-open only on true first visit (tracked by JS flag, not localStorage).
- Minimize button is one-click; widget stays minimized if user closes it.
- Metrics in future phase (Phase 10) can measure if auto-open reduces engagement; toggleable then.
- No enforcement; current MVP is optimized for new visitor engagement.

---

### Risk 4: Component Ref Collision (Two Chat Instances on Same Page)
**Probability**: Low | **Impact**: Medium (confusion, state corruption)

**Mitigation**:
- FloatingChatWidget only placed on landing page; `/interview` is separate full-page route.
- No scenario in current design where two chat instances coexist on same DOM.
- Unique session IDs for each `ChatShell` instance prevent state collision at agent level.
- If future feature requires multiple chats on one page, explicit namespacing and instance tracking added then.

---

### Risk 5: CSS Scoping Breaks Landing Page Styles
**Probability**: Low | **Impact**: High (visual regression)

**Mitigation**:
- Use CSS custom properties for colors/sizing (inherited, not hardcoded).
- Scoped component styles in `.razor` files; no global stylesheet modifications.
- Use CSS class namespacing (`.floating-*` prefix) to avoid global selector collisions.
- Visual regression testing (Phase 4) compares landing page before/after.

---

### Risk 6: Streaming Response Causes Layout Thrash
**Probability**: Low | **Impact**: Medium (perceived slowness, battery drain)

**Mitigation**:
- Reuse existing `ChatMessageItem.NotifyChanged()` pattern; proven to stream smoothly.
- No layout changes during token append; only text content updates.
- Disable pointer events during streaming if needed (low priority for MVP).

---

## Test Strategy (Deferred to Phase 11)

- **Unit tests** (bUnit): `ChatShell` state management, message append, session reset, streaming.
- **Component tests** (bUnit): `FloatingChatWidget` open/close state, auto-open delay, minimize/close behavior.
- **UI tests** (Playwright): Landing page loads, widget appears, can send message, responsive layouts.
- **Accessibility tests**: Keyboard navigation, focus management, ARIA labels (Phase 8+).

---

## Success Criteria (Epic-Level)

1. ✅ Landing page renders without Technologies section.
2. ✅ Floating widget appears on landing page; auto-opens on first visit.
3. ✅ Users can send messages and receive responses in mini-chat.
4. ✅ Desktop layout: 400px fixed window at bottom-right.
5. ✅ Mobile layout: full-width bottom sheet, 90vh max height.
6. ✅ Minimize/close buttons hide widget; floating button re-appears.
7. ✅ Conversation history persists while widget open (session scope).
8. ✅ No visual regressions on landing page.
9. ✅ Full-page `/interview` chat unaffected and accessible.
10. ✅ Build passes: `dotnet build InterviewCoach.slnx`, no errors/warnings.
11. ✅ Ready for production merge and staging deployment.
