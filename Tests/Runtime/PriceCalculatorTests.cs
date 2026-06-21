using NUnit.Framework;

namespace AchEconomy.Tests
{
    public class PriceCalculatorTests
    {
        static EconomyConfig Config() => EconomyConfig.Default;

        [Test]
        public void TargetMultiplier_AtTargetStock_IsApproximatelyOne()
        {
            var calc = new PriceCalculator(Config());
            var def = new CommodityDef("wheat", basePrice: 10f, targetStock: 100f, baseDemand: 0f);

            // 재고가 적정치와 같으면 분자=분모 → 배수 1.
            Assert.AreEqual(1f, calc.TargetMultiplier(def, 100f), 1e-4f);
        }

        [Test]
        public void TargetMultiplier_LowStock_IsAboveOne()
        {
            var calc = new PriceCalculator(Config());
            var def = new CommodityDef("wheat", 10f, 100f, 0f);

            // 재고 부족 → 비싸짐.
            Assert.Greater(calc.TargetMultiplier(def, 10f), 1f);
        }

        [Test]
        public void TargetMultiplier_HighStock_IsBelowOne()
        {
            var calc = new PriceCalculator(Config());
            var def = new CommodityDef("wheat", 10f, 100f, 0f);

            // 재고 과잉 → 싸짐.
            Assert.Less(calc.TargetMultiplier(def, 1000f), 1f);
        }

        [Test]
        public void TargetMultiplier_IsDeterministic()
        {
            var calc = new PriceCalculator(Config());
            var def = new CommodityDef("wheat", 10f, 100f, 5f);

            // 같은 입력 → 항상 같은 출력 (락스텝/결정성 보장).
            float a = calc.TargetMultiplier(def, 42f);
            float b = calc.TargetMultiplier(def, 42f);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void LongTick_InertiaIsSymmetric_NoArbitrageExploit()
        {
            // 대칭 관성 검증: 같은 크기로 위/아래에서 출발하면 한 틱 뒤 목표까지의 잔여 격차 크기가 같아야 한다.
            var calc = new PriceCalculator(Config());
            var def = new CommodityDef("wheat", 10f, 100f, 0f);

            float target = calc.TargetMultiplier(def, 100f); // == 1

            var up = new CommodityState(100f) { ShortMultiplier = target + 0.4f, LongMultiplier = target + 0.4f };
            var down = new CommodityState(100f) { ShortMultiplier = target - 0.4f, LongMultiplier = target - 0.4f };

            calc.OnLongTick(def, up);
            calc.OnLongTick(def, down);

            float gapUp = up.LongMultiplier - target;
            float gapDown = target - down.LongMultiplier;

            // 오를 때와 내릴 때 수렴 속도가 동일 → 익일 차익거래 익스플로잇 차단.
            Assert.AreEqual(gapUp, gapDown, 1e-4f);
        }

        [Test]
        public void Price_ClampedByMultiplierBounds()
        {
            var config = Config();
            config.MaxPriceMultiplier = 2f;
            var calc = new PriceCalculator(config);
            var def = new CommodityDef("wheat", 10f, 100f, 0f);
            var state = new CommodityState(0f) { ShortMultiplier = 100f }; // 비현실적으로 큰 배수

            // 상한 2배 → 최대 20.
            Assert.AreEqual(20f, calc.Price(def, state, 1f), 1e-3f);
        }
    }
}
