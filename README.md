# Ach Economy

공급-수요 기반 동적 가격과 source/sink 인플레이션 관리를 한 번에 처리하는 재사용 가능한 Unity 경제 시뮬레이션 패키지입니다.

`EconomyConfig` 하나만 넘기면 `Economy` 클래스 안에서 **가격 결정 · 거점 간 무역 · 생산-소비 · 인플레이션 계측**이 자동으로 돌아갑니다. ScriptableObject도, 씬에 붙이는 컴포넌트도 필요 없습니다.

## 설계 한눈에

동적 경제는 성격이 다른 두 층위로 나뉩니다.

- **미시 (가격)**: 단일 시장의 공급/수요로 가격·재고가 움직임 — 무역·상점 컨텐츠.
- **거시 (인플레이션)**: 전체 경제에 재화가 얼마나 들어오고(source/faucet) 빠지는가(sink/drain) — 라이브 서비스의 핵심.

Ach Economy는 이 둘을 한 `Economy` 파사드 뒤로 숨기고, 사용자에게는 **`EconomyConfig` struct + `Economy` class** 두 가지만 노출합니다.

## 설치

### Git URL

```text
https://github.com/achieveonepark/ach-economy.git#1.0.0
```

### OpenUPM

```bash
openupm add com.achieve.ach-economy
```

## 빠른 시작

```csharp
using AchEconomy;

var economy = new Economy(EconomyConfig.Default);

economy.AddCommodity("wheat", basePrice: 10f, targetStock: 100f);
economy.AddMarket("town_a", new() { ["wheat"] = 200f });

// 거래 — 재고가 줄면 가격이 오른다 (인플레이션 중립)
var r = economy.Buy("town_a", "wheat", 10);
float price = economy.GetPrice("town_a", "wheat");

// source/sink — 재화의 생성·소멸을 알려주면 인플레이션이 자동 보정된다
economy.Inject("wheat", 100f, tag: "quest_reward"); // faucet
economy.Consume("wheat", 30f, tag: "crafting");     // sink

// 매 프레임 시간 진행
void Update() => economy.Tick(Time.deltaTime);
```

전체 예제는 `Samples~/Basic Usage`를 참고하세요.

## 핵심 개념

| 항목 | 설명 |
|---|---|
| **단기/장기 가격 분리** | 단기가는 거래 즉시, 장기가는 틱마다 대칭 관성으로 수렴 |
| **대칭 관성** | 오를 때·내릴 때 같은 속도 → 대량매도 후 익일 매수 차익거래 익스플로잇 차단 |
| **거점 간 무역** | 무역로로 연결된 노드 간 재고가 평준화(재정거래 자동 모사) |
| **생산-소비 루프** | 레시피가 입력을 출력으로 바꿔 수급 수치를 채움 |
| **source/sink 계측** | `Inject`/`Consume`로 faucet·drain을 기록해 인플레이션 압력 산출 |

## 문서

전체 API와 가이드는 문서 사이트를 참고하세요. (`docs~/` — fumadocs)

## 라이선스

MIT
