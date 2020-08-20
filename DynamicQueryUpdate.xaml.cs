using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReactiveDynamicData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReactiveWindow<DynamicQueryUpdateVM>
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new DynamicQueryUpdateVM();

            this.WhenActivated(disposable =>
            {
                this.Bind(ViewModel,
                    viewModel => viewModel.SearchInput,
                    view => view.QueryInput.Text)
                .DisposeWith(disposable);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.SearchResults,
                    view => view.ResultGrid.ItemsSource)
                .DisposeWith(disposable);
            });
        }
    }
}
