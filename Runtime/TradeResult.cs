namespace AchEconomy
{
    /// <summary>
    /// <see cref="Economy.Buy"/> / <see cref="Economy.Sell"/> 호출 결과.
    /// 실패해도 예외를 던지지 않고 <see cref="Success"/>가 false인 결과를 돌려줍니다.
    /// </summary>
    public readonly struct TradeResult
    {
        /// <summary>거래 성사 여부. false면 재고/시장/상품 문제 등으로 거래가 일어나지 않았습니다.</summary>
        public readonly bool Success;

        /// <summary>거래가 일어난 시장(노드) id.</summary>
        public readonly string Market;

        /// <summary>거래 상품 id.</summary>
        public readonly string Commodity;

        /// <summary>실제 체결된 수량(부분 체결 시 요청보다 작을 수 있음).</summary>
        public readonly float Quantity;

        /// <summary>체결 시점의 단가(인플레이션·수급 배수 반영).</summary>
        public readonly float UnitPrice;

        /// <summary>총 거래 금액(<see cref="UnitPrice"/> × <see cref="Quantity"/>).</summary>
        public readonly float TotalCost;

        /// <summary>거래 후 해당 시장의 상품 재고.</summary>
        public readonly float StockAfter;

        /// <summary>실패 사유(성공 시 빈 문자열).</summary>
        public readonly string Reason;

        public TradeResult(bool success, string market, string commodity, float quantity,
            float unitPrice, float totalCost, float stockAfter, string reason)
        {
            Success = success;
            Market = market;
            Commodity = commodity;
            Quantity = quantity;
            UnitPrice = unitPrice;
            TotalCost = totalCost;
            StockAfter = stockAfter;
            Reason = reason;
        }

        internal static TradeResult Fail(string market, string commodity, string reason) =>
            new TradeResult(false, market, commodity, 0f, 0f, 0f, 0f, reason);
    }
}
