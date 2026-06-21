namespace AchEconomy
{
    /// <summary>
    /// 경제 시뮬레이션의 모든 튜닝 값. 사용자가 건드리는 유일한 설정 타입입니다.
    /// <see cref="Economy"/> 생성자에 값으로 한 번 넘기면 끝이며, 이후 내부에서 복사되어 사용됩니다.
    /// 기본값이 필요하면 <see cref="Default"/>를 쓰고 필요한 필드만 덮어쓰세요.
    /// </summary>
    public struct EconomyConfig
    {
        /// <summary>장기 틱(생산·소비·장기가 수렴) 1회가 도는 시뮬레이션 시간(초).</summary>
        public float TickInterval;

        /// <summary>장기 가격이 목표치로 수렴하는 속도(0~1). 오를 때와 내릴 때가 동일한 <b>대칭 관성</b>이라 차익거래 익스플로잇이 생기지 않습니다.</summary>
        public float PriceInertia;

        /// <summary>가격 공식에서 수요 성분에 곱해지는 가중치. 클수록 수요가 가격을 강하게 끌어올립니다.</summary>
        public float DemandWeight;

        /// <summary>거래 한 건이 단기 가격을 목표치로 즉시 끌어당기는 강도(0~1).</summary>
        public float ShortTermResponse;

        /// <summary>단기 가격이 매 장기 틱마다 장기 가격 쪽으로 되돌아가는(평균 회귀) 강도(0~1).</summary>
        public float ShortTermDecay;

        /// <summary>source/sink 순압력이 인플레이션 배수를 움직이는 민감도. 0이면 인플레이션 보정을 끕니다.</summary>
        public float InflationSensitivity;

        /// <summary>인플레이션·수급에 의한 최종 가격 배수 하한.</summary>
        public float MinPriceMultiplier;

        /// <summary>인플레이션·수급에 의한 최종 가격 배수 상한.</summary>
        public float MaxPriceMultiplier;

        /// <summary>이 값을 넘는 절대 인플레이션 압력에서 <see cref="Economy.OnInflationAlert"/>가 발생합니다.</summary>
        public float InflationAlertThreshold;

        /// <summary>장기 틱마다 잉여 노드에서 부족 노드로 이동하는 재고 비율(0~1). 0이면 노드 간 무역을 끕니다.</summary>
        public float TradeFlowRate;

        /// <summary>안전한 기본값. 필요한 필드만 골라 덮어쓰세요.</summary>
        public static EconomyConfig Default => new EconomyConfig
        {
            TickInterval = 1f,
            PriceInertia = 0.15f,
            DemandWeight = 1f,
            ShortTermResponse = 0.5f,
            ShortTermDecay = 0.25f,
            InflationSensitivity = 0.1f,
            MinPriceMultiplier = 0.25f,
            MaxPriceMultiplier = 4f,
            InflationAlertThreshold = 0.2f,
            TradeFlowRate = 0.1f,
        };
    }
}
