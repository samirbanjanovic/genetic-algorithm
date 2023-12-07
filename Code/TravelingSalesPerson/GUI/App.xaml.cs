using GUI.ViewModels;
using GUI.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            GraphColoringView view = new GraphColoringView();
            GraphColoringViewModel viewModel = new GraphColoringViewModel(view);
            view.DataContext = viewModel;

            view.Show();
        }
    }
}
