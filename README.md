# Hero Defense — Runtime parity and gameplay stability

> GitHub Actions, 브라우저용 GitHub Pages, macOS/iOS 아티팩트 설정은 [GITHUB_SETUP.md](GITHUB_SETUP.md)를 참고하세요.

`Hero Defense`는 macOS에서 개발하고 WebGL·iOS·Android를 목표로 하는 가로형 전략 디펜스 프로젝트입니다. Editor와 Player는 동일한 Runtime Catalog를 사용하며, 자동 밸런스 측정·전투 피드백·안전한 오디오 이벤트·장시간 수명주기 회귀 검증을 포함합니다.

## 권장 환경과 프로젝트 열기

- 현재 설치/검증 버전: Unity `6000.5.6f1` Apple Silicon
- macOS 앱 빌드: Unity Hub의 Mac Build Support
- iOS 프로젝트 생성: Unity Hub의 iOS Build Support
- 실제 iPhone 실행: 최신 안정 Xcode, Apple ID 및 필요한 코드 서명 자격

Unity Hub에서 **Open > Add project from disk**로 이 폴더를 엽니다. 최초 package import 후 `Tools > Hero Defense > Setup Phase 6`를 실행하십시오. 이 메뉴는 이전 Phase 설정과 Build Scenes를 검증하고 비어 있는 고급 능력치를 마이그레이션하며 상태 효과와 특수 적 데이터만 생성합니다. 이미 작성한 고급 수치는 보존합니다. 이후 `Assets/Scenes/Boot.unity`를 열어 Play하면 Boot → MainMenu → HeroSelect → Battle 흐름을 확인할 수 있습니다.

URP와 Input System이 선언되어 있습니다. Canvas는 1920×1080, Match 0.5 기준이며 Safe Area가 노치, Dynamic Island, 둥근 모서리와 화면 크기 변경을 추적합니다. 마우스 클릭은 UI 터치와 동일하게 동작합니다.

## Phase 1 기능

- MainMenu: 게임 시작, 런타임 음량/진동 설정, 플랫폼 안전 종료
- Battle: Gold 500, Wave 0, Base HP 100/100의 이벤트 기반 HUD와 네 전장 구역
- 병영/사격장/마법소 단일 선택 및 같은 버튼 재클릭 해제
- 일시정지, 계속하기, 메인 메뉴 복귀, 종료
- Escape 및 Android Back 대응
- Editor 테스트 키: `G` 골드 +100, `W` 웨이브 +1, `H` 본진 HP -10

## Phase 2 자동 전투

- 검사(`player_swordsman`): HP 100, 이동 2.0, 공격 20/1.0초, 가장 가까운 Enemy 탐색
- 슬라임(`enemy_slime`): HP 60, 이동 1.6, 공격 8/1.2초, 처치 보상 10 Gold
- 고블린(`enemy_goblin`): HP 90, 이동 2.2, 공격 12/0.9초, 처치 보상 15 Gold
- 적은 살아 있는 아군 유닛을 우선하며, 대상이 없으면 아군 본진을 공격합니다.
- 대상 탐색은 활성 개체만 보관하는 `CombatRegistry` 목록을 0.2초 주기로 순회합니다. 매 프레임 Find/LINQ를 사용하지 않습니다.
- 이동, 사거리 정지, 공격 간격, 피격·사망, 다음 대상 재탐색이 자동입니다.
- 유닛은 `CombatPool`에서 가져오며 사망·초기화 시 Destroy하지 않고 풀로 돌아갑니다.
- 공유 Canvas 안의 체력바, 피격 플래시, 공격 펄스, 풀링된 데미지 숫자로 전투 상태를 표시합니다.
- 본진 `HealthComponent`가 실제 HP의 원본이며 변경 이벤트가 `BattleSessionState`와 HUD를 갱신합니다. HP 0에서 패배 패널은 한 번만 열립니다.

Battle 하단의 **개발용 소환**에서 `검사 소환`, `슬라임 소환`, `고블린 소환`을 사용할 수 있습니다. 적 최대 20마리이며, 검사 직접 소환은 개발 확인용입니다. Phase 3의 일반 아군 생산은 아래 건설 시스템을 사용합니다.

## Phase 3 건설과 자동 생산

Battle 하단에서 건물을 선택한 뒤 아군 진영에 표시된 빈 슬롯을 터치하면 즉시 건설됩니다. 같은 버튼을 다시 누르거나 `선택 취소`를 누르면 건설 선택이 해제됩니다. 골드가 부족하거나 일시정지·패배 상태이면 건설되지 않습니다.

| 건물 | 건설 비용 | 생산 유닛 | Lv.1 / Lv.2 / Lv.3 생산 주기 |
| --- | ---: | --- | --- |
| 병영 | 100 | 검사 | 5.0 / 4.2 / 3.5초 |
| 사격장 | 140 | 궁수 | 6.0 / 5.0 / 4.0초 |
| 마법소 | 180 | 마법사 | 8.0 / 6.8 / 5.5초 |

- 총 8개 고유 슬롯이 있으며 점유된 슬롯에는 중복 건설할 수 없습니다.
- 0.7초 건설 연출이 끝난 뒤 건물별 독립 생산 타이머가 시작됩니다.
- 건물 위 수평 게이지가 건설 또는 생산 진행률을 표시합니다.
- 설치된 슬롯을 터치하면 이름, 레벨, 생산 유닛, 현재/다음 생산 주기와 판매 가격이 표시됩니다.
- 업그레이드는 최대 3레벨이며 생산 진행률 비율을 유지하면서 주기만 단축합니다.
- 판매 전 확인창이 표시됩니다. 판매액은 건설비와 지불한 업그레이드 비용 합계의 50%이며 한 번만 지급됩니다.
- 판매된 슬롯은 즉시 비어 다시 건설할 수 있고, 이미 생산된 유닛은 유지됩니다.
- 아군 30명에 도달하면 생산 게이지가 100%에서 대기하고 빈자리가 생기면 생산합니다.
- 일시정지와 패배 중에는 건설, 업그레이드, 판매, 생산이 중단됩니다.
- `전투 초기화`는 유닛뿐 아니라 모든 건물, 슬롯, 패널과 생산 타이머도 초기화합니다.

신규 궁수(`player_archer`)는 HP 65, 공격 14, 사거리 4.5의 원거리 유닛입니다. 마법사(`player_mage`)는 HP 55, 공격 24, 사거리 4.0의 고화력 원거리 유닛입니다. 둘 다 기존 Team, CombatRegistry, HealthComponent와 CombatPool을 사용하며 화살과 마법탄 비주얼도 풀링됩니다. 수동 몬스터 소환은 자동 웨이브와 별도로 개발 검증용으로만 유지됩니다.

## Phase 4 — 초원의 관문

Stage 1 `stage_01_grassland`는 “초원의 관문”이며 Battle 진입 즉시 Wave 1 준비 카운트다운이 시작됩니다. 준비 중에도 건설과 아군 생산은 계속됩니다. 카운트다운이 끝나면 두 Enemy Spawn Point에서 몬스터가 자동 생성됩니다. `웨이브 즉시 시작`은 준비 상태에서만 사용할 수 있습니다. 기존 몬스터 소환 버튼은 개발 확인용으로 유지됩니다.

| Wave | 구성 | 준비 | 클리어 보상 |
| ---: | --- | ---: | ---: |
| 1 | 슬라임 5 | 8초 | 30 |
| 2 | 슬라임 8 | 7초 | 35 |
| 3 | 슬라임 6, 고블린 3 | 7초 | 40 |
| 4 | 고블린 7 | 6초 | 45 |
| 5 | 슬라임 6, 고블린 5, 정예 고블린 1 | 8초 | 80 |
| 6 | 슬라임 10, 고블린 6 | 6초 | 55 |
| 7 | 고블린 10, 정예 슬라임 2 | 6초 | 65 |
| 8 | 슬라임 12, 고블린 10 | 5초 | 75 |
| 9 | 정예 고블린 3, 정예 슬라임 3, 고블린 8 | 8초 | 100 |
| 10 | 고블린 대장 1, 고블린 8, 정예 고블린 2 | 12초 | 200 |

- 정예 슬라임: HP 240, 보상 40 Gold, 큰 크기와 오라
- 정예 고블린: HP 280, 보상 50 Gold, 큰 무기와 붉은 오라
- 고블린 대장: HP 1800, 공격 45, 보상 300 Gold, 전용 상단 체력바
- 활성 적은 최대 35마리이며 한도에 도달하면 생성 수량을 잃지 않고 대기합니다.
- 웨이브는 모든 예약 생성 완료와 해당 웨이브 적 전멸을 모두 만족해야 완료됩니다.
- 개별 처치 보상과 별도로 WaveData 클리어 보상이 한 번 지급됩니다.
- 마지막 보스 웨이브 완료와 본진 생존 시 승리 패널이 표시됩니다.
- 결과 화면은 완료 웨이브, 처치 수, 획득 골드, 본진 HP와 플레이 시간을 표시합니다.
- 본진 파괴 시 웨이브 생성·보상·승리가 중단되고 기존 패배 패널이 표시됩니다.
- 다시 플레이는 유닛, 건물, 슬롯, 골드, 본진, 통계와 Wave 1 준비 상태를 초기화합니다.
- 앱이 포커스를 잃거나 iOS 백그라운드로 이동하면 자동 일시정지되며 자동 재개하지 않습니다.

개발 검증은 준비 중 `웨이브 즉시 시작`을 사용합니다. `WaveManager.ForceClearCurrentWave`는 테스트용 무보상 강제 제거 후 정상 웨이브 클리어 보상만 처리하도록 분리되어 있습니다.

## Phase 5 — 영웅 선택과 스킬

메인 메뉴에서 `게임 시작`을 누르면 HeroSelect 씬으로 이동합니다. 세 영웅 카드 중 하나를 선택한 뒤 `전투 시작`을 누르면 선택값이 씬 전환 중 유지되고 Battle의 아군 본진 앞에 영웅 한 명만 생성됩니다.

| 영웅 | 역할 | HP | 공격 | 액티브 스킬 | 궁극기 |
| --- | --- | ---: | ---: | --- | --- |
| 아르덴 | 근접 탱커 | 500 | 35 | 방패 강타: 180% 범위 피해와 짧은 경직 | 수호자의 맹세: 피해 감소와 주변 피해 |
| 리안 | 원거리 지속 딜러 | 320 | 28 | 화살비: 3초 동안 5회 범위 피해 | 매의 눈: 공속·사거리 증가와 추가 관통 |
| 세라 | 광역 폭발 딜러 | 280 | 45 | 화염 폭발: 220% 범위 피해와 화상 | 메테오: 밀집 지역 고화력 피해 |

- 영웅은 일반 생산 유닛과 별도로 존재하며 최대 아군 30명 제한에 포함되지 않습니다.
- 기본 공격은 기존 `CombatRegistry`, Team, DamageInfo와 대상 탐색을 재사용합니다. 아군 오인 공격은 차단됩니다.
- 패시브는 각각 본진 위기 시 피해 감소, 동일 대상 세 번째 공격 강화, 주기적 불씨 범위 피해입니다.
- 스킬 버튼은 적이 있을 때만 동작하고 사용 즉시 쿨타임이 시작됩니다. 같은 입력을 중복 처리하지 않습니다.
- 궁극기 에너지는 기본 공격과 처치로 충전되며 100에서 버튼이 활성화되고 사용 시 0이 됩니다.
- 전용 HUD는 초상화, HP, 상태, 스킬 쿨타임과 궁극기 게이지를 표시합니다.
- HP가 0이 되면 영웅은 전투 대상에서 빠지고 영웅별 부활 시간 후 최대 HP로 복귀합니다. 부활 직후 2초 동안 피해를 받지 않습니다.
- 일시정지, 승리와 패배 중에는 이동, 공격, 쿨타임, 부활 타이머와 스킬 입력이 중단됩니다.
- 결과 화면 통계에는 선택 영웅, 피해량, 처치, 스킬·궁극기 사용, 사망·부활과 최대 단일 피해가 포함됩니다.

플레이스홀더를 정식 에셋으로 교체하려면 각 `Assets/Resources/HeroData` 에셋의 Portrait, Full Body Visual, Prefab을 지정하고, `Assets/Resources/HeroSkills`의 Icon을 교체합니다. 데이터 에셋의 원본 수치는 런타임에 수정하지 않습니다.

## Phase 6 — 고급 전투와 수동 조준

피해는 유효성 → 무적 → 회피 → 주는 피해 → 치명타 → 관통 → 방어력 → 받는 피해 → 보호막 → 체력 순으로 한 번만 계산됩니다.

- 물리 피해는 물리 방어력과 관통을 사용하며 일반 공격은 회피·치명타가 가능합니다.
- 마법 피해는 마법 방어력을 사용합니다. 마법사 기본 공격과 화염 스킬이 해당합니다.
- 고정 피해는 방어력을 무시하지만 무적과 전체 피해 감소 및 보호막은 적용됩니다.
- 방어 감소율은 `유효 방어력 / (100 + 유효 방어력)`입니다. 비율 관통 후 고정 관통을 적용하며 방어 후 최소 5% 피해를 보장합니다.
- 치명타 기본 배율은 1.5배이며 확률은 0~100%로 제한됩니다. 지속 피해와 고정 피해는 치명타가 발생하지 않습니다.
- 최종 회피율은 `대상 회피 - 공격자 명중`이고 0~75%로 제한됩니다. 회피하면 피해, 상태 효과, 궁극기 에너지가 발생하지 않습니다.
- 보호막은 오래된 것부터 체력보다 먼저 소모되며 지속시간과 출처별 제거를 지원합니다.

상태 효과는 개체별 `StatusEffectController` 하나에서 함께 Tick하므로 효과마다 Update를 만들지 않습니다. 중첩·갱신·강한 효과 교체 규칙과 최대 중첩 수는 ScriptableObject 데이터에 저장됩니다.

| 상태 | 동작 |
| --- | --- |
| 기절 | 이동·공격·스킬 차단 |
| 빙결 | 이동을 거의 완전히 정지하고 공격 차단 |
| 화상 | 매초 마법 지속 피해, 최대 3중첩 |
| 독 | 매초 지속 피해, 최대 5중첩 |
| 감전 | 받는 마법 피해 15% 증가, 공격속도 15% 감소 |
| 둔화 | 가장 강한 이동속도 감소만 유지 |
| 침묵 | 기본 공격은 허용하되 영웅 스킬·궁극기 차단 |
| 도발 | 유효한 도발 시전자를 최우선 대상으로 지정 |
| 무적 | 피해와 적대 상태 적용 차단 |

정예는 30%, 보스는 60%의 군중 제어 저항을 사용합니다. 최종 제어 시간은 기본 지속시간의 20%보다 짧아지지 않습니다. 상태 종료와 전투 초기화 시 해당 출처의 능력치 수정자, 아이콘과 보호막이 제거됩니다.

스킬 버튼을 누르면 원형 또는 부채꼴 미리보기가 열립니다. 전장을 터치하면 해당 위치/방향으로 발동하며 최대 사거리 밖 입력은 사거리 끝으로 자동 보정됩니다. `조준 취소`를 누르면 쿨타임, 궁극기 에너지, 시전 상태와 이펙트가 전혀 발생하지 않습니다. 일시정지·사망·승리·패배 시 조준은 자동 취소됩니다.

- 방패 강타: 수동 부채꼴, 물리 피해, 1.5초 기절과 3초 도발
- 수호자의 맹세: 피해 감소와 자신 20%·주변 아군 8% 보호막
- 화살비: 수동 원형, 5회 물리 피해와 둔화
- 매의 눈: 공격속도·사거리·치명타 확률 및 관통 강화
- 화염 폭발: 수동 원형 마법 피해와 화상
- 메테오: 수동 원형, 중심/외곽 차등 피해, 화상·기절·둔화

개발용 소환 영역에는 `독 고블린`과 `주술 고블린` 버튼이 추가됐습니다. 독 고블린은 원거리 공격으로 독을 부여하고, 주술 고블린은 10초마다 주변 적 공격력을 5초 동안 15% 높입니다.

## macOS 앱 빌드

Editor에서 `Tools > Hero Defense > Build > macOS App`을 실행합니다. 빌드는 Boot/MainMenu/HeroSelect/Battle 순서를 검증하고 다음 Universal 앱 번들을 생성합니다.

```text
Builds/macOS/HeroDefense.app
```

기본 설정은 1280×720 창 모드, 창 크기 변경 가능, Intel 64-bit와 Apple Silicon Universal 아키텍처입니다. 외부 서명과 공증은 Phase 2 범위가 아닙니다.

명령줄에서는 다음을 실행합니다.

```bash
chmod +x build-macos.sh
./build-macos.sh
```

Unity 설치 경로가 다르면 환경 변수로 지정할 수 있습니다.

```bash
UNITY_EDITOR="/Applications/Unity/Hub/Editor/6000.5.6f1/Unity.app/Contents/MacOS/Unity" ./build-macos.sh
```

로그는 `Builds/Logs/macos-build.log`에 저장됩니다. 스크립트는 Unity 종료 코드와 실제 `.app` 디렉터리를 모두 검사합니다.

## iOS Xcode 프로젝트 생성

Unity Hub에서 해당 Editor 버전의 **iOS Build Support**를 설치한 후 다음 중 하나를 사용합니다.

- `Tools > Hero Defense > Build > iOS Xcode Project`
- `Tools > Hero Defense > Build > iOS and Open Xcode`
- `./build-ios.sh`

셸 스크립트가 실행 가능하지 않다면 먼저 `chmod +x build-ios.sh`를 실행합니다. 출력과 로그는 다음과 같습니다.

```text
Builds/iOS/HeroDefenseXcode
Builds/Logs/ios-build.log
```

성공 판정은 `Unity-iPhone.xcodeproj` 또는 `Unity-iPhone.xcworkspace`의 실제 존재 여부를 기준으로 합니다. 이 단계는 Xcode 프로젝트만 생성하며 서명된 IPA나 iPhone 설치 완료를 의미하지 않습니다.

iOS 설정은 ARM64, IL2CPP, Metal, Landscape Left/Right 전용이며 Portrait를 허용하지 않습니다. 불필요한 Camera, Microphone, Location 권한이나 capability는 추가하지 않습니다. 임시 Bundle Identifier는 `com.independent.herodefense`입니다. Apple Developer 계정에서 사용 가능한 고유 identifier로 추후 변경할 수 있습니다.

## 실제 iPhone에서 실행

1. Unity Hub에서 현재 Unity 버전의 iOS Build Support 설치 여부를 확인합니다.
2. `chmod +x build-ios.sh` 후 `./build-ios.sh`를 실행합니다.
3. `Builds/iOS/HeroDefenseXcode`의 `Unity-iPhone.xcodeproj` 또는 생성된 workspace를 Xcode로 엽니다.
4. Xcode의 **Signing & Capabilities**에서 자신의 Apple Team을 선택합니다.
5. Bundle Identifier가 자신의 계정에서 고유한지 확인하고 필요하면 변경합니다.
6. MacBook에 iPhone을 연결하고 기기의 신뢰 요청을 승인합니다.
7. iPhone에서 요구되는 경우 **설정 > 개인정보 보호 및 보안 > 개발자 모드**를 활성화합니다.
8. Xcode 상단 실행 대상을 연결된 iPhone으로 선택합니다.
9. Run 버튼으로 빌드하고 설치합니다.

Apple 계정, 인증서 또는 provisioning profile이 없으면 실제 기기 설치가 불가능할 수 있습니다. Phase 2에서는 IPA archive, App Store 제출 및 TestFlight 배포를 요구하지 않으며 Team ID나 인증서 이름을 임의로 설정하지 않습니다.

## iOS Simulator

`Tools > Hero Defense > Build > iOS Simulator Project`는 Device SDK와 분리된 Simulator SDK 설정으로 다음 경로를 생성합니다.

```text
Builds/iOSSimulator/HeroDefenseXcode
```

Simulator 지원 여부는 설치된 Unity 6 LTS 패치와 iOS Build Support에 따라 달라질 수 있습니다. 실패해도 실제 기기용 Xcode 프로젝트 설정에는 영향을 주지 않으며, 기기용 프로젝트를 우선 사용하십시오.

## 테스트

`Window > General > Test Runner`에서 EditMode와 PlayMode 각각 **Run All**을 실행합니다. Phase 6 EditMode는 피해 공식, 관통, 치명타, 회피, 보호막, 모든 상태 규칙, 조준 보정과 통계를 검증합니다. PlayMode는 실제 Battle 씬에서 방어력, 기사 제어기·보호막, 레인저/마법사 수동 조준, 침묵, 일시정지, 신규 적과 초기화를 검증합니다. 결과는 `Builds/Logs/phase6-editmode.xml`과 `phase6-playmode.xml`에 저장됩니다.

- 런타임 데이터 기준: [Documentation/RUNTIME_CATALOG.md](Documentation/RUNTIME_CATALOG.md)
- Stage 1 측정과 변경 전후 결과: [Documentation/BALANCE_STAGE1.md](Documentation/BALANCE_STAGE1.md)
- 오디오 클립 연결 방법: [Documentation/AUDIO_INTEGRATION.md](Documentation/AUDIO_INTEGRATION.md)
- 자동/수동 QA 구분: [Documentation/QA_CHECKLIST.md](Documentation/QA_CHECKLIST.md)

## 주요 스크립트

- `GameBootstrap`, `SceneLoader`, `SceneNames`, `ApplicationQuitService`: 초기화, 장면 전환과 종료
- `SafeAreaController`, `UiFactory`, `MainMenuController`, `SettingsPanelController`: 반응형 UI
- `BattleSessionState`, `BattleHudController`: 이벤트 기반 상태와 HUD
- `BuildingSelectionModel`, `BuildingSelectionController`: 확장 가능한 건물 선택
- `PauseController`, `BackInputRouter`, `BattleSceneController`: 시간 정지, 입력, 전장 조립
- `Phase1Setup`: 장면과 Apple 플랫폼 기본 설정의 멱등 구성
- `UnitData`, `CombatTypes`, `HealthComponent`: 데이터 중심 능력치, 진영·피해 계약, 체력 원본
- `CombatRegistry`, `CombatUnit`, `AttackCooldown`: 활성 대상 탐색과 자동 이동·공격 상태
- `CombatPool`, `UnitVisualController`, `WorldHealthBar`, `FloatingDamageTextPool`: 재사용과 전투 피드백
- `PlayerBase`, `BattleCombatController`: 본진-HUD 연결, 테스트 소환, 보상, 초기화와 패배 흐름
- `Phase2Setup`: UnitData와 Phase 2 폴더를 기존 에셋 보존 방식으로 구성
- `BuildingData`, `BuildingRuntimeState`, `BuildSlotState`: 건물 원본 데이터와 인스턴스·슬롯 상태 분리
- `BuildingEconomyService`, `ProductionTimer`, `PlayerUnitLimitService`: 경제 처리, 결정적 타이머와 30명 제한
- `ProductionBuilding`, `BuildingSystemController`: 건설 연출, 자동 생산, 정보 패널, 업그레이드와 판매
- `ProjectilePool`: 궁수 화살과 마법사 마법탄의 풀링된 전투 피드백
- `Phase3Setup`: 궁수·마법사 및 건물 3종의 편집 가능한 데이터 생성과 검증
- `WaveData`, `StageData`, `WaveRuntimeState`: 웨이브/스테이지 원본 데이터와 결정적 상태 머신
- `WaveManager`, `EnemyCapacity`: 병렬 그룹 생성, 35마리 제한, 소유 적 추적, 보상과 승패
- `WaveHudView`: 카운트다운, 적 수, 안내, 보스 체력바와 결과 화면
- `BattleStatistics`: 처치, 생산, 건설, 판매, 업그레이드, 골드와 플레이 시간 통계
- `Phase4Setup`: 정예·보스, Wave 1~10과 Stage 1 데이터 생성 및 검증
- `HeroData`, `HeroSkillData`, `HeroPassiveData`: 영웅 능력치·스킬·패시브 원본 데이터
- `HeroSelectionService`, `HeroSelectController`: 선택 유지와 Safe Area 카드 UI
- `HeroController`, `HeroRuntimeState`, `HeroSkillExecutors`: 자동 전투, 상태, 쿨타임, 에너지, 스킬과 부활
- `HeroSpawnManager`, `HeroHudController`, `HeroEffectPool`: Battle 생성, 전용 HUD와 풀링 효과
- `Phase5Setup`: 세 영웅과 스킬·패시브·플레이스홀더 에셋 및 HeroSelect 씬의 멱등 구성
- `CombatStats`, `RuntimeCombatStats`: 원본과 분리된 능력치 및 출처별 수정자
- `DamageCalculationService`, `DamageResult`: 방어·관통·치명타·회피·보호막의 단일 계산 경로
- `StatusEffectData`, `StatusEffectController`, `ShieldController`: 통합 상태 타이머, 중첩과 보호막
- `SkillAimingController`: 원형·부채꼴 미리보기, 사거리 보정과 취소 입력
- `Phase6Setup`: 능력치 마이그레이션, 상태 데이터와 독·주술 고블린 생성 및 검증
- `BuildAutomation`: macOS Universal, iOS Device/Simulator 및 산출물 검증 빌드

## 알려진 제한 사항

- 단일 전선의 단순 Transform 이동이며 NavMesh, 복잡한 충돌 회피, 다중 라인과 카메라 이동은 없습니다.
- 정식 모델·스프라이트·Animator·사운드 대신 코드 기반 플레이스홀더 피드백을 사용합니다.
- 자유 배치 대신 모바일 안정성이 높은 8개 고정 슬롯을 사용합니다.
- 보스는 전용 스킬 없이 기존 자동 공격 구조를 사용합니다.
- 영웅은 수동 이동이나 위치 지정형 조준 없이 자동 전투하며, 스킬은 가장 밀집한 적 위치를 자동 선택합니다.
- 세라의 불씨는 테스트 재현성을 위해 확률 난수 대신 매 5번째 기본 공격에서 발동합니다.
- 정식 메시·파티클 대신 텍스트 상태 아이콘과 반투명 범위 미리보기를 사용합니다.
- 감전 연쇄 피해, 복잡한 보스 면역과 확률 기반 상태 저항은 후속 확장 범위입니다.
- 설정은 로컬 저장되며 앱 재실행 후 복구됩니다.
- 앱 아이콘을 제외한 전투 아트와 사운드는 플레이스홀더이고 실제 햅틱은 지원 모바일 기기에서만 검증할 수 있습니다.
- Apple 코드 서명, 공증, IPA archive와 스토어 배포는 자동화하지 않습니다.
- Unity Editor, 플랫폼 Build Support 또는 Xcode가 설치되지 않은 머신에서는 실제 결과물을 생성할 수 없습니다.

## Phase 7 예정

- 전투 경험치와 영웅 전투 레벨
- 레벨업 3지선다 강화
- 영웅 스킬·건물·유닛 강화
- 희귀 강화와 강화 중복·조합
- 전투 종료 시 초기화되는 로그라이크 성장
# Phase 7 — 전투 경험치와 로그라이크 강화

적 처치와 웨이브 클리어로 전투 경험치를 얻으며, 전투 레벨은 1에서 시작해 최대 20까지 상승합니다. 레벨업 시 전투가 안전하게 정지되고 중복 없는 강화 카드 3개가 표시됩니다. 카드 하나를 선택하면 즉시 적용되며, 한 전투당 무료 리롤 1회를 사용할 수 있습니다. 큰 경험치 보상으로 여러 레벨이 상승하면 남은 선택 횟수가 대기열로 처리됩니다.

Phase 7에는 Common, Rare, Epic, Legendary 희귀도와 Global, Hero, HeroSkill, HeroUltimate, Unit, Building, Economy, Base, Special 카테고리의 강화 41종이 포함됩니다. 기사·레인저·화염 마법사 전용 강화와 선행 조건도 지원합니다. 선택 내용은 한 판에만 유지되며 다시 플레이하거나 전투를 초기화하면 모두 제거됩니다. 이미 존재하는 아군과 이후 생산되는 아군 모두 현재 전투 Modifier를 적용받습니다.

경험치 요구량은 60, 80, 105, 135, 170, 210, 255, 305, 360, 420, 490, 565, 645, 730, 820, 920, 1030, 1150, 1300 XP입니다. 일반 적은 종류에 따라 8~18 XP, 정예는 35~45 XP, 고블린 대장은 200 XP를 지급하고 웨이브 클리어는 `웨이브 번호 × 10 XP`를 지급합니다.

에디터 데이터 생성은 `Tools/Hero Defense/Setup Phase 7`에서 실행합니다. 강화 에셋은 `Assets/ScriptableObjects/Progression` 아래에 생성됩니다. macOS 빌드는 기존 `./build-macos.sh`, iOS Xcode 프로젝트는 `./build-ios.sh`를 사용합니다. 테스트는 Unity Test Runner의 EditMode 및 PlayMode에서 `HeroDefense.Tests`를 실행합니다.

Phase 8 예정 항목은 종족·진영 확장, 신규 영웅/유닛/건물/몬스터/보스/스테이지, 환경 효과, 난이도 선택 및 무한 모드입니다.
# Phase 8 — 콘텐츠 확장, 스테이지 선택, 난이도, 끝없는 방어

게임 흐름은 `MainMenu → HeroSelect → StageSelect → Battle`입니다. 영웅 선택 화면에는 아르덴, 리안, 세라와 신규 영웅 카이(기계공), 엘리아(성녀), 녹스(암살자)가 표시됩니다. StageSelect에서 초원의 관문, 붉은 협곡, 얼어붙은 성채, 망자의 성역 중 하나와 쉬움/보통/어려움을 선택할 수 있으며 `끝없는 방어`를 선택하면 마지막 정적 웨이브 이후 런타임 생성 웨이브가 계속됩니다. 끝없는 방어는 5웨이브마다 정예, 10웨이브마다 순환 보스를 구성하고 웨이브에 따라 체력과 공격력이 증가합니다.

생산 건물은 병영, 사격장, 마법소, 수호 훈련소, 공성 작업장, 성소의 6종입니다. 신규 생산 유닛은 방패병, 대포병, 사제입니다. 런타임 콘텐츠는 `GameContentDatabase`가 ID별로 캐시하므로 전투 중 전체 에셋 검색이나 `Resources.LoadAll`을 사용하지 않습니다. `BattleLaunchConfig`는 선택 영웅 외의 스테이지/난이도/모드를 세션 동안 유지하며 재시작 시 원본 ScriptableObject를 변경하지 않습니다.

신규 적 데이터에는 돌격 멧돼지, 갑옷 오크, 해골 궁수, 흡혈 박쥐, 얼음 정령, 폭탄 고블린, 정예 갑옷 오크, 정예 얼음 정령, 정예 해골 기사와 오크 전쟁군주, 서리 여왕, 사령 기사 보스가 포함됩니다. 현재 Phase 8 플레이스홀더에서는 공통 이동·공격·피해·풀링 동작을 사용하며 일부 고유 AI/스킬 연출은 데이터 슬롯만 준비된 제한 사항입니다.

에디터에서 `Tools/Hero Defense/Setup Phase 8`을 실행하면 StageSelect와 Build Settings를 멱등적으로 구성합니다. `Tools/Hero Defense/Validate All Content`는 콘텐츠 개수, 중복 ID, 영웅 스킬 참조, 건물 생산 유닛, 스테이지/보스 웨이브, 필수 씬을 검사합니다.

권장 이미지 크기: 영웅 전신 1024×1536 PNG, 초상화 512×512, 스킬·유닛·몬스터 아이콘 256×256, 스테이지 대표 이미지 1280×720. 정식 이미지는 각 데이터의 Sprite 참조를 교체하는 방식으로 추가합니다.

Phase 9 예정: 로컬 저장, 스테이지/무한 기록, 영웅·스테이지 해금, 영구 재화와 성장, 도감, 업적, 튜토리얼, 설정 저장, 오디오와 진동.
# Phase 9 — 로컬 저장과 메타 진행

Phase 9 저장은 `Application.persistentDataPath` 아래 `hero_defense_save.json`에 JSON으로 기록됩니다. 쓰기는 먼저 `hero_defense_save.temp.json`에 수행하고, 이전 정상 파일은 `hero_defense_save.backup.json`으로 보존합니다. 본 파일이 손상되면 백업 로드를 시도하며, 둘 다 사용할 수 없으면 Save Version 1 기본 데이터를 생성해 앱 실행을 계속합니다. macOS와 iOS의 실제 persistent data 경로는 운영체제와 Bundle Identifier에 따라 다릅니다.

저장 항목에는 Commander 프로필, Coin, Soul Gem, 영웅 해금·숙련도, 스테이지 난이도별 클리어, 무한 최고 기록, 19종 지휘관 연구, 도감 발견 기록, 28종 업적, 튜토리얼 상태, 볼륨·진동·그래픽·프레임·언어 설정과 평생 통계가 포함됩니다. 콘텐츠 진행은 표시 이름이 아닌 안정적인 콘텐츠 ID를 사용합니다.

기본 영웅 아르덴·리안·세라는 해금 상태입니다. 카이, 엘리아, 녹스는 지정 스테이지 보통 클리어와 Coin/Soul Gem 조건을 충족한 후 HeroSelect 카드에서 해금할 수 있습니다. Stage 2~4는 이전 스테이지 보통 클리어로 순차 해금되고, 어려움과 끝없는 방어도 보통 클리어 조건을 검사합니다.

메인 메뉴의 `연구`, `도감`, `업적`에서 메타 진행을 확인합니다. 연구 비용은 `기본 비용 × (현재 레벨 + 1)^1.5`를 올림한 값이며 ScriptableObject 원본 대신 저장된 레벨과 Runtime Modifier를 사용합니다. 데이터 초기화는 메인 메뉴에서 2단계 확인 후 실행되며 main/backup/temp 파일을 삭제하고 기본 저장을 다시 만듭니다.

개발 메뉴: `Tools/Hero Defense/Save/Open Save Folder`, `Create New Save`, `Delete Save`, `Validate Save`, `Unlock All Content`, `Add Currency`. `Tools/Hero Defense/Setup Phase 9`은 필요한 폴더와 Phase 8 콘텐츠를 멱등적으로 검증합니다.

외부 저작권 음원은 포함하지 않았으므로 기본 오디오 큐의 클립은 비어 있습니다. 런타임에는 씬별 음악 크로스페이드, 동시 재생 제한 SFX 풀, Master/Music/SFX 볼륨, 플랫폼 안전 햅틱 구조가 연결되어 있어 자체 제작 클립을 `AudioCueData`에 지정하면 바로 재생됩니다. 튜토리얼은 대상 버튼 강조와 입력 제한·건너뛰기·재시작을 지원하며, 도감 상세 목록과 업적 완료 알림 큐도 저장 데이터에 연동됩니다.

## Phase 10 — 출시 후보 준비

Phase 10은 `Development`, `InternalTest`, `ReleaseCandidate`, `StoreRelease` 환경을 분리합니다. 현재 버전은 `1.0.0`이며 iOS Build Number와 Android Version Code는 `BuildEnvironment`의 양수 빌드 번호를 사용합니다. 메인 메뉴에는 `v1.0.0 (Build N)`이 표시되고 RC/Store 빌드에서는 개발 소환 UI, 치트 키와 성능 HUD가 비활성화됩니다.

개발 빌드의 성능 HUD는 FPS, frame time, 아군·적·상태 효과와 관리 메모리를 1초 단위로 표시합니다. 저품질 설정은 동시 피해 텍스트와 권장 전투 개체 제한을 낮추며, 30/60 FPS 설정은 즉시 적용됩니다. Profiler에서는 MainMenu 대기, 스테이지 전환, 10웨이브, 보스, 무한 Wave 20+, 재시작과 Scene 왕복을 CPU/Rendering/Memory/GC/UI/Audio 모듈로 기록하십시오. macOS 수치는 실제 모바일 발열·배터리·메모리 성능을 대체하지 않습니다.

접근성 설정에는 화면 흔들림, 진동, 피해 숫자, 체력바, 큰 UI, 고대비, 색상 외 상태 텍스트와 스킬 조준 보조가 포함됩니다. 큰 UI는 주요 텍스트와 버튼 최소 높이를 확대하고 저장합니다. 상태이상·진영·잠금·선택 정보는 텍스트 라벨과 형태를 함께 사용합니다.

출시 검증은 `Tools/Hero Defense/Setup Phase 10` 후 `Tools/Hero Defense/Release/Validate Release`에서 실행합니다. 콘텐츠 ID, Build Scene 순서, 저장 버전, 환경, 앱 아이콘, 문서, 방향, 패키지 ID와 민감한 서명 파일을 검사하며 Blocker가 있으면 RC 모바일 빌드를 중단합니다. 테스트는 `chmod +x run-*-tests.sh` 후 `./run-all-tests.sh`로 실행하며 XML은 `Builds/TestResults`, 로그는 `Builds/Logs`에 기록됩니다.

### 빌드와 서명

- macOS RC: `Tools/Hero Defense/Build/Release Candidate macOS`
- iOS: `iOS Development`, `iOS Release Candidate`, `iOS Store Release` 메뉴. Xcode에서 Signing & Capabilities의 Apple Team을 선택하고 연결 기기 실행 후 Product → Archive, Validate App, Distribute App으로 TestFlight에 업로드합니다. Team ID나 인증서는 프로젝트에 저장하지 않습니다.
- Android APK: `Tools/Hero Defense/Build/Android APK`. Unity Hub의 Android Build Support, SDK, NDK, OpenJDK가 필요합니다.
- Android AAB: 사용자 Keystore와 Alias를 로컬 Player Settings에 연결한 뒤 `Android Release AAB`을 사용합니다. Keystore와 비밀번호는 커밋하지 말고 암호화된 오프라인 백업을 보관하십시오. Keystore를 분실하면 동일 앱 업데이트가 불가능할 수 있습니다.

앱 아이콘은 [HeroDefenseIcon.png](Assets/Art/Branding/AppIcons/HeroDefenseIcon.png)에 연결되어 있습니다. Splash는 짙은 단색 배경과 Boot의 실제 초기화 흐름을 사용합니다. 게임명 `Hero Defense`의 상표·스토어 고유성, 스토어 스크린샷 실제 해상도, Apple/Google의 최소 OS·Target API·개인정보 정책은 제출 직전에 공식 문서로 다시 확인해야 합니다.

QA, 출시, 알려진 문제, 스토어 설명, 개인정보, 이용약관과 라이선스 초안은 [Documentation](Documentation) 폴더에 있습니다. 개인정보 문서와 이용약관은 법률 자문이 아니며 실제 문의 이메일과 공개 URL을 사용자가 제공해야 합니다.
