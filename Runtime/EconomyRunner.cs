using UnityEngine;

namespace AchEconomy
{
    /// <summary>
    /// <see cref="Economy"/>를 autoTick=true로 만들 때만 생성되는 숨은 구동용 컴포넌트.
    /// 사용자가 직접 씬에 붙일 필요는 없습니다 — Economy가 알아서 생성·파괴합니다.
    /// </summary>
    [AddComponentMenu("")] // 컴포넌트 추가 메뉴에서 숨김
    internal sealed class EconomyRunner : MonoBehaviour
    {
        Economy _economy;

        public static EconomyRunner Attach(Economy economy)
        {
            var go = new GameObject("[AchEconomy.EconomyRunner]")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            Object.DontDestroyOnLoad(go);
            var runner = go.AddComponent<EconomyRunner>();
            runner._economy = economy;
            return runner;
        }

        void Update()
        {
            _economy?.Tick(Time.deltaTime);
        }

        public void Detach()
        {
            if (this != null)
                Destroy(gameObject);
        }
    }
}
