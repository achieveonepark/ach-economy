using System.Collections.Generic;
using UnityEngine;

namespace AchEconomy
{
    /// <summary>
    /// 상품별 source(faucet, 생성)/sink(drain, 소멸) 유입·유출을 계측하고,
    /// 그로부터 인플레이션 압력과 전역 가격 배수를 산출하는 거시(D) 모듈.
    /// 미시 가격 모듈과 분리되어 있어, 라이브 서비스가 아닌 게임은 이 모듈을 무시해도 됩니다.
    /// </summary>
    internal sealed class SourceSinkTracker
    {
        readonly EconomyConfig _config;

        // 현재 틱 윈도 동안 누적된 생성/소멸량.
        readonly Dictionary<string, float> _faucet = new Dictionary<string, float>();
        readonly Dictionary<string, float> _sink = new Dictionary<string, float>();

        // 상품별 전역 인플레이션 배수(가격에 곱해짐). 1 = 중립.
        readonly Dictionary<string, float> _inflationMul = new Dictionary<string, float>();

        public SourceSinkTracker(EconomyConfig config)
        {
            _config = config;
        }

        public void RecordSource(string commodityId, float amount)
        {
            if (amount <= 0f) return;
            _faucet.TryGetValue(commodityId, out var v);
            _faucet[commodityId] = v + amount;
        }

        public void RecordSink(string commodityId, float amount)
        {
            if (amount <= 0f) return;
            _sink.TryGetValue(commodityId, out var v);
            _sink[commodityId] = v + amount;
        }

        public float GetInflationMultiplier(string commodityId)
        {
            return _inflationMul.TryGetValue(commodityId, out var m) ? m : 1f;
        }

        /// <summary>최근 윈도의 source/sink 비율. 1보다 크면 faucet 우세(공급 과잉), 작으면 sink 우세(희소).</summary>
        public float SinkFaucetRatio(string commodityId)
        {
            float faucet = _faucet.TryGetValue(commodityId, out var f) ? f : 0f;
            float sink = _sink.TryGetValue(commodityId, out var s) ? s : 0f;
            return (sink + 1f) / (faucet + 1f);
        }

        /// <summary>
        /// 장기 틱마다 호출. 누적 압력으로 인플레이션 배수를 갱신하고, 윈도를 비웁니다.
        /// 순 생성(faucet&gt;sink)은 자원을 흔하게 만들어 가격을 낮추고(배수↓),
        /// 순 소멸(sink&gt;faucet)은 희소하게 만들어 가격을 올립니다(배수↑).
        /// 임계값을 넘는 압력은 <paramref name="onAlert"/>로 통지합니다.
        /// </summary>
        public void UpdateAndDrain(IEnumerable<string> commodityIds, float referenceVolume,
            System.Action<string, float> onAlert)
        {
            foreach (var id in commodityIds)
            {
                float faucet = _faucet.TryGetValue(id, out var f) ? f : 0f;
                float sink = _sink.TryGetValue(id, out var s) ? s : 0f;

                // 순압력을 기준량으로 정규화. +면 공급 과잉, -면 희소.
                float pressure = (faucet - sink) / Mathf.Max(referenceVolume, 1f);

                float current = GetInflationMultiplier(id);
                // 공급 과잉이면 배수를 1 미만으로, 희소면 1 초과로 끌고 가되 민감도로 완만하게.
                float target = Mathf.Clamp(1f - pressure * _config.InflationSensitivity,
                    _config.MinPriceMultiplier, _config.MaxPriceMultiplier);
                _inflationMul[id] = Mathf.Lerp(current, target, Mathf.Clamp01(_config.PriceInertia));

                if (Mathf.Abs(pressure) >= _config.InflationAlertThreshold)
                    onAlert?.Invoke(id, pressure);
            }

            _faucet.Clear();
            _sink.Clear();
        }
    }
}
