using IxMilia.Dxf.Entities;
using IxMilia.Dxf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using WpfPanAndZoom.CustomControls.DXF;

namespace WpfPanAndZoom.CustomControls
{
    public class DXFCanvas:Canvas
    {
        private DxfFile dxfFileCurrent = null;
        /// <summary>
        /// list which is bound to elements' coordinates contained inside this canvas
        /// </summary>
        public List<RenderFigureCommon> renderFigures { get; set; } = null;
        /// <summary>
        /// raw bound box of current dxf file. no mirror and no rotation
        /// </summary>
        public BoundBox thecurrentBox = null;
        /// <summary>
        /// bound box of current dxf file with mirror and rotation applied
        /// </summary>
        public BoundBox thetransformedBox = null;
        /// <summary>
        /// offset X of bound box : at first it is set to thecurrentBox then to thetransformedBox
        /// </summary>
        public double offsetX { get; private set; } = 0;
        /// <summary>
        /// offset Y of bound box : at first it is set to thecurrentBox then to thetransformedBox
        /// </summary>
        public double offsetY { get; private set; } = 0;

        public System.Windows.Media.Brush currentStroke = System.Windows.Media.Brushes.Red;
        private RenderFigureCommon getLineRenderFigure(DxfLine line, bool doMirroring, double AngleRad, double midPointH, double midPointV, double offsetX, double offsetY)
        {

            RenderFigureCommon rfcLine = new RenderFigureCommon();
            rfcLine.figureType = RenderFigureCommonType.LINE;
            double P1X = line.P1.X; double P1Y = line.P1.Y;
            double P2X = line.P2.X; double P2Y = line.P2.Y;
            if (doMirroring)
            {
                P1X = ProfileConstructor2D.MirrorPointByGuide(P1X, midPointH);
                P2X = ProfileConstructor2D.MirrorPointByGuide(P2X, midPointH);
            }
            double P1XNew = P1X; double P1YNew = P1Y;
            double P2XNew = P2X; double P2YNew = P2Y;
            ProfileConstructor2D.rotatePointAroundCenterPoint(out P1XNew, out P1YNew, P1X, P1Y, AngleRad, midPointH, midPointV);
            ProfileConstructor2D.rotatePointAroundCenterPoint(out P2XNew, out P2YNew, P2X, P2Y, AngleRad, midPointH, midPointV);
            P1X = P1XNew + offsetX; P1Y = P1YNew + offsetY;
            P2X = P2XNew + offsetX; P2Y = P2YNew + offsetY;
            rfcLine.LineX1 = P1X; rfcLine.LineY1 = P1Y;
            rfcLine.LineX2 = P2X; rfcLine.LineY2 = P2Y;
            return rfcLine;
        }

        private RenderFigureCommon getArcRenderFigure(DxfArc arc, bool doMirroring, double AngleRad, double midPointH, double midPointV, double offsetX, double offsetY)
        {
            RenderFigureCommon rfcArc = new RenderFigureCommon();
            rfcArc.figureType = RenderFigureCommonType.ARC;
            double arcCenterX = arc.Center.X;
            double arcCenterY = arc.Center.Y;
            double radAngleStart = ProfileConstructor2D.ConvertDegreesToRadians(arc.StartAngle);
            double radAngleEnd = ProfileConstructor2D.ConvertDegreesToRadians(arc.EndAngle);

            if (arc.Normal != DxfVector.ZAxis)
            {
                arcCenterX = -arcCenterX;
                radAngleStart = ProfileConstructor2D.MirrorAngleByGuide(radAngleStart);
                radAngleEnd = ProfileConstructor2D.MirrorAngleByGuide(radAngleEnd);
                // do I swap angles?
                double tempDecimal = radAngleStart;
                radAngleStart = radAngleEnd;
                radAngleEnd = tempDecimal;
            }

            if (radAngleStart > radAngleEnd)
            {
                radAngleEnd += Math.PI * 2;
            }
            if (doMirroring)
            {
                // mirror of arc
                double midPointHorizontal = midPointH;
                radAngleStart = ProfileConstructor2D.MirrorAngleByGuide(radAngleStart);
                radAngleEnd = ProfileConstructor2D.MirrorAngleByGuide(radAngleEnd);
                // swap?

                double tempDecimal = radAngleStart;
                radAngleStart = radAngleEnd;
                radAngleEnd = tempDecimal;

                if (radAngleStart > radAngleEnd)
                {
                    radAngleEnd += Math.PI * 2;
                }
                arcCenterX = ProfileConstructor2D.MirrorPointByGuide(arcCenterX, midPointHorizontal);
            }
            double arcCenterXNew = arcCenterX; double arcCenterYNew = arcCenterY;
            ProfileConstructor2D.rotatePointAroundCenterPoint(out arcCenterXNew, out arcCenterYNew, arcCenterX, arcCenterY, AngleRad, midPointH, midPointV);
            // at this point End Angle > Start Angle always (should be)
            radAngleEnd += AngleRad;
            radAngleStart += AngleRad;
            arcCenterX = arcCenterXNew + offsetX; arcCenterY = arcCenterYNew + offsetY;

            rfcArc.ArcCenter = new System.Windows.Point(arcCenterX, arcCenterY);
            rfcArc.ArcRadius = arc.Radius;
            rfcArc.ArcStartAngle = radAngleStart;
            rfcArc.ArcEndAngle = radAngleEnd;
            /*
            Arc arcGraphic = new Arc();
            double correctedXCenter = arcCenterX;
            double correctedYCenter = arcCenterY;
            arcGraphic.StartAngle = radAngleEnd;
            arcGraphic.EndAngle = radAngleStart;
            arcGraphic.Radius = arc.Radius;
            arcGraphic.Center = new System.Windows.Point(correctedXCenter, correctedYCenter);
            arcGraphic.Stroke = currentStroke;
            */
            return rfcArc;
        }
        /// <summary>
        /// global index in RenderFigures list
        /// </summary>
        private int globul_indx;
        private void setRenderFiguresList(List<DxfEntity> dxfFileCurrentEntities, bool mirror, double AngleRad, bool initRequired, double in_midpointH, double in_midpointV, double in_offsetX, double in_offsetY)
        {

            foreach (DxfEntity entity in dxfFileCurrentEntities)
            {
                if (ProfileConstructor2D.ignoredLayers.Contains(entity.Layer.ToLower()) == true) continue;
                switch (entity.EntityType)
                {
                    case DxfEntityType.Arc:
                        {
                            RenderFigureCommon rfcArc = getArcRenderFigure(entity as DxfArc, mirror, AngleRad, in_midpointH, in_midpointV, in_offsetX, in_offsetY);
                            if (initRequired)
                            {
                                renderFigures.Add(rfcArc);
                            }
                            else
                            {
                                renderFigures[globul_indx].assignArc(rfcArc.ArcStartAngle, rfcArc.ArcEndAngle, rfcArc.ArcCenter, rfcArc.ArcRadius);
                            }
                            globul_indx++;
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
                            // circle is divided in two arcs to simplify logic
                            double XCenterCircle = (entity as DxfCircle).Center.X;
                            double YCenterCircle = (entity as DxfCircle).Center.Y;
                            double ZCenterCircle = (entity as DxfCircle).Center.Z;
                            double RadiusCircle = (entity as DxfCircle).Radius;
                            DxfArc arc1= new DxfArc(new DxfPoint(XCenterCircle, YCenterCircle,ZCenterCircle), RadiusCircle,0,180);
                            DxfArc arc2 = new DxfArc(new DxfPoint(XCenterCircle, YCenterCircle, ZCenterCircle), RadiusCircle, 180, 359.99);
                            List<DxfEntity> lll = new List<DxfEntity> { arc1, arc2 };
                            setRenderFiguresList(lll, mirror, AngleRad, initRequired, in_midpointH, in_midpointV, in_offsetX, in_offsetY);
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
                            /// TODO BLOCK ENTRY
                            break;
                        }
                    case DxfEntityType.Leader:
                        break;
                    case DxfEntityType.Light:
                        break;
                    case DxfEntityType.Line:
                        {
                            RenderFigureCommon rfcLine = getLineRenderFigure(entity as DxfLine, mirror, AngleRad, in_midpointH, in_midpointV, in_offsetX, in_offsetY);
                            if (initRequired)
                            {
                                renderFigures.Add(rfcLine);
                            }
                            else
                            {
                                renderFigures[globul_indx].assignLine(rfcLine.LineX1, rfcLine.LineY1, rfcLine.LineX2, rfcLine.LineY2);
                            }
                            globul_indx++;
                            break;
                        }
                    case DxfEntityType.LwPolyline:
                        {
                            List<DxfEntity> entitiesPolyLine = (entity as DxfLwPolyline).AsSimpleEntities().ToList();
                            setRenderFiguresList(entitiesPolyLine, mirror, AngleRad, initRequired, in_midpointH, in_midpointV, in_offsetX, in_offsetY);
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
                            List<DxfEntity> entitiesPolyLine = (entity as DxfPolyline).AsSimpleEntities().ToList();
                            setRenderFiguresList(entitiesPolyLine, mirror, AngleRad, initRequired, in_midpointH, in_midpointV, in_offsetX, in_offsetY);
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
        public void N0_CleanupCanvas()
        {
            this.Children.Clear();            
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
            if ((dxfFileCurrent == null) || (thecurrentBox == null))
            {

            }
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
            this.Width = Math.Abs(thetransformedBox.upperLeft.X - thetransformedBox.bottomRight.X);
            this.Height = Math.Abs(thetransformedBox.upperLeft.Y - thetransformedBox.bottomRight.Y);
            offsetX = -thetransformedBox.upperLeft.X; offsetY = -thetransformedBox.bottomRight.Y;
            globul_indx = 0;
            setRenderFiguresList(dxfFileCurrent.Entities.ToList(), mirror, AngleRad, initRequired, midPointHorizontal, midPointVertical, offsetX, offsetY);
            if (initRequired)
            {
                // add items and setup bindings.
                // If init is not required then coordinates of child elements should be updated through binding mechanism
                // To bind to just one item from a collection https://stackoverflow.com/q/18659667/5128696
                // how to set binding in code https://stackoverflow.com/a/7525254/5128696
                globul_indx = 0;
                foreach (RenderFigureCommon itemRenderFigure in renderFigures)
                {
                    if (itemRenderFigure.figureType == RenderFigureCommonType.LINE)
                    {
                        Line lineLine = new Line();
                        lineLine.Stroke = currentStroke;
                        /*
                        lineLine.X1 = itemRenderFigure.LineX1;
                        lineLine.Y1 = itemRenderFigure.LineY1;
                        lineLine.X2 = itemRenderFigure.LineX2;
                        lineLine.Y2 = itemRenderFigure.LineY2;
                        */

                        Binding P1Xb = new Binding();
                        P1Xb.Source = this;
                        P1Xb.Path = new PropertyPath("renderFigures[" + globul_indx.ToString() + "].LineX1");
                        P1Xb.Mode = BindingMode.TwoWay;
                        P1Xb.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        BindingOperations.SetBinding(lineLine, Line.X1Property, P1Xb);

                        Binding P1Yb = new Binding();
                        P1Yb.Source = this;
                        P1Yb.Path = new PropertyPath("renderFigures[" + globul_indx.ToString() + "].LineY1");
                        P1Yb.Mode = BindingMode.TwoWay;
                        P1Yb.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        BindingOperations.SetBinding(lineLine, Line.Y1Property, P1Yb);

                        Binding P2Xb = new Binding();
                        P2Xb.Source = this;
                        P2Xb.Path = new PropertyPath("renderFigures[" + globul_indx.ToString() + "].LineX2");
                        P2Xb.Mode = BindingMode.TwoWay;
                        P2Xb.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        BindingOperations.SetBinding(lineLine, Line.X2Property, P2Xb);

                        Binding P2Yb = new Binding();
                        P2Yb.Source = this;
                        P2Yb.Path = new PropertyPath("renderFigures[" + globul_indx.ToString() + "].LineY2");
                        P2Yb.Mode = BindingMode.TwoWay;
                        P2Yb.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        BindingOperations.SetBinding(lineLine, Line.Y2Property, P2Yb);
                        this.Children.Add(lineLine);
                    }
                    else if (itemRenderFigure.figureType == RenderFigureCommonType.ARC)
                    {
                        Arc arcGraphic = new Arc();
                        arcGraphic.Stroke = currentStroke;
                        arcGraphic.Radius = itemRenderFigure.ArcRadius;
                        /*
                        arcGraphic.StartAngle = itemRenderFigure.ArcEndAngle;
                        arcGraphic.EndAngle = itemRenderFigure.ArcStartAngle;                        
                        arcGraphic.Center = itemRenderFigure.ArcCenter;
                        */
                        Binding ArcStartAngleBind = new Binding();
                        ArcStartAngleBind.Source = this;
                        ArcStartAngleBind.Path = new PropertyPath("renderFigures[" + globul_indx.ToString() + "].ArcEndAngle");
                        ArcStartAngleBind.Mode = BindingMode.TwoWay;
                        ArcStartAngleBind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        BindingOperations.SetBinding(arcGraphic, Arc.StartAngleProperty, ArcStartAngleBind);

                        Binding ArcEndAngleBind = new Binding();
                        ArcEndAngleBind.Source = this;
                        ArcEndAngleBind.Path = new PropertyPath("renderFigures[" + globul_indx.ToString() + "].ArcStartAngle");
                        ArcEndAngleBind.Mode = BindingMode.TwoWay;
                        ArcEndAngleBind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        BindingOperations.SetBinding(arcGraphic, Arc.EndAngleProperty, ArcEndAngleBind);

                        Binding ArcCenterBind = new Binding();
                        ArcCenterBind.Source = this;
                        ArcCenterBind.Path = new PropertyPath("renderFigures[" + globul_indx.ToString() + "].ArcCenter");
                        ArcCenterBind.Mode = BindingMode.TwoWay;
                        BindingOperations.SetBinding(arcGraphic, Arc.CenterProperty, ArcCenterBind);

                        this.Children.Add(arcGraphic);
                    }
                    globul_indx++;
                }
            }
        }
    }
}
