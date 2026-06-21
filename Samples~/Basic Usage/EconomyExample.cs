using AchEconomy;
using UnityEngine;

namespace AchEconomy.Samples
{
    /// <summary>
    /// Ach Economy 최소 사용 예제. 씬의 빈 GameObject에 붙이면 됩니다.
    /// EconomyConfig 하나로 경제를 만들고, 두 도시를 무역로로 잇고, 인플레이션까지 굴립니다.
    /// </summary>
    public sealed class EconomyExample : MonoBehaviour
    {
        Economy _economy;

        void Start()
        {
            // 1) 설정 — 기본값에서 필요한 것만 바꾼다.
            var config = EconomyConfig.Default;
            config.TickInterval = 2f;       // 2초마다 장기 틱
            config.PriceInertia = 0.2f;

            // 2) 생성 — 이게 전부.
            _economy = new Economy(config);

            // 3) 상품·시장 등록
            _economy.AddCommodity("wheat", basePrice: 10f, targetStock: 100f, baseDemand: 5f);
            _economy.AddCommodity("cloth", basePrice: 40f, targetStock: 30f);

            _economy.AddMarket("village", new() { ["wheat"] = 250f, ["cloth"] = 0f });
            _economy.AddMarket("town", new() { ["wheat"] = 20f, ["cloth"] = 40f });

            // 4) 생산 루프: 도시가 밀을 소비하고 직물을 만든다.
            _economy.AddRecipe("town",
                inputs: new[] { ("wheat", 3f) },
                outputs: new[] { ("cloth", 1f) },
                throughput: 2f);

            // 5) 무역로: 잉여(village)에서 부족(town)으로 밀이 흐른다.
            _economy.AddTradeRoute("village", "town");

            // 6) 이벤트 구독 (선택)
            _economy.OnInflationAlert += (commodity, pressure) =>
                Debug.Log($"[AchEconomy] {commodity} 인플레 압력 {pressure:P0}");

            // 7) 거래
            var r = _economy.Buy("town", "cloth", 5f);
            Debug.Log($"직물 5개 구매: 단가 {r.UnitPrice:F1}, 총액 {r.TotalCost:F1}, 잔고 {r.StockAfter}");

            // 8) source/sink: 퀘스트로 밀 100개를 무에서 지급 → faucet 압력
            _economy.Inject("wheat", 100f, tag: "daily_quest");
        }

        void Update()
        {
            // 수동 틱 — 결정적이고 테스트하기 쉽다.
            _economy.Tick(Time.deltaTime);
        }

        void OnDestroy()
        {
            _economy?.Dispose();
        }
    }
}
