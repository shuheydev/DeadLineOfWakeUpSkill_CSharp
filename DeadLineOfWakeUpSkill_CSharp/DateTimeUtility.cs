using System;
using System.Collections.Generic;
using System.Text;

namespace DeadLineOfWakeUpSkill_CSharp
{
    static class DateTimeUtility
    {
        public static DateTimeOffset GetNowJst()
        {
            //現在時刻を取得（日本時間に変換する）
            //var jstTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            var nowUtc = DateTimeOffset.UtcNow;
            var nowJst = nowUtc.ToOffset(new TimeSpan(9, 0, 0));//TimeZonInfo.Convert...だとエラーになる。

            return nowJst;
        }
    }
}
