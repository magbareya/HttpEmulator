using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HttpEmulator
{
    /// <summary>
    /// Interaction logic for AdvancedForm.xaml
    /// </summary>
    public partial class AdvancedForm : Window
    {
        public AdvancedForm()
        {
            InitializeComponent();
            this.btnOk.Click += BtnOkOnClick;
            this.btnCancel.Click += BtnCancelOnClick;
        }

        private void BtnCancelOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnOkOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void AllowOnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            int i;
            if (int.TryParse(e.Text, out i) && i >= 0)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
