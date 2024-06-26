﻿using System;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
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

namespace WpfPanAndZoom.CustomControls
{
    /// <summary>
    /// Interaktionslogik für PanAndZoomCanvas.xaml .
    /// https://stackoverflow.com/questions/35165349/how-to-drag-rendertransform-with-mouse-in-wpf .
    /// https://github.com/SEilers/WpfPanAndZoom
    /// </summary>
    public partial class PanAndZoomCanvas : Canvas
    {
        #region Variables
        private readonly MatrixTransform _transform = new MatrixTransform();
        private Point _initialMousePosition;

        private bool _dragging;
        private UIElement _selectedElement;
        private Vector _draggingDelta;

        private Color _lineColor = Colors.LightGray;
        //private Color _backgroundColor = Color.FromArgb(0xFF, 0x33, 0x33, 0x33);
        private Color _backgroundColor = Colors.White;
        private List<Line> _gridLines = new List<Line>();
        /// <summary>
        /// how fast do we zoom when scrolling mousewheel. This number should be bigger than 1 and default is 1.1 . Should never be negative, never be zero!
        /// </summary>
        public float Zoomfactor { get; set; } = 1.1f;
        /// <summary>
        /// current value of zoom
        /// </summary>
        public float ZoomValue { get; internal set; } = 1.0f;

        private const double StrokeThick = 4;
        private const double StrokeThin = 2;
        #endregion
        
        public PanAndZoomCanvas()
        {
            InitializeComponent();

            this.Cursor = Cursors.Cross;

            MouseDown += PanAndZoomCanvas_MouseDown;
            MouseUp += PanAndZoomCanvas_MouseUp;
            MouseMove += PanAndZoomCanvas_MouseMove;
            MouseWheel += PanAndZoomCanvas_MouseWheel;

            BackgroundColor = _backgroundColor;

            // draw lines
            for (int x = -4000; x <= 4000; x += 100)
            {
                Line verticalLine = new Line
                {
                    Stroke = new SolidColorBrush(_lineColor),
                    X1 = x,
                    Y1 = -4000,
                    X2 = x,
                    Y2 = 4000
                };

                if ( x % 1000 == 0 )
                {
                    verticalLine.StrokeThickness = StrokeThick;
                }
                else
                {
                    verticalLine.StrokeThickness = StrokeThin;
                }

                Children.Add(verticalLine);
                _gridLines.Add(verticalLine);
            }

            for (int y = -4000; y <= 4000; y += 100)
            {
                Line horizontalLine = new Line
                {
                    Stroke = new SolidColorBrush(_lineColor),
                    X1 = -4000,
                    Y1 = y,
                    X2 = 4000,
                    Y2 = y
                };

                if (y % 1000 == 0)
                {
                    horizontalLine.StrokeThickness = StrokeThick;
                }
                else
                {
                    horizontalLine.StrokeThickness = StrokeThin;
                }

                Children.Add(horizontalLine);
                _gridLines.Add(horizontalLine);
            }
        }        

        public Color LineColor
        {
            get { return _lineColor; }

            set
            {
                _lineColor = value;

                foreach( Line line in _gridLines )
                {
                    line.Stroke = new SolidColorBrush(_lineColor);
                }
            }
        }

        public Color BackgroundColor
        {
            get { return _backgroundColor; }

            set
            {
                _backgroundColor = value;
                Background = new SolidColorBrush(_backgroundColor);
            }
        }

        public void SetGridVisibility(Visibility value)
        {
            foreach (Line line in _gridLines)
            {
                line.Visibility = value;
            }
        }
        public void resetTransform()
        {
            _transform.Matrix = Matrix.Identity;
            foreach (UIElement child in this.Children)
            {
                child.RenderTransform = _transform;
                //adjust thick of grid lines
                if (child is System.Windows.Shapes.Shape)
                {
                    (child as System.Windows.Shapes.Shape).StrokeThickness *= ZoomValue;
                }
            }
            ZoomValue = 1;
        }
        private void PanAndZoomCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                _initialMousePosition = _transform.Inverse.Transform(e.GetPosition(this));
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                if (this.Children.Contains((UIElement)e.Source))
                {
                    _selectedElement = (UIElement)e.Source;
                    Point mousePosition = Mouse.GetPosition(this);
                    double x = Canvas.GetLeft(_selectedElement);
                    double y = Canvas.GetTop(_selectedElement);
                    Point elementPosition = new Point(x, y);
                    _draggingDelta = elementPosition - mousePosition;
                }
                _dragging = true;
            }
        }
        /// <summary>
        /// make sure that specified rectangular area completely fits in display. Includes ResetTransform call since it depends on it
        /// input : coordinates of bottom left point and upper right point
        /// </summary>
        public void highlightRectangleAreaToDisplay(double bottomLeftX, double bottomLeftY, double upperRightX, double upperRightY)
        {
            double controlWidth = this.ActualWidth;
            double controlHeight = this.ActualHeight;
            double contentWidth = Math.Abs(bottomLeftX - upperRightX);
            double contentHeight = Math.Abs(bottomLeftY - upperRightY);
            double sfH = controlHeight / contentHeight;
            double sfW = controlWidth / contentWidth;
            double scaleFactor = (sfH < sfW)? sfH : sfW;
            resetTransform();
            float translocatorX = (float)(controlWidth / 2 -contentWidth/2);
            float translocatorY = (float)(controlHeight / 2+contentHeight/2 );
            //let Zero point be placed in control's center
            translocateAction(new Vector(translocatorX, translocatorY ));
            float scalingX = (float)(controlWidth / 2);
            float scalingY = (float)(controlHeight / 2);
            scalingAction((float)scaleFactor, scalingX, scalingY);
        }
        private void PanAndZoomCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragging = false;
            _selectedElement = null;
        }

        private void PanAndZoomCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Point mousePosition = _transform.Inverse.Transform(e.GetPosition(this));
                Vector delta = Point.Subtract(mousePosition, _initialMousePosition);

                translocateAction(delta);
            }
            // it's regarding dragging element. To be removed in future because we won't drag elements here
            if (_dragging && e.LeftButton == MouseButtonState.Pressed)
            {
                double x = Mouse.GetPosition(this).X;
                double y = Mouse.GetPosition(this).Y;

                if (_selectedElement != null)
                {
                    Canvas.SetLeft(_selectedElement, x + _draggingDelta.X);
                    Canvas.SetTop(_selectedElement,  y + _draggingDelta.Y);
                }
            }
        }
        private void translocateAction(Vector delta)
        {
            var translate = new TranslateTransform(delta.X, delta.Y);
            _transform.Matrix = translate.Value * _transform.Matrix;

            foreach (UIElement child in this.Children)
            {
                child.RenderTransform = _transform;
            }
        }
        /// <summary>
        /// scale by a specified scale factor around center X and center Y
        /// </summary>
        private void scalingAction(float scaleFactor, double centerX, double centerY)
        {
            ZoomValue *= scaleFactor;
            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, centerX, centerY);
            _transform.Matrix = scaleMatrix;

            foreach (UIElement child in this.Children)
            {
                double x = Canvas.GetLeft(child);
                double y = Canvas.GetTop(child);

                double sx = x * scaleFactor;
                double sy = y * scaleFactor;

                //adjust thick of grid lines
                if (child is System.Windows.Shapes.Shape)
                {
                    (child as System.Windows.Shapes.Shape).StrokeThickness /= scaleFactor;
                }
                else if (child is Canvas)
                {
                    foreach (var itemChildInternal in (child as Canvas).Children)
                    {
                        (itemChildInternal as System.Windows.Shapes.Shape).StrokeThickness /= scaleFactor;
                    }
                }

                Canvas.SetLeft(child, sx);
                Canvas.SetTop(child, sy);

                child.RenderTransform = _transform;
            }
        }
        public Point ScreenCoordinatesToFieldCoordinates(Point inCoordinates)
        {
            // here is wizardry of matrix transforms
            Matrix mtr1 = new Matrix(_transform.Matrix.M11, _transform.Matrix.M12, _transform.Matrix.M21, _transform.Matrix.M22, _transform.Matrix.OffsetX, _transform.Matrix.OffsetY);
            mtr1.Invert();
            return mtr1.Transform(inCoordinates);
        }
        private void PanAndZoomCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed) return;
            float scaleFactor = Zoomfactor;
            if (e.Delta < 0)
            {
                scaleFactor = 1f / scaleFactor;
            }
            
            Point mousePostion = e.GetPosition(this);

            scalingAction(scaleFactor,mousePostion.X,mousePostion.Y);

            
        }
    }
}
