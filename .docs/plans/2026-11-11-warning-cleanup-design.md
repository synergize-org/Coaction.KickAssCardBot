# Build Warning Cleanup Design

## Goal

Remove all existing warnings from the project build without changing runtime behavior.

## Approach

- Correct nullable annotations for DTO/API values that may be absent.
- Preserve existing runtime assumptions and control flow; do not introduce defaults,
  new validation branches, or warning suppressions.
- Correct logging templates so their placeholders match the existing arguments.
- Leave dependencies, output formats, and unrelated code unchanged.

## Verification

Build the solution and confirm that no warnings remain. Review the diff for changes
that could affect runtime behavior.
