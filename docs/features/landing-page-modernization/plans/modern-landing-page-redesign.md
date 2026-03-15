## Plan: Modern Consultancy Landing Redesign

Create a polished, editorial-industrial landing experience in the existing Blazor page by implementing the previously selected sections with strong typography, restrained motion, and a grey/minimal base that matches the current app tone.

### Steps

**Phase 1: Visual Direction + Tokens**
- Aesthetic direction: editorial-industrial minimalism
- Declare CSS variables for surface, text, accent, muted tones, shadows, radius, and motion timing
- Keep neutral grey base aligned to app global background; reserve accent gradient for key moments only (hero marker, process numbers, final CTA)
- Typography: expressive serif/slab display stack for hero headlines + readable sans for body
- Background texture via pure CSS pseudo-elements (no image assets)

**Phase 2: Hero and Page Composition**
- Asymmetrical composition: left = headline + sub-copy + 2 CTAs; right = value chips/cards
- Differentiator: diagonal accent rule + floating metric chips with staggered animation
- CTAs: primary → `#contact`, secondary → `#services`

**Phase 3: Selected Sections**
1. Services (6 cards with outcome copy, hover: lift + border accent + icon tint)
2. How We Work (4-step timeline, mobile stacks, connector line on desktop)
3. Technologies (grouped badge rail by capability band, hover effect for pointer devices)
4. Contact CTA (accent gradient bg, white text, email CTA from site properties)
5. Footer (lightweight anchor links + social placeholders)

**Phase 4: Content/Data Consistency**
- Contact/company from site-properties.json
- Service wording aligned with services.json

**Phase 5: Motion, Accessibility, QA**
- Single orchestrated page-load stagger reveal
- `prefers-reduced-motion` respected
- Contrast compliance on CTA panel + chips
- Responsive: mobile, tablet, desktop

### Relevant files
- LandingPage.razor
- LandingPage.razor.css
- site-properties.json
- services.json
- app.css

### Scope decisions
- **Included**: Hero, Services, How We Work, Technologies, Contact CTA, Footer
- **Excluded**: Stats bar, case studies, testimonials
- Visual constraint: avoid generic purple-heavy gradient aesthetic

### Recommendations
1. Keep contact as email-first unless a backend submission flow exists
2. Lock expressive local font fallback stacks early if hosted font loading is restricted
3. Centralise content from JSON to prevent messaging drift

---

To re-enable the memory tool and allow me to read/write session notes automatically going forward, go to VS Code Settings → Copilot → Agent Memory and toggle it on.