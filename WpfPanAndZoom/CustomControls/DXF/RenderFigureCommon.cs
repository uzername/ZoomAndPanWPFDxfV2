using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WpfPanAndZoom.CustomControls.DXF
{
    /// <summary>
    /// special class to be used as binding for render figure. Combines arc and line for easier binding.
    /// https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-implement-property-change-notification?view=netframeworkdesktop-4.8
    /// </summary>
    public class RenderFigureCommon : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private double _lineX1;  private double _lineY1;
        private double _lineX2;  private double _lineY2;
        
        private double _arcStartAngle; private double _arcEndAngle;
        private double _arcRadius; private Point _arcCenter;
        // Call OnPropertyChanged whenever the property is updated
        public double LineX1  {
            get { return _lineX1; }
            set
            {
                _lineX1 = value;                
                OnPropertyChanged();
            }
        }
        public double LineX2
        {
            get { return _lineX2; }
            set
            {
                _lineX2 = value;
                OnPropertyChanged();
            }
        }
        public double LineY1
        {
            get { return _lineY1; }
            set
            {
                _lineY1 = value;
                OnPropertyChanged();
            }
        }
        public double LineY2
        {
            get { return _lineY2; }
            set
            {
                _lineY2 = value;
                OnPropertyChanged();
            }
        }
        public double ArcStartAngle
        {
            get { return _arcStartAngle; }
            set
            {
                _arcStartAngle = value;
                OnPropertyChanged();
            }
        }
        public double ArcEndAngle
        {
            get { return _arcEndAngle; }
            set
            {
                _arcEndAngle = value;
                OnPropertyChanged();
            }
        }
        public double ArcRadius
        {
            get { return _arcRadius; }
            set
            {
                _arcRadius = value;
                OnPropertyChanged();
            }
        }
        public Point ArcCenter
        {
            get { return _arcCenter; }
            set
            {
                _arcCenter = value;
                OnPropertyChanged();
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
