using System;
using System.Collections.Generic;
using System.Text;
using AlexaPersistentAttributesManager;
using Amazon.Runtime;
using DeadLineOfWakeUpSkill_CSharp.ExtensionMethods;

namespace DeadLineOfWakeUpSkill_CSharp
{
    public class DBHelper
    {
        private string db_wakeTimeWorkday = "wakeTimeWorkday";
        private string db_wakeTimeHoliday = "wakeTimeHoliday";

        private AttributesManager _attrMgr;
        public string wakeTimeWorkdayString;
        public string wakeTimeHolidayString;
        public DateTimeOffset wakeTimeWorkdayDateTimeOffset;
        public DateTimeOffset wakeTimeHolidayDateTimeOffset;

        public DBHelper(AttributesManager attrMgr)
        {
            this._attrMgr = attrMgr;

            var attr = _attrMgr.GetPersistentAttributes();//nullが返る！

            wakeTimeWorkdayString = attr?.GetOrDefault(db_wakeTimeWorkday) ?? "";//12/16/18 5:00:00 PM +09:00
            wakeTimeHolidayString = attr?.GetOrDefault(db_wakeTimeHoliday) ?? "";

            DateTimeOffset.TryParse(wakeTimeWorkdayString, out wakeTimeWorkdayDateTimeOffset);
            DateTimeOffset.TryParse(wakeTimeHolidayString, out wakeTimeHolidayDateTimeOffset);
        }

        //public (DateTimeOffset, DateTimeOffset) GetWakeTimesDateTimeOffset()
        //{
        //    return (wakeTimeWorkdayDateTimeOffset, wakeTimeHolidayDateTimeOffset);
        //}

        public DateTimeOffset GetTodaysWakeTime(DateTimeOffset nowJst)
        {
            //今日の曜日を取得
            var nowDayOfWeek = nowJst.DayOfWeek;

            if (nowDayOfWeek == DayOfWeek.Sunday || nowDayOfWeek == DayOfWeek.Saturday)
            {
                return this.wakeTimeHolidayDateTimeOffset;
            }
            else
            {
                return this.wakeTimeWorkdayDateTimeOffset;
            }
        }

        public string GetTodaysWakeTimeString(DateTimeOffset nowJst)
        {
            //今日の曜日を取得
            var nowDayOfWeek = nowJst.DayOfWeek;

            if (nowDayOfWeek == DayOfWeek.Sunday || nowDayOfWeek == DayOfWeek.Saturday)
            {
                return this.wakeTimeHolidayString;
            }
            else
            {
                return this.wakeTimeWorkdayString;
            }
        }

        public DateTimeOffset GetNextdaysDiffTimeToday(DateTimeOffset nextJst)
        {
            //今日の曜日を取得
            var nowDayOfWeek = nextJst.DayOfWeek;

            if (nowDayOfWeek == DayOfWeek.Sunday || nowDayOfWeek == DayOfWeek.Saturday)
            {
                return this.wakeTimeHolidayDateTimeOffset.AddDays(1);
            }
            else
            {
                return this.wakeTimeWorkdayDateTimeOffset.AddDays(1);
            }
        }
    }
}
