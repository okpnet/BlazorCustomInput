using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
namespace WpfBlazor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            //serviceCollection.AddScoped<HttpClient>();
            //ここに追加
            //serviceCollection.AddLogging(option => option.AddDebug());
            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }
        private void Handle_UrlLoading(object sender, UrlLoadingEventArgs urlLoadingEventArgs)
        {

            if (urlLoadingEventArgs.Url.Host != "0.0.0.0")
            {
                urlLoadingEventArgs.UrlLoadingStrategy = UrlLoadingStrategy.OpenInWebView;
            }
        }
    }
}
