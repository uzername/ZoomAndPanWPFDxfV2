using CommunityToolkit.Mvvm.ComponentModel;
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
        /// <summary>
        /// how should be dxf file rendered - with mirroring and rotation. First mirror then turn
        /// </summary>
        public DXFParameters renderValues { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            renderValues = new DXFParameters { ValAngleDegrees = 0, ValMirroring = false };
            renderValues.PropertyChanged += RenderValues_PropertyChanged;
            renderValues.PropertyChanging += RenderValues_PropertyChanging;
            MirrorAnglePanel.DataContext = renderValues;

        }

        private void RenderValues_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
        {
        
        }

        private void RenderValues_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }

        private void renderDxf(String inFname)
        {
            cleanupPanAndZoomCanvas();

            BoundBox resBoundBox = new BoundBox();
            double displcmntX = 0;
            double displcmntY = 0;
            // get initial canvas with dxf
            Canvas canvasToShow = ProfileConstructor2D.parseAndRenderDXF(inFname, out resBoundBox, out displcmntX, out displcmntY);
            // allocate it relatively to parent
            double obtainedHeight = Math.Abs(resBoundBox.bottomRight.Y-resBoundBox.upperLeft.Y);
            double obtainedWidth = Math.Abs(resBoundBox.bottomRight.X - resBoundBox.upperLeft.X);
            // contour has been displaced before properly, just move it a bit to up
            Canvas.SetTop (canvasToShow, 0 - obtainedHeight);
            Canvas.SetLeft(canvasToShow, 0);           
            canvas.Children.Add(canvasToShow);
            // focus on obtained dxf shape
            canvas.highlightRectangleAreaToDisplay(0, 0, obtainedWidth, obtainedHeight);
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
             renderDxf(dialog.FileName);
            }
        }

        /// remove Canvas with DXF but keep everything else. 
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
        private void PerformMirroring()
        {

        }
    }
     public class DXFParameters: ObservableObject
    {
        public bool ValMirroring { get; set; }
        public double ValAngleDegrees { get; set;}
    }
}
