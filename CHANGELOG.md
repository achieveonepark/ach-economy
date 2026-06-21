# Changelog

이 프로젝트의 주요 변경 사항을 기록합니다. 형식은 [Keep a Changelog](https://keepachangelog.com/ko/1.0.0/)를 따르며, 버전은 [Semantic Versioning](https://semver.org/lang/ko/)을 따릅니다.

## [1.0.0] - 2026-06-21

### Added
- `Economy` 파사드: 가격 결정·무역·생산·인플레이션을 한 진입점으로 통합
- `EconomyConfig` struct: 모든 튜닝 값을 값 타입 하나로 외부화
- 공급-수요 가격 결정 (`PriceCalculator`) — 단기/장기 분리, 대칭 관성
- 거점 간 무역 흐름 (`TradeFlowSim`) — 무역로 기반 재고 평준화
- 생산-소비 루프 (`ProductionSim`, `ProductionRecipe`)
- source/sink 인플레이션 계측 (`SourceSinkTracker`) — `Inject`/`Consume`, 인플레이션 배수·경보
- 선택적 자동 틱 러너 (`EconomyRunner`, `autoTick: true`)
- EditMode 테스트 (가격 공식 결정성·대칭 관성·인플레이션 압력)
- 최소 사용 예제 (`Samples~/Basic Usage`)
