using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_AutoPurchase_TrackingOrders.Lib
{
    public static class XpathHelper
    {
        public static bool IsElementPresent(ChromeDriver driver, By by, out IWebElement element)
        {
            try
            {
                element = driver.FindElement(by);
                return true;
            }
            catch (Exception ex)
            {
                element = null;
                return false;
            }
        }
        public static bool IsElementPresent(IWebElement parentElement, By by, out IWebElement element)
        {
            try
            {
                element = parentElement.FindElement(by);
                return true;
            }
            catch (Exception ex)
            {
                element = null;
                return false;
            }
        }
        public static bool IsElementPresents(ChromeDriver driver, By by, out IReadOnlyCollection<IWebElement> element)
        {
            try
            {
                element = driver.FindElements(by);
                return true;
            }
            catch (Exception ex)
            {
                element = null;
                return false;
            }
        }
        public static DateTime? ParseDateTimeFromStringNoYear(string text)
        {
            // Text: Arriving tomorrow by 10 PM 
            // Text: Arriving May 1 - May 5
            // Text: Delivered Thursday
            // Text: Arriving Thursday
            // Text: Delivered Apr 20
            // Text: Arriving Apr 20
            // Text: Arriving by May 18
            //return DateTime.ParseExact("April 16, 2011", "MMMM d, yyyy", null);
            try
            {
                var parse_text = text;
                DateTime time = new DateTime(2000, 5, 15, 15, 30, 30);
                int year = DateTime.Now.Year, month = 1, day = 1, hour = 0, minutes = 0, second = 0;
                List<string> skip = new List<string>() {
                    "Arriving","Delivered"
                };
                string month_text = "January,February,March,April,May,June,July,August,September,October,November,December";
                string dayofweek_text = "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday";
                bool after_month = false,after_by=false;
                var model = parse_text.Split(" ");
                foreach(var value in model)
                {
                    // If is status, continues
                    if (skip.Contains(value))
                    {
                        continue;
                    }
                    //After
                    else if (after_by)
                    {
                        break;
                    }
                    // if today or tomorrow
                    else if (value.Contains("today"))
                    {
                        month = DateTime.Now.Month;
                        day = DateTime.Now.Day;
                    }
                    else if (value.Contains("tomorrow"))
                    {
                        month = DateTime.Today.AddDays(1).Month;
                        day = DateTime.Now.AddDays(1).Day;
                    }
                    // If month
                    else if (month_text.Split(",").Contains(value))
                    {
                        month = DateTime.ParseExact(value, "MMMM", CultureInfo.InvariantCulture).Month;
                        after_month = true;
                    }
                    //After month is day
                    else if (after_month && !after_by)
                    {
                        day = Convert.ToInt32(value);
                        after_month = false;
                    }
                    // "By": After is date or time:
                    else if (value.Contains("by"))
                    {
                        after_by = true;
                    }
                    // Day of Week: return
                    else if (dayofweek_text.Split(",").Contains(value))
                    {
                        DayOfWeek dayofweek = DayOfWeek.Monday;
                        switch (value)
                        {
                            case "Monday":
                                {
                                    dayofweek = DayOfWeek.Monday;
                                }
                                break;
                            case "Tuesday":
                                {
                                    dayofweek = DayOfWeek.Tuesday;
                                }
                                break;
                            case "Wednesday":
                                {
                                    dayofweek = DayOfWeek.Wednesday;
                                }
                                break;
                            case "Thursday":
                                {
                                    dayofweek = DayOfWeek.Thursday;
                                }
                                break;
                            case "Friday":
                                {
                                    dayofweek = DayOfWeek.Friday;
                                }
                                break;
                            case "Saturday":
                                {
                                    dayofweek = DayOfWeek.Saturday;
                                }
                                break;
                            case "Sunday":
                                {
                                    dayofweek = DayOfWeek.Sunday;
                                }
                                break;
                        }
                        var date= GetNextWeekday(DateTime.Now, dayofweek);
                        year = date.Year;
                        month = date.Month;
                        day = date.Day;
                    }
                }
                //-- After By: Date or time
                if (after_by)
                {
                    var text_2 = parse_text.Split("by");
                    //-- Time
                    if(text_2[1].Contains("AM")|| text_2[1].Contains("PM"))
                    {
                       hour= DateTime.ParseExact("09 AM", "hh tt", CultureInfo.InvariantCulture).Hour;
                    }
                    //-- Date:
                    else
                    {
                        try
                        {
                            day = DateTime.ParseExact(text_2[1], "MMMM dd", CultureInfo.InvariantCulture).Day;
                            month = DateTime.ParseExact(text_2[1], "MMMM dd", CultureInfo.InvariantCulture).Month;
                        }
                        catch { }

                    }
                }
                TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                var result= TimeZoneInfo.ConvertTimeFromUtc(new DateTime(year, month, day, hour, minutes, second), pacificZone);
                return result;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }
    }
}
