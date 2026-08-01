# GitHub에서 Hero Defense 실행하기

Unity 네이티브 프로젝트는 GitHub 저장소 화면에서 직접 실행되지 않습니다. 포함된 GitHub Actions가 WebGL을 빌드하고 GitHub Pages에 배포하면 브라우저에서 실행할 수 있습니다.

## 최초 설정

1. GitHub에 빈 저장소를 만들고 이 프로젝트를 `main` 브랜치로 푸시합니다.
2. `Settings > Secrets and variables > Actions`에 `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`를 등록합니다.
3. `Settings > Pages > Build and deployment > Source`를 **GitHub Actions**로 선택합니다.
4. `Actions > Hero Defense Unity CI > Run workflow`에서 `webgl`을 선택합니다.
5. 완료 후 Actions 결과에 표시된 GitHub Pages 주소로 접속합니다.

Unity 라이선스 준비 방법은 [GameCI Activation](https://game.ci/docs/github/activation/)을 따릅니다. 라이선스와 비밀번호를 커밋하지 마십시오.

## 네이티브 빌드

`Run workflow`에서 `macos`, `ios`, 또는 `all`을 선택할 수 있습니다.

- `HeroDefense-macOS`: macOS 앱 아티팩트
- `HeroDefense-iOS-Xcode`: Xcode 프로젝트 아티팩트. iPhone 설치에는 별도 코드 서명이 필요합니다.

`Library`, `Temp`, `Logs`, `UserSettings`, `Builds`, 서명 인증서와 프로비저닝 파일은 저장소에서 제외됩니다. 다른 Mac에서 clone한 뒤 Unity `6000.5.6f1`로 열면 필요한 Library가 다시 생성됩니다.

