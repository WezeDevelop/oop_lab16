using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;


namespace UdpChatApp.Views
{
    public partial class PromptDialog : Window
    {
        public string Result { get; private set; }

        public PromptDialog(string message, string defaultValue = "")
        {
            InitializeComponent();
            PromptLabel.Text = message;
            InputBox.Text = defaultValue;

            OkButton.Click += (_, _) =>
            {
                Result = InputBox.Text;
                Close(Result);
            };

            CancelButton.Click += (_, _) => Close(null);
        }

        public static async Task<string> ShowDialog(Window parent, string message, string defaultValue = "")
        {
            var dialog = new PromptDialog(message, defaultValue);
            return await dialog.ShowDialog<string>(parent);
        }
    }
}
