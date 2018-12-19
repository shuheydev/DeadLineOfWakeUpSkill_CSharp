using System;
using System.Collections.Generic;
using System.Text;

namespace DeadLineOfWakeUpSkill_CSharp
{
    static class MessageComposer
    {
        public static string ReportSetting(DBHelper dbHelper)
        {
            var speechText = "";

            var wakeTimeWorkdayDateTimeOffset = dbHelper.wakeTimeWorkdayDateTimeOffset;
            var wakeTimeHolidayDateTimeOffset = dbHelper.wakeTimeHolidayDateTimeOffset;

            var subText = "";
            if (!string.IsNullOrEmpty(dbHelper.wakeTimeWorkdayString))
            {
                subText += $"平日の時刻は{wakeTimeWorkdayDateTimeOffset:HH:mm}、";
            }

            if (!string.IsNullOrEmpty(dbHelper.wakeTimeHolidayString))
            {
                subText += $"休日の時刻は{wakeTimeHolidayDateTimeOffset:HH:mm}、";
            }

            if (!string.IsNullOrEmpty(subText))
            {
                speechText += $"現在設定されている{subText}です。";
            }
            else
            {
                speechText += "設定した起床時刻まであとどれくらい時間があるかを知らせます。" +
                    "現在、設定されている時刻はありません。" +
                    "まずは時刻を設定しましょう。" +
                    "平日と休日で、それぞれ設定できます。" +
                    "例えば、平日の10:50に設定して、と言ってください。";
            }

            return speechText;
        }

        public static string ReportTimeHasSet(string dayTypeInSlot,DateTimeOffset wakeTimeInSlotDateTimeOffset)
        {
            var speechText = "";

            speechText += $"{dayTypeInSlot}の起床時刻を{wakeTimeInSlotDateTimeOffset:HH:mm}に設定しました。";

            return speechText;
        }

        public static string ReportDiffTime(DBHelper dbHelper,DateTimeOffset nowJst)
        {
            var speechText = "";

            //設定時刻をDateTimeOffset型に変換する。
            //var wakeTimeDateTimeOffset = DateTimeOffset.Parse(wakeTimeString); //その日の日付の日時になる。日本時間にする。
            var wakeTimeDateTimeOffset = dbHelper.GetTodaysWakeTime(nowJst);

            var diffTime = wakeTimeDateTimeOffset - nowJst;
            if (diffTime.TotalSeconds > 0)
            {
                speechText += $"起床リミットまであと、{diffTime.Hours}時間{diffTime.Minutes}分{diffTime.Seconds}秒です。";
            }
            else
            {
                speechText += $"起床リミットは、{CreateWakeUpTime(diffTime)}前に過ぎました。";

                var nextDay = nowJst.AddDays(1);

                var nextWakeTimeDateTimeOffset = dbHelper.GetNextdaysDiffTimeToday(nextDay);


                var nextDiffTime = nextWakeTimeDateTimeOffset - nowJst;

                speechText += $"次の起床リミットまであと、{CreateWakeUpTime(nextDiffTime)}秒です。";
            }

            return speechText;
        }

        private static string CreateWakeUpTime(TimeSpan diffTime)
        {
            var diffTimeString = "";

            //時間
            if (diffTime.Hours != 0)
            {
                diffTimeString +=$"{Math.Abs(diffTime.Hours)}時間";
            }
            //分
            if (diffTime.Minutes != 0)
            {
                diffTimeString += $"{Math.Abs(diffTime.Minutes)}分";
            }
            //秒
            if (diffTime.Seconds != 0)
            {
                diffTimeString += $"{Math.Abs(diffTime.Seconds)}秒";
            }

            return diffTimeString;
        }


        public static string ReportNotSetWakeTime(DateTimeOffset nowJst)
        {
            var speechText = "";

            var todayOfWeek = nowJst.DayOfWeek;
            var dayTypeString = "";
            if (todayOfWeek == DayOfWeek.Sunday || todayOfWeek == DayOfWeek.Saturday)
            {
                dayTypeString = "休日";
            }
            else
            {
                dayTypeString = "平日";
            }

            speechText += $"{dayTypeString}に時刻が設定されていません。" +
                          "まずは時刻を設定してください。";

            return speechText;
        }



        public static string ReportGoodby()
        {
            string[] messages = { "さようなら","いつでも呼んでください。","失礼します","それではまた。"};

            var rand=new Random();
            var index=rand.Next(messages.Length);

            return messages[index];
        }

        public static string ReportAskAnythingElse()
        {
            string[] messages = { "他に何かありますか？", "他に御用はありますか？", "どうしますか？","他に聞きたいことはありますか？" };

            var rand = new Random();
            var index = rand.Next(messages.Length);

            return messages[index];
        }

        public static string ReportHelpMenu()
        {
            var speechText = "";

            speechText += "設定した起床時刻まであとどれくらい時間があるかを知らせます。" +
                          "このスキルは、好きな時刻を設定しておくと、" +
                          "その時刻まであとどれくらいあるかを知らせます。" +
                          "このヘルプでは、時刻の設定方法と、残り時間の確認方法と、現在の設定内容の確認方法を聞くことができます。" +
                          "どれを聞きたいですか？";

            return speechText;
        }

        internal static string ReportHelpAboutReportRemainTime()
        {
            var speechText = "";

            speechText += "残り時間を聞くには、例えば、" +
                "あと何分あるか教えて、と言ってみてください。";

            return speechText;
        }

        internal static string ReportHelpAboutReportSetting()
        {
            var speechText = "";

            speechText += "設定内容を確認するには、例えば、" +
                "設定内容を教えて、と言ってみてください。";

            return speechText;
        }

        internal static string ReportHelpAboutSetTime()
        {
            var speechText = "";

            speechText += "時刻を設定するには、例えば、" +
                "平日の午前7:00に設定して、と言ってみてください。";

            return speechText;
        }
    }
}
