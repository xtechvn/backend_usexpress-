using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_AutomaticPurchase_AMZ.Lib
{
    public static class ImageHelper
    {
        public static string TakeScreenshot(ChromeDriver driver, string step_name ,string order_code, string label_id="amazon",string screenshot_folder_path=null)
        {
            if (screenshot_folder_path == null)
            {
                screenshot_folder_path = Directory.GetCurrentDirectory();
            }
            if (!Directory.Exists(screenshot_folder_path + @"\screenshot"))
            {
                Directory.CreateDirectory(screenshot_folder_path + @"\screenshot");
            }

            if (!Directory.Exists(screenshot_folder_path + @"\screenshot\" + label_id.Trim()))
            {
                Directory.CreateDirectory(screenshot_folder_path + @"\screenshot\" + label_id.Trim());
            }
            if (!Directory.Exists(screenshot_folder_path + @"\screenshot\" + label_id.Trim() + @"\" + order_code.ToUpper()))
            {
                Directory.CreateDirectory(screenshot_folder_path + @"\screenshot\" + label_id.Trim() + @"\" + order_code.ToUpper());
            }
            string file_path = screenshot_folder_path + @"\screenshot\" + label_id.Trim() + @"\" + order_code.ToUpper() + @"\" + step_name.Trim().Replace(" ", "_") + ".png";
            ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(file_path, ScreenshotImageFormat.Png);
            return file_path;
        }
    }
}
