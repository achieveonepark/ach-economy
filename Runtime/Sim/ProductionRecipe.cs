using System.Collections.Generic;

namespace AchEconomy
{
    /// <summary>
    /// 한 시장(노드)에서 장기 틱마다 입력 상품을 소비해 출력 상품을 생산하는 레시피.
    /// 출력이 비어 있으면 순수 소비(인구가 식량을 먹는 등)로 동작합니다 — C(생산-소비 루프).
    /// </summary>
    internal sealed class ProductionRecipe
    {
        public readonly string MarketId;
        public readonly IReadOnlyList<(string commodity, float amount)> Inputs;
        public readonly IReadOnlyList<(string commodity, float amount)> Outputs;

        /// <summary>틱당 최대 가동 횟수(입력 재고가 충분할 때).</summary>
        public readonly float Throughput;

        public ProductionRecipe(string marketId,
            IReadOnlyList<(string, float)> inputs,
            IReadOnlyList<(string, float)> outputs,
            float throughput)
        {
            MarketId = marketId;
            Inputs = inputs;
            Outputs = outputs;
            Throughput = throughput;
        }
    }
}
