# Plan: Refactor Printing and Label Generation

**Quick ID:** 260625-kib
**Date:** 2026-06-25
**Status:** Completed

## Task 1: Add customExpiryDate to frontend models

Add `customExpiryDate?: string | null` to `PrintLabelRequest` and `PrintBatchItem` interfaces in `api.models.ts`.

**Files:** `src/app/shared/models/api.models.ts`
**Action:** Add property to both interfaces
**Verify:** Build compiles
**Done:** Interfaces updated

## Task 2: Fix date serialization in menu-detail component

Change `printSingle` and `printBatch` to pass `customExpiryDate` as raw string (not ISO conversion) to avoid 400 Bad Request.

**Files:** `src/app/features/dashboard/menu/menu-detail/menu-detail.component.ts`
**Action:** Replace ISO conversion with raw string passthrough
**Verify:** Build compiles
**Done:** Date serialization fixed
