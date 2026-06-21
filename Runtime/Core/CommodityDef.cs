namespace AchEconomy
{
    /// <summary>
    /// 상품의 정적 정의(기준가·적정재고). 시장마다 공유되는 불변 설정이며,
    /// 가변 상태(현재재고·가격 배수)는 <see cref="CommodityState"/>가 따로 들고 있습니다.
    /// </summary>
    internal sealed class CommodityDef
    {
        public readonly string Id;

        /// <summary>수급이 적정재고와 정확히 균형일 때의 가격.</summary>
        public readonly float BasePrice;

        /// <summary>가격이 기준가가 되는 균형 재고 수준.</summary>
        public readonly float TargetStock;

        /// <summary>인구·수요로 인한 기본 수요량(가격 공식의 demand 성분).</summary>
        public readonly float BaseDemand;

        public CommodityDef(string id, float basePrice, float targetStock, float baseDemand)
        {
            Id = id;
            BasePrice = basePrice;
            TargetStock = targetStock;
            BaseDemand = baseDemand;
        }
    }
}
