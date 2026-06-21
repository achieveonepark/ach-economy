using System.Collections.Generic;
using UnityEngine;

namespace AchEconomy
{
    /// <summary>
    /// 생산-소비 루프(C). 장기 틱마다 등록된 레시피를 가동해 입력 재고를 출력 재고로 바꿉니다.
    /// 이 재고 변화가 곧 <see cref="PriceCalculator"/>의 수급 수치를 채워주는 엔진입니다.
    /// </summary>
    internal sealed class ProductionSim
    {
        readonly List<ProductionRecipe> _recipes = new List<ProductionRecipe>();

        public void AddRecipe(ProductionRecipe recipe) => _recipes.Add(recipe);

        public void Step(Dictionary<string, MarketState> markets)
        {
            foreach (var recipe in _recipes)
            {
                if (!markets.TryGetValue(recipe.MarketId, out var market))
                    continue;

                // 입력 재고가 허용하는 만큼만 가동 (부분 가동 허용).
                float runs = recipe.Throughput;
                foreach (var (commodity, amount) in recipe.Inputs)
                {
                    if (amount <= 0f) continue;
                    float available = market.GetOrCreate(commodity).CurrentStock;
                    runs = Mathf.Min(runs, available / amount);
                }
                if (runs <= 0f) continue;

                foreach (var (commodity, amount) in recipe.Inputs)
                    market.GetOrCreate(commodity).CurrentStock -= amount * runs;

                foreach (var (commodity, amount) in recipe.Outputs)
                    market.GetOrCreate(commodity).CurrentStock += amount * runs;
            }
        }
    }
}
