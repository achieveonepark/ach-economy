using System.Collections.Generic;

namespace AchEconomy
{
    /// <summary>
    /// 한 시장(노드)의 상태. 상품별 <see cref="CommodityState"/>를 들고 있습니다.
    /// </summary>
    internal sealed class MarketState
    {
        public readonly string Id;
        public readonly Dictionary<string, CommodityState> Commodities = new Dictionary<string, CommodityState>();

        public MarketState(string id)
        {
            Id = id;
        }

        public CommodityState GetOrCreate(string commodityId, float initialStock = 0f)
        {
            if (!Commodities.TryGetValue(commodityId, out var state))
            {
                state = new CommodityState(initialStock);
                Commodities[commodityId] = state;
            }
            return state;
        }
    }
}
