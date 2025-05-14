using EonaCat.EventViewer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EonaCat.EventViewer.View
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow(EventLogModel selectedEvent)
        {
            InitializeComponent();

            EventIDTextBlock.Text = selectedEvent.EventID.ToString();
            TimeGeneratedTextBlock.Text = selectedEvent.TimeGenerated.ToString("g");
            SourceTextBlock.Text = selectedEvent.Source;
            MessageTextBox.Text = selectedEvent.Message;
        }
    }
}
