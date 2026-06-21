using System.Collections.Generic;
using UnityEngine;

namespace AchEconomy
{
    /// <summary>
    /// 거점 간 무역 흐름(B). 장기 틱마다 잉여 노드(재고 &gt; 적정)에서 부족 노드(재고 &lt; 적정)로
    /// 재고를 일부 이동시켜 AI 캐러밴의 재정거래를 모사합니다. 결과적으로 노드 간 가격이 평준화됩니다.
    /// 무역로(<see cref="_routes"/>)가 등록된 노드 쌍에 대해서만 흐릅니다 — 무역로를 끊으면 가격이 튑니다.
    /// </summary>
    internal sealed class TradeFlowSim
    {
        readonly EconomyConfig _config;
        readonly List<(string a, string b)> _routes = new List<(string, string)>();

        public TradeFlowSim(EconomyConfig config)
        {
            _config = config;
        }

        public void AddRoute(string marketA, string marketB) => _routes.Add((marketA, marketB));

        public void Step(Dictionary<string, MarketState> markets,
            IReadOnlyDictionary<string, CommodityDef> defs)
        {
            if (_config.TradeFlowRate <= 0f) return;

            foreach (var (a, b) in _routes)
            {
                if (!markets.TryGetValue(a, out var ma) || !markets.TryGetValue(b, out var mb))
                    continue;

                foreach (var kv in defs)
                {
                    string commodity = kv.Key;
                    float target = kv.Value.TargetStock;

                    var sa = ma.GetOrCreate(commodity);
                    var sb = mb.GetOrCreate(commodity);

                    // 적정재고 기준 잉여/부족을 비교해 잉여 → 부족 방향으로만 흐름.
                    float surplusA = sa.CurrentStock - target;
                    float surplusB = sb.CurrentStock - target;
                    float gap = surplusA - surplusB;
                    if (Mathf.Abs(gap) < Mathf.Epsilon) continue;

                    // 격차의 절반을 기준으로 흐름량을 잡고 비율만큼만 이동(과조정 방지).
                    float move = gap * 0.5f * Mathf.Clamp01(_config.TradeFlowRate);
                    sa.CurrentStock -= move;
                    sb.CurrentStock += move;
                }
            }
        }
    }
}
