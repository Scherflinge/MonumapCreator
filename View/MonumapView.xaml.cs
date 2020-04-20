using MonumapCreator.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace MonumapCreator.View
{
    /// <summary>
    /// Interaction logic for MonumapView.xaml
    /// </summary>
    public partial class MapView : Window, MonumapViewInterface
    {
        MonumapViewModel m_vm;
        public MapView()
        {
            InitializeComponent();
            m_vm = new MonumapViewModel(this);
            this.DataContext = m_vm;
            m_vm.SetUpCanvas(Canvas);
            m_vm.SetUpImage(Image);
        }

        public object GetResource(String s)
        {
            return FindResource(s);
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!Canvas.Focusable)
                {
                    Canvas.Focusable = true;
                }
                Canvas.Focus();
            }
        }
    }
}
