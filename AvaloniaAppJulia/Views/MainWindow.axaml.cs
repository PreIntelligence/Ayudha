using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using JuliaSharp;
using Avalonia.Interactivity;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;
//using ViewModelsSamples.Lines.AutoUpdate;

namespace AvaloniaAppJulia.Views
{
    public partial class MainWindow : Window
    {

       

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainWindowViewModel();
          




#if DEBUG
            this.AttachDevTools();
#endif
        }



        public IEnumerable<ISeries> Series { get; set; }

        public void button_Click(object sender, RoutedEventArgs e)
        {
            //JuliaNative.InitThreading();
            ////JuliaValue<string> b  =JuliaNative.UnboxString("ஃத௴ழ்");



            //JuliaValue<double> r = JuliaNative.EvalString("sqrt(2.0)");
            //double d = r;

            //JuliaArray<double> dc = JuliaNative.EvalString("rand(10,4)");
            ////Console.WriteLine(dc);
            //string[] arr = dc.ToString().Split(' '); ;

            //for (int i = 0; i <= 10; i++)
            //{

            //}

            // Change button text when button is clicked.
            // var button = (Button)sender;
            // button.Content = "ஃத௴ழ் Hello, Avalonia!" + dc.ToString();






            //  ViewModels.MainWindowViewModel test = new ViewModels.MainWindowViewModel();





            //var Values = new ObservableCollection<double> { 20, 1, 3, 5, 3, 4, 6, 7, 8, 8 };




            //test.Series = new ObservableCollection<ISeries>
            //{
            //  new LineSeries<double>
            //  {
            //      Values = new ObservableCollection<double> { 2000, 1, 3, 5, 3, 4, 6,7,8,8},
            //      Fill = null
            //  }
            //};




            //Series = new ObservableCollection<ISeries>
            //    {
            //        new LineSeries<double>
            //        {
            //            Values =  new ObservableCollection<double> { 20, 1, 3, 5, 3, 4, 6, 7, 8, 8 },
            //           Fill = null
            //        }
            //    };


            ((Button)sender).Content = "";
            ((Button)sender).Content = ViewModels.MainWindowViewModel.StringValue;



        }
        //public void button_Click(object sender, RoutedEventArgs args) => ((Button)sender).Content = ViewModels.MainWindowViewModel.StringValue;

    private void InitializeComponent()
        {

            //Console.WriteLine($"{d}");
            JuliaNative.InitThreading();
            AvaloniaXamlLoader.Load(this);
        }



    }
}
