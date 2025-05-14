using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EonaCat.EventViewer.Model
{
    public class EventLogModel
    {
        public DateTime TimeGenerated { get; set; }
        public string Source { get; set; }
        public long EventID { get; set; }
        public string EntryType { get; set; }
        public string Category { get; set; }
        public string Username { get; set; }
        public string MachineName { get; set; }
        public string Message { get; set; }
        public string IconPath => EntryType switch
        {
            "Error" => "pack://application:,,,/Images/error.png",
            "Warning" => "pack://application:,,,/Images/warning.png",
            "Information" => "pack://application:,,,/Images/info.png",
            _ => "pack://application:,,,/Images/info.png"
        };
    }
}
