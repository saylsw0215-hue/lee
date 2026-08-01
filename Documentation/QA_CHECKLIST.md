# Hero Defense QA Checklist

각 항목은 실제 수행 후 담당자, 기기/OS, 빌드 번호와 결과를 기록한다. Simulator/Editor 결과를 실기기 결과로 대체하지 않는다.

## 자동 검증 완료

- [x] Runtime Catalog가 Editor/Player 공통 경로와 동일 ID 객체 사용
- [x] Boot → MainMenu → HeroSelect → StageSelect → Battle PlayMode 스모크
- [x] Stage 1 보상 중복 방지와 Stage 2 해금
- [x] 패배 재시작 시 유닛·투사체·상태·보호막·결과 UI·Time.timeScale 초기화
- [x] MainMenu ↔ Battle 10회 왕복 후 영구 Manager/Canvas 중복 없음
- [x] 전투 초기화 5회 후 유닛·투사체·데미지 텍스트·영웅 효과 풀 누적 없음
- [x] 무한 모드 Wave 30 상태 머신 종료와 보스 주기
- [x] 화면 흔들림·피해 숫자 OFF, 오디오 null/중복 BGM/볼륨/Pause 처리
- [x] 저장 실패 시 게임 중단 없이 SystemMessage 표시

## 수동 검증 필요 — 설치와 실행

- [ ] 신규 설치 후 Boot → MainMenu
- [ ] 최초 튜토리얼 시작·스킵·재시작
- [ ] 앱 강제 종료 후 재실행과 저장 복구
- [ ] 비행기 모드에서 메뉴·전투·저장

## 수동 검증 필요 — 실제 플레이

- [x] MainMenu → HeroSelect → StageSelect → Battle (자동)
- [x] Stage 1 승리·패배·재시작 (자동 기준 전략 및 스모크)
- [ ] 건설·업그레이드·판매와 자동 생산
- [ ] 영웅 스킬·궁극기·부활
- [ ] 강화 3지선다와 무한 모드
- [ ] 영웅/스테이지 해금, 연구, 도감, 업적
- [ ] 결과 보상 중복 터치 방지
- [ ] 데이터 초기화 확인·취소

## 수동 검증 필요 — 화면과 입력

- [ ] 16:9, 18:9, 19.5:9, 20:9, 4:3
- [ ] iPhone 노치/Dynamic Island, iPad, Android 카메라 홀
- [ ] 빠른 연속 터치·멀티터치·가장자리 터치
- [ ] ScrollRect 드래그 중 버튼 오작동 없음
- [ ] Battle 뒤로가기 → Pause, 메뉴 뒤로가기
- [ ] 모든 중요 버튼 Safe Area 내부

## 수동 검증 필요 — 생명주기와 장시간

- [ ] 전투 중 백그라운드/복귀 10회
- [x] 스킬 조준 중 포커스 상실 시 취소 (자동)
- [x] Scene 왕복 10회 후 중복 매니저·오디오 없음 (자동)
- [ ] 일반 스테이지 10회 연속
- [x] Wave 30 가속 상태 머신 (자동; 실제 30분 플레이는 미검증)
- [ ] 30/60 FPS, 발열, 배터리, 저메모리 복귀 기록

## 수동 검증 필요 — 접근성·오디오

- [ ] 큰 UI, 상태 텍스트, 고대비, 조준 보조
- [x] 화면 흔들림·피해 숫자 OFF (자동; 실기기 진동은 미검증)
- [x] Master/Music/SFX 저장 및 채널 적용 (자동; 실제 클립 청취는 미검증)
- [ ] 이어폰·무음 모드·햅틱 과다 발생 점검
