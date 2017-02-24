using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace MyPersianCalendar
{
    class Program
    {
        private static void Main(string[] args)
        {
            /*var weekNum = GetPersianWeekNumber(DateTime.Now);
            Console.WriteLine("Week: {0}",  weekNum);
            Console.ReadLine();*/
            var start = DateTime.Now;
            Console.WriteLine("Start");
            UpdateDate();
            var end = DateTime.Now;
            Console.WriteLine("end");
            var t = end.Subtract(start);
            Console.WriteLine("{0}", t);
            Console.ReadLine();
        }
        private static readonly PersianCalendar Calender = new PersianCalendar();

        private static void UpdateDate()
        {
            var startDat = new DateTime(1960, 01, 01);
            startDat = startDat.AddDays(-9);
            var endDat = new DateTime(2050, 01, 01);
            var weekNumberInMonth = 1;
            var connection = new SqlConnection(Properties.Settings.Default.ConStr);
            var command = new SqlCommand { Connection = connection };
            connection.Open();
            while (startDat <= endDat)
            {
                int persianYear;
                int persianManth;
                int persianDay;
                DayOfWeek dayOfWeek;
                string shamsiDate;
                string shamsiSeason;
                int shamsiSeasonNumber;
                string persianDayOfWeek;
                string persianMonthStr;
                string persianWeekStr;
                string persianWeekMonthStr;
                int shamsiNumberInWeek;
                var persianWeekNumber = InitializePersianDate(startDat, out persianYear, out persianManth, out persianDay, out dayOfWeek, out shamsiDate, ref weekNumberInMonth,
                    out shamsiSeason, out shamsiSeasonNumber, out persianDayOfWeek, out persianMonthStr, out persianWeekStr, out persianWeekMonthStr, out shamsiNumberInWeek);

                Insert(command, startDat, shamsiDate, persianYear, persianManth, persianDay, shamsiSeasonNumber, weekNumberInMonth, persianWeekNumber, persianMonthStr, persianWeekStr, persianWeekMonthStr, shamsiSeason, persianDayOfWeek, dayOfWeek, shamsiNumberInWeek);
                startDat = startDat.AddDays(1);
            }
            connection.Close();
        }

        private static int InitializePersianDate(DateTime startDat, out int persianYear, out int persianManth,
            out int persianDay, out DayOfWeek dayOfWeek, out string shamsiDate, ref int weekNumberInMonth,
            out string shamsiSeason, out int shamsiSeasonNumber, out string persianDayOfWeek, out string persianMonthStr, out string persianWeekStr, out string persianWeekMonthStr, out  int shamsiNumberInWeek)
        {
            var persianWeekNumber = GetPersianWeekNumber(startDat);
            persianYear = Calender.GetYear(startDat);
            persianManth = Calender.GetMonth(startDat);
            persianDay = Calender.GetDayOfMonth(startDat);
            dayOfWeek = startDat.DayOfWeek;
            shamsiDate = string.Format(@"{0:00}/{1:00}/{2:00}", persianYear, persianManth, persianDay);

            if (persianDay == 1)
                weekNumberInMonth = 1;
            shamsiSeason = string.Empty;
            var startWeek = StartOfWeek(startDat, DayOfWeek.Saturday);
            shamsiSeasonNumber = OnPersianSeason(persianManth, ref shamsiSeason);
            persianDayOfWeek = OnPersianDayOfWeek(dayOfWeek);
            persianMonthStr = OnPersianMonth(persianManth);
            persianWeekStr = OnPersianWeek(weekNumberInMonth);
            persianWeekMonthStr = string.Format("{0}؛{1} ماه", persianWeekStr, persianMonthStr);
            if (startDat.Year == startWeek.Year && startDat.Month == startWeek.Month && startDat.Day == startWeek.Day)
                weekNumberInMonth += 1;
            shamsiNumberInWeek = OnShamsiNumberInWeek(dayOfWeek);
            return persianWeekNumber;
        }

        private static void Insert(SqlCommand command, DateTime miladi, string shamsi, int shamsiYearNumber, int shamsiMonthNumber, int shamsiDayNumber, int shamsiSeasonNumber,
          int shamsiWeekNumberInMonth, int shamsiWeekNumberInYear, string shamsiMonth, string shamsiWeekAndMonth, string shamsiWeekNumberMonth, string shamsiSeason, string shamsiDayOfWeek, DayOfWeek miladiDayOfWeek, int shamsiNumberInWeek)
        {
            var commandText = string.Format("INSERT INTO MANG.TBL_DateConverter( miladi ,shamsi ,ShamsiYearNumber ,ShamsiMonthNumber ,ShamsiDayNumber,ShamsiSeasonNumber," +
                                            "ShamsiWeekNumberInMonth,ShamsiWeekNumberInYear,ShamsiMonth,ShamsiWeekAndMonth" +
                                            ",ShamsiWeekNumberMonth,ShamsiSeason,ShamsiDayOfWeek,MiladiDayOfWeek,ShamsiNumberInWeek) " +
                    "VALUES  ( '{0}', '{1}' ,{2} ,{3} , {4},{5},{6},{7},'{8}','{9}','{10}','{11}','{12}','{13}',{14})", miladi, shamsi, shamsiYearNumber, shamsiMonthNumber, shamsiDayNumber, shamsiSeasonNumber,
                    shamsiWeekNumberInMonth, shamsiWeekNumberInYear, shamsiMonth, shamsiWeekAndMonth, shamsiWeekNumberMonth, shamsiSeason, shamsiDayOfWeek, miladiDayOfWeek, shamsiNumberInWeek);
            command.CommandText = commandText;
            command.ExecuteNonQuery();
        }

        private static string OnPersianWeek(int persianweek)
        {
            var weekDictionary = new Dictionary<int, string>
            {
                {1, "هفته اول"},{2,"هفته دوم"},{3,"هفته سوم"},
                {4,"هفته چهارم"},{5,"هفته پنجم"},{6,"هفته ششم"}
            };
            string persianWeek;
            return weekDictionary.TryGetValue(persianweek, out persianWeek) ? persianWeek : string.Empty;//
        }
        private static int OnPersianSeason(int persianManth, ref string shamsiSeason)
        {
            int shamsiSeasonNumber;
            if (persianManth >= 1 && persianManth <= 3)
            {
                shamsiSeasonNumber = 1;
                shamsiSeason = "بهار";
            }
            else if (persianManth >= 4 && persianManth <= 6)
            {
                shamsiSeasonNumber = 2;
                shamsiSeason = "تابستان";
            }
            else if (persianManth >= 7 && persianManth <= 9)
            {
                shamsiSeasonNumber = 3;
                shamsiSeason = "پاییز";
            }
            else if (persianManth >= 10 && persianManth <= 12)
            {
                shamsiSeasonNumber = 4;
                shamsiSeason = "زمستان";
            }
            else
                shamsiSeasonNumber = 0;
            return shamsiSeasonNumber;
        }

        private static string OnPersianDayOfWeek(DayOfWeek dayOfWeek)
        {
            var persianDayOfWeek = string.Empty;
            switch (dayOfWeek)
            {
                case DayOfWeek.Friday:
                    persianDayOfWeek = "جمعه";
                    break;
                case DayOfWeek.Monday:
                    persianDayOfWeek = "دو شنبه";
                    break;
                case DayOfWeek.Saturday:
                    persianDayOfWeek = "شنبه";
                    break;
                case DayOfWeek.Sunday:
                    persianDayOfWeek = "یک شنبه";
                    break;
                case DayOfWeek.Thursday:
                    persianDayOfWeek = "پنج شنبه";
                    break;
                case DayOfWeek.Tuesday:
                    persianDayOfWeek = "سه شنبه";
                    break;
                case DayOfWeek.Wednesday:
                    persianDayOfWeek = "چهار شنبه";
                    break;
            }
            return persianDayOfWeek;
        }

        private static int OnShamsiNumberInWeek(DayOfWeek dayOfWeek)
        {
            int persianDayOfWeek = 0;
            switch (dayOfWeek)
            {
                case DayOfWeek.Friday:
                    persianDayOfWeek = 7;
                    break;
                case DayOfWeek.Monday:
                    persianDayOfWeek = 3;
                    break;
                case DayOfWeek.Saturday:
                    persianDayOfWeek = 1;
                    break;
                case DayOfWeek.Sunday:
                    persianDayOfWeek = 2;
                    break;
                case DayOfWeek.Thursday:
                    persianDayOfWeek = 6;
                    break;
                case DayOfWeek.Tuesday:
                    persianDayOfWeek = 4;
                    break;
                case DayOfWeek.Wednesday:
                    persianDayOfWeek = 5;
                    break;
            }
            return persianDayOfWeek;
        }
        private static string OnPersianMonth(int month)
        {
            string persianDayOfWeek;
            var monthDictionary = new Dictionary<int, string>
            {
                {1, "فروردین"},{2,"اردیبهشت"},{3,"خرداد"},
                {4,"تیر"},{5,"مرداد"},{6,"شهریور"},
                {7,"مهر"},{8,"آبان"},{9,"آذر"},
                {10,"دی"},{11,"بهمن"},{12,"اسفند"}
            };
            return monthDictionary.TryGetValue(month, out  persianDayOfWeek) ? persianDayOfWeek : string.Empty;
        }

        public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            var diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }
        private static int GetPersianWeekNumber(DateTime date)
        {
            var cul = CultureInfo.GetCultureInfo("fa-IR");
            var weekNum = cul.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Saturday);
            return weekNum;
        }
    }
}
