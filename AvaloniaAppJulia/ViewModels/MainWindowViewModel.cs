using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using JuliaSharp;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ReactiveUI;
using System.Reactive;
//using LiveChartsCore.Defaults;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.IO;
using Avalonia.Input;

namespace AvaloniaAppJulia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static Random _random = new Random();
        public string Greeting => "Welcome to Avalonia!";

        public static double[] values = new double[10];
        //private readonly ObservableCollection<ObservablePoint> _observableValues;
        Stopwatch stopwatch = new Stopwatch();
        public MainWindowViewModel()
        {
            DoTheThing = ReactiveCommand.Create(RunTheThing);
            //Do = ReactiveCommand.Create(RunTheThing2);
        }
        public static String StringValue = "CLICK!";

        public ReactiveCommand<Unit, Unit> DoTheThing { get; }
        public ReactiveCommand<Unit, Unit> Do { get; }
        public ObservableCollection<ISeries> Series3 { get; set; } = new ObservableCollection<ISeries>
        {
                        new LineSeries<double>
                        {
                            Values =new ObservableCollection<double> { _random.NextDouble()*10, _random.NextDouble()*5, 3, 5, 3, 4, 6 },
                            Fill = null
                        }
        };
        void RunTheThing()
        {
            stopwatch.Restart();
            stopwatch.Start();
            //JuliaValue<string> b = JuliaNative.UnboxString("ஃத௴ழ்");
            JuliaValue<double> r1 = JuliaNative.EvalString("sqrt(2.0)");
            double d = r1;
            JuliaArray<double> dc = JuliaNative.EvalString("rand(1,10)");
            //Console.WriteLine(dc);
            string[] arr = dc.ToString().Split(' ');
            double[] darray = new double[arr.Length - 2];        
            //JuliaNative.EvalString("push!(LOAD_PATH, \"G:/Freelancer/Julia/Juliasimple/\")");
            //JuliaNative.EvalString("using test");
            //JuliaNative.EvalString("TestFunc()");
            //newwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww
            string filename = "test.jl";
            string path1=System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.AppDomain.CurrentDomain.RelativeSearchPath ?? "");
            string path2=path1.Replace("\\", "/");
            JuliaNative.EvalString("Base.include(Main,\""+path2+"/"+filename+"\")");
            JuliaNative.EvalString("using Main.test");
            Object Hello = JuliaNative.EvalString("Main.test");
            /* get `foo!` function */
            object foo = JuliaNative.GetFunction((IntPtr)Hello, "foo!");
            // object ans = JuliaNative.jl_call0(foo);
            IntPtr res1 = JuliaNative.Call1((IntPtr)foo, (IntPtr)JuliaNative.BoxFloat64(200.0));
            double vv = JuliaNative.UnboxFloat64(res1);
            /////////////            
            object foo1 = JuliaNative.GetFunction((IntPtr)Hello, "Func1");
            // object ans = JuliaNative.jl_call0(foo1);
            IntPtr res2 = JuliaNative.Call1((IntPtr)foo1, (IntPtr)JuliaNative.BoxFloat64(0));
            long vv2 = JuliaNative.UnboxInt64(res2);
            /////////////
            object foo11 = JuliaNative.GetFunction((IntPtr)Hello, "@ab");           
            /////////////
            object foo2 = JuliaNative.GetFunction((IntPtr)Hello, "Func2");
            // object ans = JuliaNative.jl_call0(foo);
            IntPtr res3 = JuliaNative.Call1((IntPtr)foo2, (IntPtr)JuliaNative.BoxFloat64(0));
            long vv3 = JuliaNative.UnboxInt64(res3);//--------- need to check
            IntPtr ptr = IntPtr.Add(res3, 8);
            string ppa = Marshal.PtrToStringUTF8(ptr, (int)vv3); // utf8 for tamil //ANSI for English
            string[] aa = ppa.Split('#');


            int[] length = new int[aa.Length];
            for (int j = 0; j < aa.Length; j++)
            {
                string pp = aa[j];


                string pp1 = string.Join("", pp.Split('[', ']', ',', ';', '\''));
                string[] arr2 = pp1.ToString().Split(' ');
                length[j] = arr2.Length;
            }


            double[] values1 = new double[length[0]];
            double[] values2 = new double[length[1]];



            for (int j = 0; j < aa.Length; j++)
            {
                string pp = aa[j];


                string pp1 = string.Join("", pp.Split('[', ']', ',', ';', '\''));
                string[] arr2 = pp1.ToString().Split(' ');
                for (int i = 0; i < arr2.Length; i++)
                {
                    if (j == 0)
                        double.TryParse(arr2[i], out values1[i]);
                    else
                        double.TryParse(arr2[i], out values2[i]);
                }
            }
            
            Series3.Clear();

            Series3.Add(

                        new LineSeries<double>
                        {
                            Name = "Random X value",
                            Values = values1,
                            Fill = null
                        }
                );

            Series3.Add(

                       new LineSeries<double>
                       {
                           Name = "Differntiation",
                           Values = values2,
                           Fill = null
                       }
               );




            //   Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);
            long microseconds = stopwatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
            StringValue = microseconds.ToString()+ " μs";
            stopwatch.Stop();
        }

      

        //newwwwwwwwwwwwwwwww

    }
}


    
    






    






   




