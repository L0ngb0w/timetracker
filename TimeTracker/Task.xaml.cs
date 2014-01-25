using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TimeTracker {
    /// <summary>
    /// Interaction logic for Task.xaml
    /// </summary>
    public partial class Task : UserControl {
        public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register("IsInEditMode", typeof(bool), typeof(Task));

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Task));

        public bool IsInEditMode {
            get { return (bool)GetValue(IsInEditModeProperty); }
            set { SetValue(IsInEditModeProperty, value); }
        }

        public bool IsExpanded {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public Task() {
            InitializeComponent();

            IsInEditMode = false;
            IsExpanded = false;
        }

        private void ContentControl_MouseUp(object sender, MouseButtonEventArgs e) {
            IsExpanded = !IsExpanded;
        }

        private void ActivateControl_MouseUp(object sender, MouseButtonEventArgs e) {
            var task = ((ITaskViewModel)DataContext);

            if (task.IsActive) {
                task.Terminate();
            } else {
                task.Start();
            }
        }
    }
}
