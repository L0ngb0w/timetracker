﻿using System;
using System.Collections.Generic;
using System.Linq;
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

namespace TimeTracker
{
  /// <summary>
  /// Interaction logic for ListIntervalEntry.xaml
  /// </summary>
  public partial class ListIntervalEntry : UserControl
  {
    public ListIntervalEntry()
    {
      InitializeComponent();
    }

    private void TextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      var textBox = sender as TextBox;
      if (textBox != null && textBox.Visibility == Visibility.Visible)
        textBox.Focus();
    }

    void OnDescriptionCommit(object sender, RoutedEventArgs e)
    {
        var binding = TextBoxDescription.GetBindingExpression(TextBox.TextProperty);
        binding.UpdateSource();
    }
  }
}
