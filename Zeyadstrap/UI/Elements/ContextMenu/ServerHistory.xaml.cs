using Zeyadstrap.Integrations;
using Zeyadstrap.UI.ViewModels.ContextMenu;

namespace Zeyadstrap.UI.Elements.ContextMenu
{
    /// <summary>
    /// Interaction logic for ServerInformation.xaml
    /// </summary>
    public partial class ServerHistory
    {
        public ServerHistory(ActivityWatcher watcher)
        {
            var viewModel = new ServerHistoryViewModel(watcher);

            viewModel.RequestCloseEvent += (_, _) => Close();

            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
