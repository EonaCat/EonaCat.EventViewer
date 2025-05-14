using EonaCat.EventViewer.Helpers;
using EonaCat.EventViewer.Model;
using EonaCat.EventViewer.Services;
using EonaCat.EventViewer.View;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace EonaCat.EventViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<EventLogModel> _allEntries = new();
        private Timer _refreshTimer;

        public MainWindow()
        {
            InitializeComponent();
            LogTypeComboBox.SelectedIndex = 0;
            SeverityComboBox.SelectedIndex = 0;
            LogTypeComboBox.SelectionChanged += (s, e) => LoadLogs();
            SeverityComboBox.SelectionChanged += (s, e) => LoadLogs();

            LoadLogs();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => LoadLogs();

        private void LoadLogs()
        {
            string logType = ((ComboBoxItem)LogTypeComboBox.SelectedItem).Content.ToString();
            string severity = ((ComboBoxItem)SeverityComboBox.SelectedItem).Content.ToString();
            string search = SearchTextBox.Text?.ToLower() ?? "";
            DateTime? from = FromDatePicker.SelectedDate;
            DateTime? to = ToDatePicker.SelectedDate;

            try
            {
                var log = new EventLog(logType);

                var filtered = log.Entries.Cast<EventLogEntry>()
                   .Select(e => new EventLogModel
                   {
                       TimeGenerated = e.TimeGenerated,
                       Source = e.Source,
                       EventID = e.InstanceId.GetHashCode(),
                       EntryType = e.EntryType.ToString(),
                       Category = e.Category,
                       Username = e.UserName,
                       MachineName = e.MachineName,
                       Message = e.Message
                   })
                   .Where(e =>
                       (severity == "All" || e.EntryType == severity) &&
                       (string.IsNullOrEmpty(search) ||
                        e.Source.ToLower().Contains(search) ||
                        e.EntryType.ToLower().Contains(search) ||
                        e.Category.ToLower().Contains(search) ||
                        e.Username?.ToLower().Contains(search) == true ||
                        e.MachineName.ToLower().Contains(search) ||
                        e.Message.ToLower().Contains(search)) &&
                       (!from.HasValue || e.TimeGenerated >= from.Value) &&
                       (!to.HasValue || e.TimeGenerated <= to.Value))
                   .OrderByDescending(e => e.TimeGenerated)
                   .ToList();

                _allEntries = filtered;
                EventDataGrid.ItemsSource = _allEntries;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load log: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = "EventLogs.csv"
            };
            if (dlg.ShowDialog() == true)
            {
                ExportService.ExportToCsv(_allEntries, dlg.FileName);
                MessageBox.Show("Exported successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AutoRefresh_Checked(object sender, RoutedEventArgs e)
        {
            _refreshTimer = new Timer(5000);
            _refreshTimer.Elapsed += (_, _) => Dispatcher.Invoke(LoadLogs);
            _refreshTimer.Start();
        }

        private void AutoRefresh_Unchecked(object sender, RoutedEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }

        private void EventDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (EventDataGrid.SelectedItem is EventLogModel selected)
            {
                MessageWindow messageWindow = new MessageWindow(selected);
                messageWindow.Owner = this;
                messageWindow.ShowDialog();
            }
        }

        private void RefreshRemoteLogs_Click(object sender, RoutedEventArgs e)
        {
            string remoteComputer = RemoteComputerTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(remoteComputer))
            {
                MessageBox.Show("Please enter a valid remote computer name or IP address.");
                return;
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            try
            {
                // Get logs from the remote computer using WMI and the provided credentials
                var eventLogs = GetRemoteEventLogs(remoteComputer, username, password);

                // Bind the data to the DataGrid
                EventDataGrid.ItemsSource = eventLogs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private List<EventLogModel> GetRemoteEventLogs(string remoteComputer, string username, string password)
        {
            List<EventLogModel> eventLogs = new List<EventLogModel>();

            // Get event logs from the remote computer using WMI
            var managementLogs = RemoteEventLogAccess.GetRemoteEventLogs(remoteComputer, username, password);

            foreach (ManagementObject log in managementLogs)
            {
                // Process the logs into EventLogModel objects
                eventLogs.Add(new EventLogModel
                {
                    TimeGenerated = ManagementDateTimeConverter.ToDateTime(log["TimeGenerated"].ToString()),
                    Source = log["SourceName"]?.ToString(),
                    EventID = Convert.ToInt32(log["EventCode"]),
                    EntryType = log["Type"]?.ToString(),
                    Category = log["Category"]?.ToString(),
                    Username = log["User"]?.ToString(),
                    MachineName = remoteComputer,
                    Message = log["Message"]?.ToString()
                });
            }

            return eventLogs;
        }

        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Refresh_Click(sender, e);
            }
        }
    }
}
