# CatosTents project instructions

## Scope

This is a standalone Valheim BepInEx/Jötunn mod for four placeable tent pieces. The tents are decorative structures only:

- Do not add beds, sleep interactions, or spawn-point behavior unless the user explicitly changes the design.
- Preserve the tent's visible orientation and scale from the Unity prefab at runtime.
- Keep each tent piece and visual prefab identifier unique.

## Stable identifiers

| Purpose | Value |
|---|---|
| Plugin GUID | `com.catosvalheim.catostents` |
| Plugin name | `CatosTents` |
| DLL | `CatosTents.dll` |
| Unity bundle | `catostents` |
| Unity project | `C:\Users\magni\Documents\BotsnCoding\Valheim\ASSET-stuff\UNITY\CatosEquipment-Valheim` |
| Active Gale profile | `VHouseholds-Admin` |

## Tent roster

The Unity batch builder and `TentPiece.cs` must stay synchronized:

| Piece ID | Visual prefab | Display name |
|---|---|---|
| `catostent_market_01` | `CatosTent_Market_01_Visual` | Market Tent 01 |
| `catostent_market_02` | `CatosTent_Market_02_Visual` | Market Tent 02 |
| `catostent_market_props_01` | `CatosTent_Market_Props_01_Visual` | Market Tent Props 01 |
| `catostent_market_props_02` | `CatosTent_Market_Props_02_Visual` | Market Tent Props 02 |

## Asset sources

Original tent GLBs are stored outside this repo at:

```text
C:\Users\magni\Documents\BotsnCoding\Valheim\ASSETS-Repo\assetsetuff\tents
```

Do not overwrite the originals. Unity-incompatible `02` files have repaired copies under `repaired-assets\` and in the Unity project's `Assets\CatosTents\Source\` folder.

## Build order

1. Import or repair the visual GLBs in the Unity project.
2. Run **CatosTents > Build All Tent AssetBundles**.
3. Build the plugin:

```text
dotnet build CatosTents.csproj --configuration Release
```

4. Deploy the matching DLL to:

```text
C:\Users\magni\AppData\Roaming\com.kesomannen.gale\valheim\profiles\VHouseholds-Admin\BepInEx\plugins\CatosTents\CatosTents.dll
```

The bundle is embedded in the DLL. Do not deploy a separate bundle file as a substitute for rebuilding the plugin.

## Verification

Test in this order:

1. Confirm all four entries appear in the Hammer under `CatosTents`.
2. Place each tent and check orientation, scale, materials, and visibility.
3. Confirm no bed or spawn-point interaction exists.
4. Test removal, save/reload, and multiplayer synchronization.
