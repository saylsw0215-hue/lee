# Audio Integration

The persistent `AudioManager` owns two crossfade music sources and a bounded 12-source SFX pool. Gameplay calls semantic `GameAudioEvent` values; missing `AudioClip` references return `false` and remain silent without throwing.

Connected events include menu/battle/boss music, UI clicks, building placement/upgrade/sale, ally production, basic attacks/hits/critical hits, hero active/ultimate skills, boss appearance, base damage, victory, and defeat.

## Adding licensed clips

1. Import a clip only when redistribution rights are confirmed.
2. Create an `AudioCueData` asset under `Assets/Resources/Audio` named exactly like its `GameAudioEvent`, for example `BattleMusic.asset` or `BuildingPlaced.asset`.
3. Assign Music/Sfx/Ui channel, volume, and concurrency.
4. Record source, author, URL, license, and redistribution terms in `Documentation/ASSET_LICENSES.md`.
5. Verify Master, Music, and SFX volume plus mute persistence after restart.

The same requested BGM is not restarted. Boss music crossfades from battle music; victory and defeat stop combat music. Repeated hit cues are rate-limited, app pause/focus pauses audio, and WebGL defers music until the first user gesture.

No external audio files were added in this work, so the current build intentionally uses the silent fallback.
