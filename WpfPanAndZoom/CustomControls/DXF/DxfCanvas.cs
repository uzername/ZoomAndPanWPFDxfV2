using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using IxMilia.Dxf.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WpfPanAndZoom.CustomControls.DXF
{
    /// <summary>
    /// Control that contains all of the DXF related contours
    /// </summary>
    public class DxfCanvas: Canvas {
        private DxfFile dxfFileCurrent = null;        
        /// <summary>
        /// list which is bound to elements' coordinates contained inside this canvas
        /// </summary>
        private List<RenderFigureCommon> renderFigures = null;
        /// <summary>
        /// raw bound box of current dxf file. no mirror and no rotation
        /// </summary>
        private BoundBox thecurrentBox = null;     
        /// <summary>
        /// bound box of current dxf file with mirror and rotation applied
        /// </summary>
        private BoundBox thetransformedBox=null;
        // offsets of bound box
        public double offsetX { get; private set; } = 0; 
        public double offsetY { get; private set; } = 0;
        public System.Windows.Media.Brush currentStroke = System.Windows.Media.Brushes.Red;
        private RenderFigureCommon getLineRenderFigure(DxfLine line)
        {
            /*
            Line lineLine = new Line();
            lineLine.X1 = line.P1.X + offsetX;
            lineLine.Y1 = line.P1.Y + offsetY;
            lineLine.X2 = line.P2.X + offsetX;
            lineLine.Y2 = line.P2.Y + offsetY;
            lineLine.Stroke = currentStroke;
            */
            RenderFigureCommon rfcLine = new RenderFigureCommon();
            /// TODO
            return rfcLine;
        }

        void handleArc(DxfArc arc)
        {
            double arcCenterX = arc.Center.X + offsetX;
            double arcCenterY = arc.Center.Y + offsetY;
            double radAngleStart = ProfileConstructor2D.ConvertDegreesToRadians(arc.StartAngle);
            double radAngleEnd = ProfileConstructor2D.ConvertDegreesToRadians(arc.EndAngle);
            if (radAngleStart > radAngleEnd)
            {
                radAngleEnd += Math.PI * 2;
            }

            // arc in dxf is counterclockwise
            Arc arcGraphic = new Arc();
            double correctedXCenter = arcCenterX;
            double correctedYCenter = arcCenterY;
            arcGraphic.StartAngle = radAngleEnd;
            arcGraphic.EndAngle = radAngleStart;
            arcGraphic.Radius = arc.Radius;
            arcGraphic.Center = new System.Windows.Point(correctedXCenter, correctedYCenter);
            arcGraphic.Stroke = currentStroke;
            //pmb.Children.Add(arcGraphic);

        }

        /// <summary>
        /// Start here. Get entities from dxf file
        /// </summary>
        /// <param name="inFname">Full path to file</param>
        public void N1_PreloadDxfFile(String inFname)
        {            
            dxfFileCurrent = DxfFile.Load(inFname);
            // get initial bound box
            thecurrentBox = ProfileConstructor2D.getDisplacementOfDxf(dxfFileCurrent, false, 0, 0, 0, null);            
            if ((thecurrentBox.upperLeft.X != 0) || (thecurrentBox.bottomRight.Y != 0))
            {
                offsetX = -thecurrentBox.upperLeft.X; offsetY = -thecurrentBox.bottomRight.Y;
            }
        }
        /// <summary>
        /// iterate over dxfFileCurrent and fill in renderFigures. Also fills in actual shapes in Canvas
        /// </summary>
        /// <param name="mirror">mirror is applied before turning</param>
        /// <param name="AngleDeg">do turning. Angle is in degrees</param>
        /// <param name="initRequired">should we really re-init all the stuff? set to TRUE if you are calling it when initially parsing dxf file. set to FALSE when you are applying mirror and angle to already parsed dxf file</param>
        public void N2_modifyRenderFiguresForDxfFile(bool mirror, double AngleDeg, bool initRequired)
        {
            if (initRequired)
            {
                // start from beginning
                renderFigures = new List<RenderFigureCommon>();
                this.Children.Clear();
            }            
            double midPointHorizontal = (thecurrentBox.upperLeft.X + thecurrentBox.bottomRight.X) / 2.0;
            double midPointVertical = (thecurrentBox.upperLeft.Y + thecurrentBox.bottomRight.Y) / 2.0;
            // recalculate rotated box is required
            double AngleRad = ProfileConstructor2D.ConvertDegreesToRadians(AngleDeg);
            thetransformedBox = ProfileConstructor2D.getDisplacementOfDxf(dxfFileCurrent, true, midPointHorizontal, midPointVertical, AngleRad, mirror);            
                offsetX = -thetransformedBox.upperLeft.X; offsetY = -thetransformedBox.bottomRight.Y;
            foreach (DxfEntity entity in dxfFileCurrent.Entities)
            {
                if (ProfileConstructor2D.ignoredLayers.Contains(entity.Layer.ToLower()) == true) continue;
                switch (entity.EntityType)
                {
                    case DxfEntityType.Arc:
                        {
                            //
                            break;
                        }
                    case DxfEntityType.ArcAlignedText:
                        break;
                    case DxfEntityType.Attribute:
                        break;
                    case DxfEntityType.AttributeDefinition:
                        break;
                    case DxfEntityType.Body:
                        break;
                    case DxfEntityType.Circle:
                        {
                            /// TODO
                            break;
                        }
                    case DxfEntityType.DgnUnderlay:
                        break;
                    case DxfEntityType.Dimension:
                        break;
                    case DxfEntityType.DwfUnderlay:
                        break;
                    case DxfEntityType.Ellipse:
                        break;
                    case DxfEntityType.Face:
                        break;
                    case DxfEntityType.Hatch:
                        break;
                    case DxfEntityType.Helix:
                        break;
                    case DxfEntityType.Image:
                        break;
                    case DxfEntityType.Insert:
                        {
                            /// TODO
                            break;
                        }
                    case DxfEntityType.Leader:
                        break;
                    case DxfEntityType.Light:
                        break;
                    case DxfEntityType.Line:
                        {
                            /// TODO
                            break;
                        }
                    case DxfEntityType.LwPolyline:
                        {
                            /// TODO
                            break;
                        }
                    case DxfEntityType.MLine:
                        break;
                    case DxfEntityType.ModelerGeometry:
                        break;
                    case DxfEntityType.MText:
                        break;
                    case DxfEntityType.Ole2Frame:
                        break;
                    case DxfEntityType.OleFrame:
                        break;
                    case DxfEntityType.PdfUnderlay:
                        break;
                    case DxfEntityType.Point:
                        break;
                    case DxfEntityType.Polyline:
                        {
                            /// TODO
                            break;
                        }
                    case DxfEntityType.ProxyEntity:
                        break;
                    case DxfEntityType.Ray:
                        break;
                    case DxfEntityType.Region:
                        break;
                    case DxfEntityType.RText:
                        break;
                    case DxfEntityType.Section:
                        break;
                    case DxfEntityType.Seqend:
                        break;
                    case DxfEntityType.Shape:
                        break;
                    case DxfEntityType.Solid:
                        break;
                    case DxfEntityType.Spline:
                        break;
                    case DxfEntityType.Text:
                        break;
                    case DxfEntityType.Tolerance:
                        break;
                    case DxfEntityType.Trace:
                        break;
                    case DxfEntityType.Underlay:
                        break;
                    case DxfEntityType.Vertex:
                        break;
                    case DxfEntityType.WipeOut:
                        break;
                    case DxfEntityType.XLine:
                        break;
                    default:
                        {
                            break;
                        }
                }


            }

        }

    }
}
