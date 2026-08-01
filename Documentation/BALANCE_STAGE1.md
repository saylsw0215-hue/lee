# Stage 1 Balance Measurements

The opt-in `StageOneBalanceScenarioPlayModeTests` fixture drives the real Battle scene at 12× time scale. It installs Barracks and Archery Range immediately, buys Magic Tower when affordable, performs one Barracks upgrade, and uses each hero's active and ultimate skills. Results are written under `Builds/Balance` and are not committed.

## Running the measurement

Run it locally without adding the long scenario to ordinary PlayMode regression time:

```bash
UNITY_BIN="/Applications/Unity/Hub/Editor/6000.5.6f1/Unity.app/Contents/MacOS/Unity"
"$UNITY_BIN" -batchmode -nographics -projectPath "$PWD" \
  -runTests -testPlatform playmode -assemblyNames HeroDefense.PlayModeTests \
  -testFilter HeroDefense.Tests.PlayMode.StageOneBalanceScenarioPlayModeTests \
  -testResults "$PWD/Builds/Balance/balance-tests.xml" \
  -logFile "$PWD/Builds/Logs/balance-tests.log"
```

On GitHub Actions, choose **Run workflow → balance**. JSON measurements, test XML, and the Unity log are uploaded as the `HeroDefense-Stage1-Balance-*` artifact. Each scenario has a timeout and reports NaN/Infinity, invalid health/gold/attack intervals, missing boss, stalled wave, missing terminal result, and continuously increasing active-object counts.

## Before adjustment

Measured on Normal difficulty with the original 500 starting Gold and 116 enemies.

| Hero | Result | Stage time | Base HP | Final Gold |
| --- | --- | ---: | ---: | ---: |
| Arden | Victory | 157.3 s | 100 | 3,130 |
| Rian | Victory | 155.9 s | 100 | 3,130 |
| Sera | Victory | 156.1 s | 100 | 3,130 |

The fixed baseline strategy won every run in about 2.6 minutes without base damage, so the stage did not provide the requested 8–12 minute learning curve.

## Applied adjustment

- Starting Gold: 500 → 300.
- Enemy count: 116 → 152, concentrated progressively in Waves 4–9.
- Wave clear rewards were reduced while monster kill rewards and all content IDs stayed unchanged.
- Goblin Chieftain: 1,800 → 2,600 base HP with a small attack/defense increase.
- Stage 1 Normal applies a stage-local 10× enemy health and 0.4× enemy damage profile. Easy and Hard still apply their existing independent health, damage, speed, economy, and base modifiers on top.
- Stage 2–4 data was not rebalanced.

The large Stage 1 health multiplier is intentionally stage-local. It extends encounter duration without changing shared UnitData IDs or the balance of later stages.

## After adjustment

| Hero | Result | Stage time | Base HP | Final Gold | Allies produced | Hero damage | Hardest wave |
| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |
| Arden | Victory | 527.5 s | 100 | 3,425 | 116 | 17,099 | Wave 10 (125.4 s) |
| Rian | Victory | 521.3 s | 100 | 4,975 | 140 | 31,031 | Wave 10 (125.0 s) |
| Sera | Victory | 540.5 s | 100 | 5,475 | 151 | 25,689 | Wave 10 (119.6 s) |

All three reference runs complete in 8.7–9.0 minutes and reach the boss. These are deterministic reference-strategy results, not a statistically valid player win-rate sample; real-player Normal win rate remains a manual telemetry target of 55–70%.

## Remaining balance risks

- The reference automation uses skills efficiently and therefore reports 100% success; novice-player success requires playtesting.
- Gold Mine ownership makes final Gold hero- and battle-state-dependent.
- The 10× Stage 1 health profile should be checked on a physical iPhone for pacing and thermal impact.
- Easy/Hard full-run distributions and Stage 2–4 balance were intentionally left unchanged.
