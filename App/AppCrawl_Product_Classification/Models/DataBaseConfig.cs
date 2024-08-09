using System;
using System.Collections.Generic;
using System.Text;

namespace AppLandingPage.Models
{
    public class DataBaseConfig
    {
        public DBConfig SqlServer { get; set; }
        public IPConfig Redis { get; set; }
        public IPConfig Elastic { get; set; }
    }

    public class DBConfig
    {
        public string ConnectionString { get; set; }
    }

    public class IPConfig
    {
        public string Host { get; set; }
    }
}
