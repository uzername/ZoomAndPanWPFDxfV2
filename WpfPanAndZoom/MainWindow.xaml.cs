using Ookii.Dialogs.Wpf;
using System;
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
using WpfPanAndZoom.CustomControls.DXF;

namespace WpfPanAndZoom
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }
        private void testComponents()
        {
            CustomControls.Widget w1 = new CustomControls.Widget
            {
                Width = 200,
                Height = 150
            };
            canvas.Children.Add(w1);
            w1.Header.Text = "Widget 1";
            Canvas.SetTop(w1, 100);
            Canvas.SetLeft(w1, 100);

            CustomControls.Widget w2 = new CustomControls.Widget
            {
                Width = 200,
                Height = 150
            };
            canvas.Children.Add(w2);
            w2.Header.Text = "Widget 2";
            w2.HeaderRectangle.Fill = Brushes.Blue;
            Canvas.SetTop(w2, 400);
            Canvas.SetLeft(w2, 400);

            CustomControls.Widget w3 = new CustomControls.Widget
            {
                Width = 200,
                Height = 150
            };
            canvas.Children.Add(w3);
            w3.Header.Text = "Widget 3";
            w3.HeaderRectangle.Fill = Brushes.Red;
            Canvas.SetTop(w3, 400);
            Canvas.SetLeft(w3, 800);
        }
        private void renderDxf(String inFname)
        {
            BoundBox resBoundBox = new BoundBox();
            double displcmntX = 0;
            double displcmntY = 0;
            Canvas canvasToShow = ProfileConstructor2D.parseAndRenderDXF(inFname, 0, false, out resBoundBox, out displcmntX, out displcmntY);            
            canvas.Children.Add(canvasToShow);
        }
        private void pathChoose_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Filter = "DXF files (*.dxf)|*.dxf";
            if (!VistaFileDialog.IsVistaFileDialogSupported)
                MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular open file dialog will be used. Please use Windows Vista to see the new dialog.", "Sample open file dialog");
            if ((bool)dialog.ShowDialog(this))
            {
             this.pathText.Text = dialog.FileName;
             cleanupCanvas();
             renderDxf(dialog.FileName);
            }
        }
        private void cleanupCanvas()
        {
            int ii = 0; bool found = false;
            foreach (var item in canvas.Children)
            {                
                if (item is Canvas && (item as Canvas)?.Name==ProfileConstructor2D.strCanvasName)
                {
                    found = true;
                    break;
                }
                ii++;
            }
            if (found)
            canvas.Children.RemoveAt(ii);
        }
    }
}
