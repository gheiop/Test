using Islebound.World;

namespace Islebound.Enemies
{
    public enum EnemyTimeRule
    {
        AnyTime,
        DayOnly,
        NightOnly
    }

    public static class EnemyTimeRuleUtility
    {
        public static bool IsAllowed(EnemyTimeRule rule)
        {
            if (WorldTimeManager.Instance == null)
                return true;

            bool isNight = WorldTimeManager.Instance.IsNight;

            return rule switch
            {
                EnemyTimeRule.DayOnly => !isNight,
                EnemyTimeRule.NightOnly => isNight,
                _ => true
            };
        }
    }
}