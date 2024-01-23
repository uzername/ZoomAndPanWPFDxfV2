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
             cleanupPanAndZoomCanvas();
             renderDxf(dialog.FileName);
            }
        }
        private void cleanupPanAndZoomCanvas()
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
            canvas.resetTransform();
        }

        private void fitDxfDisplay()
        {
            // 1. scale dxf canvas to window size

            // 2. translocate dxf canvas
        }
    }
}
