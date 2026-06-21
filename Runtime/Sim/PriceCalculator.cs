using UnityEngine;

namespace AchEconomy
{
    /// <summary>
    /// 공급-수요 가격 결정. 단기(거래 즉시)·장기(틱마다 대칭 관성) 두 성분으로 분리해 갱신합니다.
    /// 가격 = 기준가 × 단기배수 × 인플레이션배수.
    /// </summary>
    internal sealed class PriceCalculator
    {
        readonly EconomyConfig _config;

        public PriceCalculator(EconomyConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// 현재 재고/수요로부터 "균형 가격 배수"를 구합니다.
        /// 재고가 적정치보다 적으면 1보다 커지고(비싸짐), 많으면 1보다 작아집니다(싸짐).
        /// 형태: (적정재고 + 수요 + 1) / (현재재고 + 수요 + 1).
        /// </summary>
        public float TargetMultiplier(CommodityDef def, float currentStock)
        {
            float demand = def.BaseDemand * _config.DemandWeight;
            float numerator = def.TargetStock + demand + 1f;
            float denominator = Mathf.Max(currentStock, 0f) + demand + 1f;
            return numerator / denominator;
        }

        /// <summary>거래 직후 호출. 단기 배수를 균형치 쪽으로 즉시 끌어당깁니다.</summary>
        public void OnTrade(CommodityDef def, CommodityState state)
        {
            float target = TargetMultiplier(def, state.CurrentStock);
            state.ShortMultiplier = Mathf.Lerp(state.ShortMultiplier, target,
                Mathf.Clamp01(_config.ShortTermResponse));
        }

        /// <summary>
        /// 장기 틱마다 호출. 장기 배수는 균형치로 <b>대칭 관성</b>(오를 때/내릴 때 같은 속도)으로 수렴하고,
        /// 단기 배수는 장기 배수 쪽으로 평균 회귀합니다. 비대칭 관성이 만드는 대량매도→익일매수 차익거래를 막습니다.
        /// </summary>
        public void OnLongTick(CommodityDef def, CommodityState state)
        {
            float target = TargetMultiplier(def, state.CurrentStock);
            state.LongMultiplier = Mathf.Lerp(state.LongMultiplier, target,
                Mathf.Clamp01(_config.PriceInertia));
            state.ShortMultiplier = Mathf.Lerp(state.ShortMultiplier, state.LongMultiplier,
                Mathf.Clamp01(_config.ShortTermDecay));
        }

        /// <summary>최종 단가. 단기 수급 배수와 전역 인플레이션 배수를 곱하고 설정된 상·하한으로 자릅니다.</summary>
        public float Price(CommodityDef def, CommodityState state, float inflationMultiplier)
        {
            float combined = Mathf.Clamp(state.ShortMultiplier * inflationMultiplier,
                _config.MinPriceMultiplier, _config.MaxPriceMultiplier);
            return def.BasePrice * combined;
        }
    }
}
