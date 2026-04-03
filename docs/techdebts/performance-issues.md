# Performance Tech Debts

Identified: 2026-03-27  
Source: Automated codebase performance analysis

---

## 🔴 Critical

### 1. Blocking `.GetAwaiter().GetResult()` — Thread Pool Starvation
**Files:** `src/InterviewCoach.Agent/Program.cs:47,77` | `src/InterviewCoach.Agent/AgentDelegateFactory.cs:79-80, 134-135`

Four call sites block thread pool threads synchronously during startup and request handling. Risk of deadlock under any `SynchronizationContext` and thread pool exhaustion under load.

```csharp
// Current (bad)
McpClient.CreateAsync(...).GetAwaiter().GetResult();
markitdown.ListToolsAsync().GetAwaiter().GetResult();
```

**Fix:** Make the factory delegates async or use lazy async initialization (e.g. `Lazy<Task<T>>`).

---

## 🟠 High

### 2. O(n²) Argument Parsing
**File:** `src/InterviewCoach.AppHost/LlmResourceFactory.cs:60-72`

`args.ToList()` is called 3× per loop iteration, plus an O(n) `.IndexOf()` search each time. Results in O(n²) complexity and repeated heap allocations.

```csharp
// Fix: convert once and index directly
var argsList = args.ToList();
for (int i = 0; i < argsList.Count; i++) { ... argsList[i + 1] ... }
```

### 3. Redundant DB Fetch + `ExecuteUpdateAsync` — Double Round-Trip
**File:** `src/InterviewCoach.Mcp.InterviewData/InterviewSessionRepository.cs:40-77`

`UpdateInterviewSessionAsync` fetches the entity via `SingleOrDefaultAsync`, modifies it in memory (changes are never saved), then calls `ExecuteUpdateAsync` with the same values — resulting in 2 DB round-trips where 1 would suffice.

```csharp
// Fix: remove the fetch and use ExecuteUpdateAsync directly with the input
await db.InterviewSessions
    .Where(r => r.Id == session.Id)
    .ExecuteUpdateAsync(r => r.SetProperty(p => p.ResumeLink, session.ResumeLink) ...);
```

### 4. Unbounded In-Memory File Store — OOM Risk
**File:** `src/InterviewCoach.Agent/Program.cs:133`

`ConcurrentDictionary<string, byte[]>` stores full file bytes with no TTL, size limit, or eviction policy. Under sustained upload load, heap memory will grow without bound until the process crashes.

```csharp
// Fix option A: IMemoryCache with size limit + expiration
cache.Set(fileId, data, new MemoryCacheEntryOptions {
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
    Size = data.Content.Length
});

// Fix option B: write to temp disk, store path only
var tempPath = Path.Combine(Path.GetTempPath(), fileId);
await File.WriteAllBytesAsync(tempPath, bytes);
```

---

## 🟡 Medium

### 5. `HttpResponseMessage` Not Disposed — Handle Leak
**File:** `src/InterviewCoach.WebUI/Services/FileUploadService.cs:26-40`

The `HttpResponseMessage` returned by `PostAsync` is never wrapped in a `using` statement, leaking connection handles over time.

```csharp
// Fix
using var response = await client.PostAsync("upload", content, cancellationToken);
```

### 6. `.Count()` on `IEnumerable` — Unnecessary Full Enumeration
**File:** `src/InterviewCoach.Mcp.InterviewData/InterviewSessionTool.cs:38`

Calling `.Count()` on an `IEnumerable<T>` enumerates the entire sequence (O(n)) just for logging, separate from the return value.

```csharp
// Fix: materialize first, reuse the list
var list = interviewSessions.ToList();
logger.LogInformation("Retrieved {Count} interview sessions.", list.Count);
return list;
```

### 7. No Query Projection — Full Entity Loaded
**File:** `src/InterviewCoach.Mcp.InterviewData/InterviewSessionRepository.cs:26-31`

`db.InterviewSessions.ToListAsync()` loads all columns including the potentially large `Transcript` field on every list call. Use `.Select()` to project only the fields callers actually need.

### 8. Non-Thread-Safe `static HashSet` — Race Condition
**File:** `src/InterviewCoach.WebUI/Services/FileUploadService.cs:10-13`

`HashSet<T>` is not safe for concurrent reads. Under parallel requests this can corrupt internal state.

```csharp
// Fix
private static readonly ImmutableHashSet<string> AllowedExtensions =
    ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, ".pdf", ".docx", ".doc", ".txt", ".md", ".html");
```

### 9. Uncached Reflection in `WorkflowExtensions`
**File:** `src/InterviewCoach.Agent/WorkflowExtensions.cs`

`GetProperty` and `GetField` are called via reflection on every agent creation with no caching of the resulting `FieldInfo`. Cache the reflection result in a `static readonly` field to pay the cost only once.

---

## Priority Order

| # | Issue | Severity | Risk |
|---|-------|----------|------|
| 4 | Unbounded file cache | High | OOM crash |
| 1 | Blocking async calls | Critical | Thread starvation / deadlock |
| 3 | Double DB round-trip | High | Latency + wasted resources |
| 5 | HttpResponseMessage leak | Medium | Handle exhaustion |
| 2 | O(n²) args parsing | High | Startup perf (low frequency) |
| 6 | `.Count()` on IEnumerable | Medium | Minor CPU waste |
| 7 | No query projection | Medium | Memory / bandwidth |
| 8 | Unsafe static HashSet | Medium | Rare race condition |
| 9 | Uncached reflection | Medium | Per-agent startup cost |
