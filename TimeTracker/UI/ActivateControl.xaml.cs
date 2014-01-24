using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimeTracker.UI {
    /// <summary>
    /// Interaction logic for ActivateControl.xaml
    /// </summary>
    public partial class ActivateControl : UserControl {
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(ActivateControl));

        public static readonly DependencyProperty ActiveBackgroundColorProperty = DependencyProperty.Register("ActiveBackgroundColor", typeof(Brush), typeof(ActivateControl));

        public static readonly DependencyProperty InactiveBackgroundColorProperty = DependencyProperty.Register("InactiveBackgroundColorProperty", typeof(Brush), typeof(ActivateControl));

        public bool IsActive {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public Brush ActiveBackgroundColor {
            get { return (Brush)GetValue(ActiveBackgroundColorProperty); }
            set { SetValue(ActiveBackgroundColorProperty, value); }
        }

        public Brush InactiveBackgroundColor {
            get { return (Brush)GetValue(InactiveBackgroundColorProperty); }
            set { SetValue(InactiveBackgroundColorProperty, value); }
        }

        public ActivateControl() {
            InitializeComponent();

            IsActive = true;
            ActiveBackgroundColor = new SolidColorBrush(Colors.Black);
            InactiveBackgroundColor = new SolidColorBrush(Colors.LightGray);
        }
    }
}
