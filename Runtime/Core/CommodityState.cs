namespace AchEconomy
{
    /// <summary>
    /// 한 시장(노드) 안에서 한 상품의 가변 상태. 단기/장기 가격 배수를 분리해서 들고 있습니다.
    /// </summary>
    internal sealed class CommodityState
    {
        public float CurrentStock;

        /// <summary>거래마다 즉시 반응하는 단기 가격 배수.</summary>
        public float ShortMultiplier = 1f;

        /// <summary>장기 틱마다 대칭 관성으로 천천히 수렴하는 장기 가격 배수.</summary>
        public float LongMultiplier = 1f;

        public CommodityState(float initialStock)
        {
            CurrentStock = initialStock;
        }
    }
}
