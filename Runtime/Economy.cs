using System;
using System.Collections.Generic;
using UnityEngine;

namespace AchEconomy
{
    /// <summary>
    /// 동적 경제 시뮬레이션의 유일한 진입점(Facade).
    /// 사용자는 이 클래스와 <see cref="EconomyConfig"/> 두 가지만 다루면 되고,
    /// 가격 결정·무역·생산·인플레이션 계측은 전부 내부에서 자동으로 돌아갑니다.
    /// ScriptableObject도, 씬에 붙이는 컴포넌트도 필요 없습니다.
    /// </summary>
    /// <example>
    /// <code>
    /// var economy = new Economy(EconomyConfig.Default);
    /// economy.AddCommodity("wheat", basePrice: 10f, targetStock: 100f);
    /// economy.AddMarket("town_a", new() { ["wheat"] = 200f });
    /// var r = economy.Buy("town_a", "wheat", 10);
    /// void Update() =&gt; economy.Tick(Time.deltaTime);
    /// </code>
    /// </example>
    public sealed class Economy : IDisposable
    {
        readonly EconomyConfig _config;
        readonly PriceCalculator _price;
        readonly TradeFlowSim _trade;
        readonly ProductionSim _production;
        readonly SourceSinkTracker _tracker;

        readonly Dictionary<string, CommodityDef> _defs = new Dictionary<string, CommodityDef>();
        readonly Dictionary<string, MarketState> _markets = new Dictionary<string, MarketState>();

        readonly EconomyRunner _runner;
        float _tickAccumulator;

        // ── 이벤트 ─────────────────────────────────────────────
        /// <summary>(market, commodity, newPrice) — 단가가 갱신될 때.</summary>
        public event Action<string, string, float> OnPriceChanged;
        /// <summary>(market, commodity, newStock) — 재고가 바뀔 때.</summary>
        public event Action<string, string, float> OnStockChanged;
        /// <summary>거래가 체결될 때.</summary>
        public event Action<TradeResult> OnTradeExecuted;
        /// <summary>(commodity, pressure) — 인플레이션 압력이 임계값을 넘을 때. +면 공급 과잉, -면 희소.</summary>
        public event Action<string, float> OnInflationAlert;
        /// <summary>장기 틱이 한 번 돌 때마다.</summary>
        public event Action OnDailyTick;

        /// <param name="config">시뮬레이션 튜닝 값. 한 번 복사되어 보관됩니다.</param>
        /// <param name="autoTick">true면 숨은 러너가 매 프레임 <see cref="Tick"/>를 호출합니다. 보통은 false로 두고 직접 호출하세요.</param>
        public Economy(EconomyConfig config, bool autoTick = false)
        {
            _config = config;
            _price = new PriceCalculator(config);
            _trade = new TradeFlowSim(config);
            _production = new ProductionSim();
            _tracker = new SourceSinkTracker(config);

            if (autoTick)
                _runner = EconomyRunner.Attach(this);
        }

        // ── 등록 ───────────────────────────────────────────────

        /// <summary>상품 정의를 등록합니다. 기준가는 수급이 적정재고와 균형일 때의 가격입니다.</summary>
        public void AddCommodity(string id, float basePrice, float targetStock, float baseDemand = 0f)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("commodity id is empty");
            _defs[id] = new CommodityDef(id, basePrice, targetStock, baseDemand);
        }

        /// <summary>시장(노드)을 등록합니다. <paramref name="initialStock"/>로 초기 재고를 줄 수 있습니다.</summary>
        public void AddMarket(string id, Dictionary<string, float> initialStock = null)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("market id is empty");
            var market = new MarketState(id);
            if (initialStock != null)
                foreach (var kv in initialStock)
                    market.GetOrCreate(kv.Key, kv.Value);
            _markets[id] = market;
        }

        /// <summary>생산/소비 레시피를 등록합니다. outputs가 비면 순수 소비로 동작합니다.</summary>
        public void AddRecipe(string marketId,
            IReadOnlyList<(string commodity, float amount)> inputs,
            IReadOnlyList<(string commodity, float amount)> outputs,
            float throughput = 1f)
        {
            _production.AddRecipe(new ProductionRecipe(marketId,
                inputs ?? Array.Empty<(string, float)>(),
                outputs ?? Array.Empty<(string, float)>(),
                throughput));
        }

        /// <summary>두 노드를 무역로로 연결합니다. 연결된 쌍에서만 재고가 평준화되어 흐릅니다.</summary>
        public void AddTradeRoute(string marketA, string marketB) => _trade.AddRoute(marketA, marketB);

        // ── 거래 (인플레이션 중립: 재화의 이동) ─────────────────

        /// <summary>플레이어가 시장에서 상품을 사들입니다. 재고가 모자라면 부분 체결됩니다.</summary>
        public TradeResult Buy(string marketId, string commodityId, float quantity)
        {
            if (quantity <= 0f) return TradeResult.Fail(marketId, commodityId, "quantity must be > 0");
            if (!TryResolve(marketId, commodityId, out var market, out var def, out var state, out var reason))
                return TradeResult.Fail(marketId, commodityId, reason);

            float filled = Mathf.Min(quantity, state.CurrentStock);
            if (filled <= 0f) return TradeResult.Fail(marketId, commodityId, "out of stock");

            float unit = GetPrice(marketId, commodityId);
            state.CurrentStock -= filled;
            _price.OnTrade(def, state);
            return Commit(market, def, state, filled, unit);
        }

        /// <summary>플레이어가 시장에 상품을 내다 팝니다. 재고가 늘고 가격이 내려갑니다.</summary>
        public TradeResult Sell(string marketId, string commodityId, float quantity)
        {
            if (quantity <= 0f) return TradeResult.Fail(marketId, commodityId, "quantity must be > 0");
            if (!TryResolve(marketId, commodityId, out var market, out var def, out var state, out var reason))
                return TradeResult.Fail(marketId, commodityId, reason);

            float unit = GetPrice(marketId, commodityId);
            state.CurrentStock += quantity;
            _price.OnTrade(def, state);
            return Commit(market, def, state, quantity, unit);
        }

        TradeResult Commit(MarketState market, CommodityDef def, CommodityState state,
            float quantity, float unit)
        {
            var result = new TradeResult(true, market.Id, def.Id, quantity, unit,
                unit * quantity, state.CurrentStock, string.Empty);

            OnStockChanged?.Invoke(market.Id, def.Id, state.CurrentStock);
            OnPriceChanged?.Invoke(market.Id, def.Id, GetPrice(market.Id, def.Id));
            OnTradeExecuted?.Invoke(result);
            return result;
        }

        // ── source / sink (인플레이션을 만드는 재화의 생성·소멸) ─

        /// <summary>재화가 무에서 생성될 때 호출(퀘스트 보상·드롭 등). faucet 압력으로 기록됩니다.</summary>
        /// <param name="tag">선택. 어느 source가 인플레를 유발하는지 디버깅·텔레메트리용 라벨.</param>
        public void Inject(string commodityId, float amount, string tag = null)
        {
            _tracker.RecordSource(commodityId, amount);
        }

        /// <summary>재화가 영구히 소멸할 때 호출(제작 소모·수수료 등). sink 압력으로 기록됩니다.</summary>
        public void Consume(string commodityId, float amount, string tag = null)
        {
            _tracker.RecordSink(commodityId, amount);
        }

        // ── 조회 ───────────────────────────────────────────────

        /// <summary>해당 시장에서의 현재 단가(수급·인플레이션 배수 반영).</summary>
        public float GetPrice(string marketId, string commodityId)
        {
            if (!_markets.TryGetValue(marketId, out var market)) return 0f;
            if (!_defs.TryGetValue(commodityId, out var def)) return 0f;
            var state = market.GetOrCreate(commodityId);
            return _price.Price(def, state, _tracker.GetInflationMultiplier(commodityId));
        }

        /// <summary>해당 시장의 현재 재고.</summary>
        public float GetStock(string marketId, string commodityId)
        {
            if (!_markets.TryGetValue(marketId, out var market)) return 0f;
            return market.GetOrCreate(commodityId).CurrentStock;
        }

        /// <summary>상품의 전역 인플레이션 배수(1 = 중립, &lt;1 = 공급 과잉, &gt;1 = 희소).</summary>
        public float GetInflationMultiplier(string commodityId) => _tracker.GetInflationMultiplier(commodityId);

        /// <summary>상품의 sink/faucet 비율. 대시보드·밸런스 패치 근거로 쓰세요. 1보다 크면 sink 우세.</summary>
        public float GetSinkFaucetRatio(string commodityId) => _tracker.SinkFaucetRatio(commodityId);

        // ── 틱 ─────────────────────────────────────────────────

        /// <summary>
        /// 시뮬레이션 시간을 진행합니다. 누적 시간이 <see cref="EconomyConfig.TickInterval"/>에 도달할 때마다
        /// 장기 틱(생산→무역→장기가 수렴→인플레이션 갱신)을 한 번 돌립니다. autoTick=true면 자동 호출됩니다.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            _tickAccumulator += deltaTime;
            while (_tickAccumulator >= _config.TickInterval)
            {
                _tickAccumulator -= _config.TickInterval;
                LongTick();
            }
        }

        void LongTick()
        {
            // 1) 생산-소비로 재고 변동 → 2) 노드 간 무역으로 평준화
            _production.Step(_markets);
            _trade.Step(_markets, _defs);

            // 3) 모든 노드의 장기/단기 가격 배수를 수렴
            foreach (var market in _markets.Values)
                foreach (var kv in market.Commodities)
                    if (_defs.TryGetValue(kv.Key, out var def))
                        _price.OnLongTick(def, kv.Value);

            // 4) source/sink 압력으로 인플레이션 배수 갱신 (상품별 총재고를 기준량으로)
            _tracker.UpdateAndDrain(_defs.Keys, TotalReferenceVolume(), OnInflationAlert);

            OnDailyTick?.Invoke();
        }

        float TotalReferenceVolume()
        {
            // 기준량이 0이면 분모 폭주를 막기 위해 최소 1로 보정(트래커 내부에서도 한 번 더 클램프).
            float total = 0f;
            foreach (var market in _markets.Values)
                foreach (var state in market.Commodities.Values)
                    total += Mathf.Max(state.CurrentStock, 0f);
            return Mathf.Max(total, 1f);
        }

        // ── 내부 ───────────────────────────────────────────────

        bool TryResolve(string marketId, string commodityId,
            out MarketState market, out CommodityDef def, out CommodityState state, out string reason)
        {
            state = null;
            def = null;
            if (!_markets.TryGetValue(marketId, out market)) { reason = "unknown market"; return false; }
            if (!_defs.TryGetValue(commodityId, out def)) { reason = "unknown commodity"; return false; }
            state = market.GetOrCreate(commodityId);
            reason = string.Empty;
            return true;
        }

        /// <summary>autoTick 러너를 정리합니다. autoTick=false면 호출하지 않아도 됩니다.</summary>
        public void Dispose()
        {
            _runner?.Detach();
        }
    }
}
