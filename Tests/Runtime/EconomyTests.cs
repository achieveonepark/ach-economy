using System.Collections.Generic;
using NUnit.Framework;

namespace AchEconomy.Tests
{
    public class EconomyTests
    {
        static Economy MakeEconomy()
        {
            var economy = new Economy(EconomyConfig.Default);
            economy.AddCommodity("wheat", basePrice: 10f, targetStock: 100f);
            economy.AddMarket("town_a", new Dictionary<string, float> { ["wheat"] = 100f });
            return economy;
        }

        [Test]
        public void Buy_ReducesStock_AndReturnsSuccess()
        {
            var economy = MakeEconomy();
            var r = economy.Buy("town_a", "wheat", 10f);

            Assert.IsTrue(r.Success);
            Assert.AreEqual(10f, r.Quantity, 1e-4f);
            Assert.AreEqual(90f, economy.GetStock("town_a", "wheat"), 1e-4f);
        }

        [Test]
        public void Buy_BeyondStock_PartiallyFills()
        {
            var economy = MakeEconomy();
            var r = economy.Buy("town_a", "wheat", 999f);

            Assert.IsTrue(r.Success);
            Assert.AreEqual(100f, r.Quantity, 1e-4f);
            Assert.AreEqual(0f, economy.GetStock("town_a", "wheat"), 1e-4f);
        }

        [Test]
        public void Buy_RaisesPrice_Sell_LowersPrice()
        {
            var economy = MakeEconomy();
            float basePrice = economy.GetPrice("town_a", "wheat");

            economy.Buy("town_a", "wheat", 50f);
            Assert.Greater(economy.GetPrice("town_a", "wheat"), basePrice, "재고가 줄면 비싸져야 한다");

            var fresh = MakeEconomy();
            float p0 = fresh.GetPrice("town_a", "wheat");
            fresh.Sell("town_a", "wheat", 50f);
            Assert.Less(fresh.GetPrice("town_a", "wheat"), p0, "재고가 늘면 싸져야 한다");
        }

        [Test]
        public void Buy_UnknownMarket_FailsGracefully()
        {
            var economy = MakeEconomy();
            var r = economy.Buy("nowhere", "wheat", 1f);

            Assert.IsFalse(r.Success);
            Assert.AreEqual("unknown market", r.Reason);
        }

        [Test]
        public void Inject_NetFaucet_LowersInflationMultiplier_OverTick()
        {
            var economy = MakeEconomy();
            // 순 생성(faucet) → 공급 과잉 → 배수 1 미만으로 이동.
            economy.Inject("wheat", 500f, tag: "quest_reward");
            economy.Tick(EconomyConfig.Default.TickInterval);

            Assert.Less(economy.GetInflationMultiplier("wheat"), 1f);
        }

        [Test]
        public void Consume_NetSink_RaisesInflationMultiplier_OverTick()
        {
            var economy = MakeEconomy();
            // 순 소멸(sink) → 희소 → 배수 1 초과로 이동.
            economy.Consume("wheat", 500f, tag: "crafting");
            economy.Tick(EconomyConfig.Default.TickInterval);

            Assert.Greater(economy.GetInflationMultiplier("wheat"), 1f);
        }

        [Test]
        public void InflationAlert_FiresWhenPressureExceedsThreshold()
        {
            var economy = MakeEconomy();
            string alerted = null;
            economy.OnInflationAlert += (commodity, pressure) => alerted = commodity;

            economy.Inject("wheat", 10000f);
            economy.Tick(EconomyConfig.Default.TickInterval);

            Assert.AreEqual("wheat", alerted);
        }

        [Test]
        public void Tick_BelowInterval_DoesNotFireDailyTick()
        {
            var economy = MakeEconomy();
            int ticks = 0;
            economy.OnDailyTick += () => ticks++;

            economy.Tick(EconomyConfig.Default.TickInterval * 0.5f);
            Assert.AreEqual(0, ticks);

            economy.Tick(EconomyConfig.Default.TickInterval * 0.5f);
            Assert.AreEqual(1, ticks);
        }

        [Test]
        public void Production_ConvertsInputsToOutputs_OnTick()
        {
            var economy = new Economy(EconomyConfig.Default);
            economy.AddCommodity("wool", 5f, 100f);
            economy.AddCommodity("cloth", 20f, 50f);
            economy.AddMarket("town", new Dictionary<string, float> { ["wool"] = 10f, ["cloth"] = 0f });
            // 양모 2 → 직물 1, 틱당 3회 가동.
            economy.AddRecipe("town",
                inputs: new[] { ("wool", 2f) },
                outputs: new[] { ("cloth", 1f) },
                throughput: 3f);

            economy.Tick(EconomyConfig.Default.TickInterval);

            Assert.AreEqual(4f, economy.GetStock("town", "wool"), 1e-4f);   // 10 - 2*3
            Assert.AreEqual(3f, economy.GetStock("town", "cloth"), 1e-4f);  // 0 + 1*3
        }

        [Test]
        public void TradeFlow_EqualizesStockBetweenLinkedNodes()
        {
            var economy = new Economy(EconomyConfig.Default);
            economy.AddCommodity("grain", 10f, 100f);
            economy.AddMarket("surplus", new Dictionary<string, float> { ["grain"] = 300f });
            economy.AddMarket("deficit", new Dictionary<string, float> { ["grain"] = 0f });
            economy.AddTradeRoute("surplus", "deficit");

            float before = economy.GetStock("deficit", "grain");
            economy.Tick(EconomyConfig.Default.TickInterval);

            // 잉여 노드에서 부족 노드로 일부 이동.
            Assert.Greater(economy.GetStock("deficit", "grain"), before);
            Assert.Less(economy.GetStock("surplus", "grain"), 300f);
        }
    }
}
