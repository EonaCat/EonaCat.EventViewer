using EonaCat.EventViewer.Model;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EonaCat.EventViewer.Services
{
    public static class ExportService
    {
        public static void ExportToCsv(List<EventLogModel> entries, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("TimeGenerated,Source,EventID,EntryType,Message");

            foreach (var e in entries)
            {
                sb.AppendLine($"\"{e.TimeGenerated}\",\"{e.Source}\",{e.EventID},\"{e.EntryType}\",\"{e.Message.Replace("\"", "\"\"")}\"");
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
