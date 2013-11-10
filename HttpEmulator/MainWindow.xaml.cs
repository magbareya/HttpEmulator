using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace HttpEmulator
{
    public delegate void WindowClosed();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            this.InitializeComponent();
            // Insert code required on object creation below this point.
        }
    }
}