# Known Issues

## Blocker

- 코드와 자동 테스트에서 확인된 항목 없음.

## Critical

- 코드와 자동 테스트에서 확인된 항목 없음.

## Major

- Stage 1의 자동 기준 전략은 아르덴·리안·세라 모두 승리하지만, 실제 신규 사용자 승률 55~70% 목표는 플레이테스트 표본이 없어 미검증.
- Xcode 전체 앱이 현재 활성 개발 경로가 아니어서 Archive/TestFlight 및 실제 iPhone 설치는 미검증.
- Android Build Support/SDK/NDK/JDK 및 사용자 Keystore 상태에 따라 APK/AAB 실빌드는 별도 검증 필요.
- 실제 모바일 기기의 FPS, 메모리 압박, 발열, 배터리와 햅틱은 미검증.

## Minor

- 정식 음악·효과음 클립이 없어 연결된 BGM/SFX 이벤트는 안전한 무음 fallback으로 동작.
- Stage 1의 10× 체력 프로필은 목표 플레이 시간에는 부합하지만 실기기 발열과 반복 플레이 피로도 검증이 필요.
- 한국어/영어 문자열 구조는 있으나 전체 전문 번역 감수는 필요.

## Cosmetic

- AI 제작 앱 아이콘과 Splash 표현은 출시 전 브랜드·법률 검수 권장.
