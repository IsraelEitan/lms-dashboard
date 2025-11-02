# LMS Dashboard — Implementation Roadmap

## Phase Tracker

- [x] Phase 0: Safety Net — Baseline, CI green, lints, findings.md
- [ ] Phase 1: API/test Baseline — Unit tests for services
- [ ] Phase 2: Optional Features — Idempotency for POST, (optionally: ETag/AuthZ stub)
- [ ] Phase 3: Docs/DX Polish — Updated README, docs, test harnesses

## Phase/PR Table

| Phase    | PR (planned)                           | Description                                                  |
|----------|----------------------------------------|--------------------------------------------------------------|
| 0        | docs(findings): Add initial requirements/gaps/plan | As in findings.md (this PR)                            |
| 1        | test(svc): Add unit tests for services | Service/unit test coverage for core business logic           |
| 2        | feat(opt): Implement Idempotency-Key POST | Lightweight idempotency middleware, header support         |
| 3        | docs(readme): Rewrite & clarify README  | Running/testing instructions, choices, prod-vs-stub section  |

---
_Auto-generated, keep this up-to-date with each real PR or major edit._
