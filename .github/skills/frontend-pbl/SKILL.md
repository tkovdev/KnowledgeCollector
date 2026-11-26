---
name: frontend-pbl
description: 'Draft frontend product backlog items (PBL), user stories, and GitHub issues for Knowledge Collector. Use when asked to plan, expand, or upload a frontend backlog to GitHub.'
argument-hint: 'Describe the frontend feature or backlog scope'
---

# Frontend product backlog

Turn a requested frontend feature or scope into reviewable, independently
actionable backlog issues. Draft first and wait for explicit approval of the
draft before creating or modifying GitHub issues, labels, or Project items.
Even if the initial request mentions uploading, present the draft for approval
before publishing.

## 1. Establish scope and current state

1. Clarify significant product, UX, or scope decisions instead of inventing
   requirements. Keep backend implementation and deployment outside the
   frontend backlog unless the user explicitly includes them. Record backend
   contracts and infrastructure as dependencies where applicable.
2. Read the relevant parts of the repository README, current frontend code, and
   any applicable API contracts. Treat prior scratch files and issues as
   historical context, not proof of current behavior.
3. Check existing GitHub issues, including closed issues and integration
   handoffs, for overlap. Prefer `gh` and inspect actual issue bodies when
   titles alone are insufficient. If GitHub is unavailable, state that
   duplicate checking could not be completed; do not claim otherwise.
4. Read the README's Issue and PR automation section and the current
   `.github/ISSUE_TEMPLATE/frontend-integration-item.md`. The workflow may
   already create integration handoffs for qualifying backend issues. Avoid
   proposing duplicates, and distinguish planned frontend product work from
   backend-triggered `FE Integration:` issues.

## 2. Prepare a scratch backlog

Create a clearly named scratch Markdown file under
`.github/skills/frontend-pbl/scratch/`, creating the directory if needed. This
directory is ignored by Git. If the user explicitly requests another location,
use it and note that Git may not ignore it. Do not overwrite or delete an
existing scratch file; choose a distinct name or ask first. If the user names
an existing file, inspect and preserve its contents before changing it.

Include:

- Goal, frontend scope, current-state evidence, assumptions, and exclusions.
- Suggested order and priority; use P0 for the first usable slice, P1 for
  follow-up improvements, and P2 for later proposals unless the user specifies
  another scheme. Identify dependencies and external blockers.
- For each issue: a stable draft ID, descriptive title, user story or objective,
  testable acceptance criteria, validation expectations, dependencies, and
  out-of-scope work when helpful. Include loading, empty, error, accessibility,
  and responsive states where relevant, without imposing irrelevant criteria.
- Unresolved product or contract decisions, and potential overlap with existing
  issues. Do not represent unverified API behavior as an established contract.
- GitHub handoff notes explaining how the draft relates to the current
  automation and any unresolved labels or Project decisions.

Summarize the draft and provide the file path. Ask for explicit approval before
publishing, and wait. If the user requests changes, revise and seek approval of
the revised draft. Do not create GitHub resources merely because a draft exists.

## 3. Publish approved issues

Only after explicit approval:

1. Verify the target repository, current authentication, available labels, and
   possible duplicates again. If a duplicate or ambiguity is found, ask how to
   proceed rather than creating it. Never treat a failed GitHub query as an
   empty result.
2. Create separate issues for the approved items with their agreed titles,
   priority, descriptions, acceptance criteria, and dependencies. Prefer
   `gh issue create`; use the existing `frontend` label when available. Do not
   create missing labels or add issues to a Project without explicit approval.
   Record priority in the issue body unless agreed priority labels or Project
   fields already exist.
3. Use `integration` and the frontend integration template only for genuine
   backend contract handoffs. Do not use `back-end` or `api-ready` to categorize
   ordinary frontend work; those labels participate in repository automation.
4. Replace draft dependency IDs with real issue references in issue bodies
   after the relevant issues exist. Text links describe dependencies; do not
   claim GitHub-native blocked-by relationships were created unless they
   actually were.
5. If any creation or update fails or its result is uncertain, stop, report
   created issue URLs and the failure, and recheck GitHub before retrying.
   Do not blindly retry or silently leave a partial upload.
6. Verify each created issue's title, body, labels, and dependency references.
   Update the scratch file with issue links and upload status without discarding
   the original draft. Report the resulting links and any incomplete work.
