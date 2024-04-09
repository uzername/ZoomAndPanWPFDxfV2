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
using WpfPanAndZoom.CustomControls;
using WpfPanAndZoom.CustomControls.DXF;

namespace WpfPanAndZoom
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// here is a canvas that contains contours of dxf file. it will be dynamically rendered in window. Not in xaml because of the problem: 
        /// https://microsoft.public.expression.interactivedesigner.narkive.com/sYbM1lpY/cannot-set-name-attribute-value
        /// </summary>
        DXFCanvas canvasToShow2 = new DXFCanvas();
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
            canvas.MouseMove += ZoompanCanvas_AdjustCoordinate;
            canvas.Children.Add(canvasToShow2);
        }

        private void ZoompanCanvas_AdjustCoordinate(object sender, MouseEventArgs e)
        {
            double x = Mouse.GetPosition(canvas).X;
            double y = Mouse.GetPosition(canvas).Y;
            Point pntMouse = Mouse.GetPosition(canvas);
            Point pntControl = canvas.ScreenCoordinatesToFieldCoordinates(pntMouse);
            // show coordinates on mouse move
            renderValues.CoordValuesFull = $"[{pntControl.X:F3};{-pntControl.Y:F3}]";
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

            canvasToShow2.N1_PreloadDxfFile(inFname);
            BoundBox resBoundBox = new BoundBox();
            double displcmntX = 0;
            double displcmntY = 0;
            // get initial canvas with dxf
            // Canvas canvasToShow = ProfileConstructor2D.parseAndRenderDXF(inFname, out resBoundBox, out displcmntX, out displcmntY);            
            canvasToShow2.N2_modifyRenderFiguresForDxfFile(renderValues.ValMirroring, renderValues.ValAngleDegrees, true);
            resBoundBox = canvasToShow2.thetransformedBox;
            // allocate it relatively to parent
            double obtainedHeight = Math.Abs(resBoundBox.bottomRight.Y-resBoundBox.upperLeft.Y);
            double obtainedWidth = Math.Abs(resBoundBox.bottomRight.X - resBoundBox.upperLeft.X);
            // contour has been displaced before properly, just move it a bit to up
            Canvas.SetTop (canvasToShow2, 0 - obtainedHeight);
            Canvas.SetLeft(canvasToShow2, 0);           
            //canvas.Children.Add(canvasToShow);
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
            canvasToShow2.N0_CleanupCanvas();
            canvas.resetTransform();
        }

        private void renderParsedFile()
        {
            BoundBox resBoundBoxNew = new BoundBox();
            BoundBox resBoundBoxOld = new BoundBox ();
            // get initial canvas with dxf
            // Canvas canvasToShow = ProfileConstructor2D.parseAndRenderDXF(inFname, out resBoundBox, out displcmntX, out displcmntY);            
            resBoundBoxOld = canvasToShow2.thetransformedBox;
            canvasToShow2.N2_modifyRenderFiguresForDxfFile(renderValues.ValMirroring, renderValues.ValAngleDegrees, false);
            resBoundBoxNew = canvasToShow2.thetransformedBox;
            // allocate it relatively to parent
            double obtainedHeightNew = Math.Abs(resBoundBoxNew.bottomRight.Y - resBoundBoxNew.upperLeft.Y);
            double obtainedWidthNew = Math.Abs(resBoundBoxNew.bottomRight.X - resBoundBoxNew.upperLeft.X);

            double obtainedHeightOld = Math.Abs(resBoundBoxOld.bottomRight.Y - resBoundBoxOld.upperLeft.Y);
            double obtainedWidthOld = Math.Abs(resBoundBoxOld.bottomRight.X - resBoundBoxOld.upperLeft.X);

            double deltaHeight = obtainedHeightNew - obtainedHeightOld;
            double currentTop = Canvas.GetTop(canvasToShow2);
            // contour has been displaced before properly, just move it a bit to up according to delta
            double cZoom = canvas.ZoomValue;
            Canvas.SetTop(canvasToShow2, currentTop-cZoom*deltaHeight);
            
            //Canvas.SetTop(canvasToShow2, 0 - obtainedHeight);
            //Canvas.SetLeft(canvasToShow2, 0);
        }

        private void ChkMirror_Checked(object sender, RoutedEventArgs e)
        {
            renderParsedFile();
        }

        private void TxtAngleDeg_KeyUp(object sender, KeyEventArgs e)
        {
            if ( e.Key == Key.Enter )
            {
                renderParsedFile();
            }
        }
        /// <summary>
        /// User clicked on Plus or Minus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlusMinus_Click(object sender, RoutedEventArgs e)
        {
            String nValue = (sender as Button).Tag.ToString();
            try
            {
                renderValues.ValAngleDegrees += Double.Parse(nValue);
                (TxtAngleDeg).GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                renderParsedFile();
            } catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }
    }
    public class DXFParameters: ObservableObject
    {
        
        private string _coordValuesFull;
        public bool ValMirroring { get; set; }
        public double ValAngleDegrees { get; set;}

        public string CoordValuesFull { get => _coordValuesFull; set => SetProperty(ref _coordValuesFull, value); }
    }
}
