# Runtime Catalog Parity

`RuntimeHeroCatalog`, `RuntimeUnitCatalog`, `RuntimeBuildingCatalog`, `RuntimeStageCatalog`, and `RuntimeStatusCatalog` are the authoritative gameplay content path in Editor, EditMode, PlayMode, WebGL, macOS, iOS, and Android Player builds.

- Player runtime-generated data remains the canonical balance source.
- Gameplay selection no longer switches to `Resources.Load` under `UNITY_EDITOR`.
- Existing Resources assets, GUIDs, meta files, content IDs, and save identifiers remain preserved for comparison and migration.
- `GameContentDatabase.Validate` checks IDs, required references, ranges, stage enemies, produced units, hero skills/passives, status data, and shared catalog object identity.
- `RuntimeCatalogParityTests` verifies null/duplicate IDs, stable same-ID references, cross-catalog references, boss final waves, and persisted ID compatibility.

Tests that intentionally verify a physical Resources asset remain separate from tests of actual gameplay behavior. New runtime gameplay tests should request content through the Runtime Catalog or `GameContentDatabase`.
