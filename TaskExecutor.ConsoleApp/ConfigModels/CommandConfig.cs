using System;
using System.Collections.Generic;
using System.Text;

namespace TaskExecutor.ConsoleApp.ConfigModels
{
    public class CommandConfig
    {
        public string Url { get; set; }
        public string Arg { get; set; }
        public bool IsActive { get; set; }
        public int TimeoutInMinutes { get; set; }
    }
}
