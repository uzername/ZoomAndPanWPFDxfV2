using IxMilia.Dxf.Entities;
using IxMilia.Dxf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using WpfPanAndZoom.CustomControls.DXF;

namespace WpfPanAndZoom.CustomControls.DXF
{
    /// <summary>
    /// Bound Box of profile in 2D.
    /// May be moved somewhere else if I port here my control for displaying graphics
    /// </summary>
    public class BoundBox
    {
        public Point upperLeft = new Point(0, 0);
        public Point bottomRight = new Point(0, 0);
        public BoundBox()
        {
            upperLeft = new Point(0, 0);
            bottomRight = new Point(0, 0);
        }
    }
    /// <summary>
    /// a mostly static class that generates Model3D objects of profile to render
    /// </summary>
    public class ProfileConstructor2D
    {
        public const String strCanvasName = "DXFCANVAS";
        // names of ignored layers. should be lowercase, lol
        public static List<String> ignoredLayers = new List<string> { "dxffix", "ext prof" };
        public static double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }



        // I want to align dxf profile somehow to axes. I find out bounding box of dxf file
        // I also use here values for rotation
        // inRotationAngleRad is from 0 to 2PI . It does not matter though
        /* https://stackoverflow.com/questions/2259476/rotating-a-point-about-another-point-2d
         * If you rotate point (px, py) around point (ox, oy) by angle theta you'll get:
            p'x = cos(theta) * (px-ox) - sin(theta) * (py-oy) + ox
            p'y = sin(theta) * (px-ox) + cos(theta) * (py-oy) + oy
         */
        public static BoundBox getDisplacementOfDxf(DxfFile inObtainedStructure, bool calculateRotation, double inRotationCenterX, double inRotationCenterY, double inRotationAngleRad)
        {
            bool isFirstEstimation = true;
            bool gotInside = false;
            // bounding box total
            BoundBox retStruct = new BoundBox();
            BoundBox currentStruct = new BoundBox();
            void handleLineDimensions(DxfLine lineEntity)
            {
                double lineEntityP1X = lineEntity.P1.X;
                double lineEntityP2X = lineEntity.P2.X;
                double lineEntityP1Y = lineEntity.P1.Y;
                double lineEntityP2Y = lineEntity.P2.Y;
                if (calculateRotation)
                {
                    double lineEntityP1XNew = Math.Cos(inRotationAngleRad) * (lineEntityP1X - inRotationCenterX) - Math.Sin(inRotationAngleRad) * (lineEntityP1Y - inRotationCenterY) + inRotationCenterX;
                    double lineEntityP1YNew = Math.Sin(inRotationAngleRad) * (lineEntityP1X - inRotationCenterX) + Math.Cos(inRotationAngleRad) * (lineEntityP1Y - inRotationCenterY) + inRotationCenterY;
                    double lineEntityP2XNew = Math.Cos(inRotationAngleRad) * (lineEntityP2X - inRotationCenterX) - Math.Sin(inRotationAngleRad) * (lineEntityP2Y - inRotationCenterY) + inRotationCenterX;
                    double lineEntityP2YNew = Math.Sin(inRotationAngleRad) * (lineEntityP2X - inRotationCenterX) + Math.Cos(inRotationAngleRad) * (lineEntityP2Y - inRotationCenterY) + inRotationCenterY;
                    lineEntityP1X = lineEntityP1XNew;
                    lineEntityP2X = lineEntityP2XNew;
                    lineEntityP1Y = lineEntityP1YNew;
                    lineEntityP2Y = lineEntityP2YNew;
                }
                currentStruct.bottomRight.X = Math.Max(lineEntityP1X, lineEntityP2X);
                currentStruct.bottomRight.Y = Math.Min(lineEntityP1Y, lineEntityP2Y);
                currentStruct.upperLeft.X = Math.Min(lineEntityP1X, lineEntityP2X);
                currentStruct.upperLeft.Y = Math.Max(lineEntityP1Y, lineEntityP2Y);
            }
            void handleArcDimensions(DxfArc arc)
            {
                double arcCenterX = arc.Center.X;
                double arcCenterY = arc.Center.Y;
                //estimation of arc bound box is going to be fun
                double radAngleStart = ProfileConstructor2D.ConvertDegreesToRadians(arc.StartAngle);
                double radAngleEnd = ProfileConstructor2D.ConvertDegreesToRadians(arc.EndAngle);
                if (radAngleStart > radAngleEnd)
                {
                    radAngleEnd += Math.PI * 2;
                }
                if (calculateRotation)
                {
                    double arcCenterXNew = Math.Cos(inRotationAngleRad) * (arcCenterX - inRotationCenterX) - Math.Sin(inRotationAngleRad) * (arcCenterY - inRotationCenterY) + inRotationCenterX;
                    double arcCenterYNew = Math.Sin(inRotationAngleRad) * (arcCenterX - inRotationCenterX) + Math.Cos(inRotationAngleRad) * (arcCenterY - inRotationCenterY) + inRotationCenterY;
                    radAngleEnd += inRotationAngleRad;
                    radAngleStart += inRotationAngleRad;
                    arcCenterX = arcCenterXNew;
                    arcCenterY = arcCenterYNew;
                }
                double startPointXcoordinate = arcCenterX + Math.Cos(radAngleStart) * arc.Radius;
                double startPointYcoordinate = arcCenterY + Math.Sin(radAngleStart) * arc.Radius;
                double endPointXcoordinate = arcCenterX + Math.Cos(radAngleEnd) * arc.Radius;
                double endPointYcoordinate = arcCenterY + Math.Sin(radAngleEnd) * arc.Radius;

                currentStruct.bottomRight.X = Math.Max(startPointXcoordinate, endPointXcoordinate);
                currentStruct.bottomRight.Y = Math.Min(startPointYcoordinate, endPointYcoordinate);
                currentStruct.upperLeft.X = Math.Min(startPointXcoordinate, endPointXcoordinate);
                currentStruct.upperLeft.Y = Math.Max(startPointYcoordinate, endPointYcoordinate);
                double angleCounter = 0;
                //iterate in steps of pi/2 from 0 to 4*pi
                while (angleCounter <= Math.PI * 4.0)
                {
                    if ((angleCounter >= radAngleStart) && (angleCounter <= radAngleEnd))
                    {
                        if ((angleCounter == 0) || (angleCounter == (2 * Math.PI)) || (angleCounter == (4 * Math.PI)))
                        {
                            currentStruct.bottomRight.X = Math.Max(currentStruct.bottomRight.X, arcCenterX + arc.Radius);

                        }
                        else if ((angleCounter == (3 * Math.PI / 2.0)) || (angleCounter == (11 * Math.PI / 2.0)))
                        {
                            currentStruct.bottomRight.Y = Math.Min(currentStruct.bottomRight.Y, arcCenterY - arc.Radius);
                        }
                        else if ((angleCounter == Math.PI) || (angleCounter == 3 * Math.PI))
                        {
                            currentStruct.upperLeft.X = Math.Min(currentStruct.upperLeft.X, arcCenterX - arc.Radius);
                        }
                        else if ((angleCounter == (Math.PI / 2.0)) || (angleCounter == (5 * Math.PI / 2.0)))
                        {
                            currentStruct.upperLeft.Y = Math.Max(currentStruct.upperLeft.Y, arcCenterY + arc.Radius);
                        }


                    }
                    if (angleCounter >= radAngleEnd)
                    {
                        break;
                    }
                    angleCounter += Math.PI / 2.0;
                }
            }
            void handleCircleDimensions(DxfCircle circle)
            {
                double circleCenterX = circle.Center.X;
                double circleCenterY = circle.Center.Y;
                double circleRadius = circle.Radius;
                if (calculateRotation)
                {
                    double circleCenterXNew = Math.Cos(inRotationAngleRad) * (circleCenterX - inRotationCenterX) - Math.Sin(inRotationAngleRad) * (circleCenterY - inRotationCenterY) + inRotationCenterX;
                    double circleCenterYNew = Math.Sin(inRotationAngleRad) * (circleCenterX - inRotationCenterX) + Math.Cos(inRotationAngleRad) * (circleCenterY - inRotationCenterY) + inRotationCenterY;
                    circleCenterX = circleCenterXNew;
                    circleCenterY = circleCenterYNew;
                }
                currentStruct.bottomRight.X = circleCenterX + circleRadius;
                currentStruct.bottomRight.Y = circleCenterY - circleRadius;
                currentStruct.upperLeft.X = circleCenterX - circleRadius;
                currentStruct.upperLeft.Y = circleCenterY + circleRadius;
            }
            void handlePolylineDimensions(List<DxfEntity> polylineEntities)
            {
                foreach (DxfEntity entity in polylineEntities)
                {
                    currentStruct = new BoundBox();
                    switch (entity.EntityType)
                    {
                        case DxfEntityType.Line:
                            {
                                if (gotInside == false) { gotInside = true; }
                                DxfLine lineEntity = entity as DxfLine;
                                handleLineDimensions(lineEntity);
                                break;
                            }
                        case DxfEntityType.Arc:
                            {
                                if (gotInside == false) { gotInside = true; }
                                DxfArc arc = (DxfArc)entity;
                                handleArcDimensions(arc);
                                break;
                            }
                        case DxfEntityType.Circle:
                            {
                                if (gotInside == false) { gotInside = true; }
                                DxfCircle circle = (DxfCircle)entity;
                                handleCircleDimensions(circle);
                                break;
                            }
                        case DxfEntityType.Polyline:
                        case DxfEntityType.LwPolyline:
                            {
                                if (gotInside == false)
                                {
                                    gotInside = true;
                                }
                                List<DxfEntity> obtainedEntitiesFromPolyline = new List<DxfEntity>();
                                if (entity is DxfPolyline)
                                {
                                    obtainedEntitiesFromPolyline = (entity as DxfPolyline).AsSimpleEntities().ToList();
                                }
                                else if (entity is DxfLwPolyline)
                                {
                                    obtainedEntitiesFromPolyline = (entity as DxfLwPolyline).AsSimpleEntities().ToList();
                                }

                                break;
                            }
                    }
                    if (gotInside)
                    {
                        if (isFirstEstimation)
                        {
                            retStruct.bottomRight.X = currentStruct.bottomRight.X;
                            retStruct.bottomRight.Y = currentStruct.bottomRight.Y;
                            retStruct.upperLeft.X = currentStruct.upperLeft.X;
                            retStruct.upperLeft.Y = currentStruct.upperLeft.Y;
                            isFirstEstimation = false;
                        }
                        else
                        {
                            retStruct.bottomRight.X = Math.Max(currentStruct.bottomRight.X, retStruct.bottomRight.X);
                            retStruct.bottomRight.Y = Math.Min(currentStruct.bottomRight.Y, retStruct.bottomRight.Y);
                            retStruct.upperLeft.X = Math.Min(currentStruct.upperLeft.X, retStruct.upperLeft.X);
                            retStruct.upperLeft.Y = Math.Max(currentStruct.upperLeft.Y, retStruct.upperLeft.Y);
                        }
                        gotInside = false;
                    }
                }
            }
            foreach (DxfEntity entity in inObtainedStructure.Entities)
            {
                if (ProfileConstructor2D.ignoredLayers.Contains(entity.Layer.ToLower()) == false)
                {
                    currentStruct = new BoundBox();
                    switch (entity.EntityType)
                    {
                        case DxfEntityType.Line:
                            {
                                if (gotInside == false)
                                {
                                    gotInside = true;
                                }
                                DxfLine lineEntity = entity as DxfLine;
                                handleLineDimensions(lineEntity);
                                break;
                            }
                        case DxfEntityType.Arc:
                            {
                                if (gotInside == false)
                                {
                                    gotInside = true;
                                }
                                DxfArc arc = (DxfArc)entity;

                                handleArcDimensions(arc);

                                break;
                            }
                        case DxfEntityType.Circle:
                            {
                                if (gotInside == false)
                                {
                                    gotInside = true;
                                }
                                DxfCircle circle = (DxfCircle)entity;
                                handleCircleDimensions(circle);
                                break;
                            }
                        case DxfEntityType.Polyline:
                        case DxfEntityType.LwPolyline:
                            {
                                if (gotInside == false)
                                {
                                    gotInside = true;
                                }
                                List<DxfEntity> obtainedEntitiesFromPolyline = new List<DxfEntity>();
                                if (entity is DxfPolyline)
                                {
                                    obtainedEntitiesFromPolyline = (entity as DxfPolyline).AsSimpleEntities().ToList();
                                }
                                else if (entity is DxfLwPolyline)
                                {
                                    obtainedEntitiesFromPolyline = (entity as DxfLwPolyline).AsSimpleEntities().ToList();
                                }
                                handlePolylineDimensions(obtainedEntitiesFromPolyline);
                                break;
                            }
                    }
                    if (gotInside)
                    {
                        if (isFirstEstimation)
                        {
                            retStruct.bottomRight.X = currentStruct.bottomRight.X;
                            retStruct.bottomRight.Y = currentStruct.bottomRight.Y;
                            retStruct.upperLeft.X = currentStruct.upperLeft.X;
                            retStruct.upperLeft.Y = currentStruct.upperLeft.Y;
                            isFirstEstimation = false;
                        }
                        else
                        {
                            retStruct.bottomRight.X = Math.Max(currentStruct.bottomRight.X, retStruct.bottomRight.X);
                            retStruct.bottomRight.Y = Math.Min(currentStruct.bottomRight.Y, retStruct.bottomRight.Y);
                            retStruct.upperLeft.X = Math.Min(currentStruct.upperLeft.X, retStruct.upperLeft.X);
                            retStruct.upperLeft.Y = Math.Max(currentStruct.upperLeft.Y, retStruct.upperLeft.Y);
                        }
                        gotInside = false;
                    }
                }
            }
            return retStruct;
        }

        private double MirrorPointByVerticalLine(double in_lineEntityP1X, double in_midPointHorizontal)
        {
            double out_lineEntityP1X = in_lineEntityP1X;
            if (in_lineEntityP1X < in_midPointHorizontal)
            {
                out_lineEntityP1X = in_lineEntityP1X + 2 * Math.Abs(in_lineEntityP1X - in_midPointHorizontal);
            }
            else if (in_lineEntityP1X > in_midPointHorizontal)
            {
                out_lineEntityP1X = in_lineEntityP1X - 2 * Math.Abs(in_lineEntityP1X - in_midPointHorizontal);
            }
            return out_lineEntityP1X;
        }

        // https://stackoverflow.com/a/60580020/5128696
        private double MirrorAngleByVerticalLine(double angleRad)
        {
            double mirrored_Angle = Math.PI - angleRad;
            if (mirrored_Angle < 0)
                mirrored_Angle = 2 * Math.PI + mirrored_Angle;

            return mirrored_Angle;
        }

        /// <summary>
        /// BEWARE! MATH AND TRIGONOMETRY RIGHT AHEAD! 
        /// due to specifics of WPF, mirroring and turning is to be applied later, to items inside the canvas. Items inside canvas are rendered straight
        /// </summary>
        /// <param name="inFname">file to parse</param>

        /// <param name="out_thecurrentBox2">final bound box of dxf as it is rendered on scene</param>
        /// <param name="out_offsetX">offset of DXF as calculated here</param>
        /// <param name="out_OffsetY">offset of DXF as calculated here</param>
        /// <returns>Canvas with all added items but not mirrored and not turned</returns>
        public static Canvas parseAndRenderDXF(String inFname,  out BoundBox out_thecurrentBox2, out double out_offsetX, out double out_OffsetY)
        {
            bool useBorders = true;
            Canvas pmb = new Canvas();

            DxfFile dxfFile = DxfFile.Load(inFname);
            BoundBox thecurrentBox = getDisplacementOfDxf(dxfFile, false, 0, 0, 0);
            double offsetX = 0; double offsetY = 0;
            if ((thecurrentBox.upperLeft.X != 0) || (thecurrentBox.bottomRight.Y != 0))
            {
                offsetX = -thecurrentBox.upperLeft.X; offsetY = -thecurrentBox.bottomRight.Y;
            }
            double midPointHorizontal = (thecurrentBox.upperLeft.X + thecurrentBox.bottomRight.X) / 2.0;
            double midPointVertical = (thecurrentBox.upperLeft.Y + thecurrentBox.bottomRight.Y) / 2.0;
            

            void handleLine(DxfLine line)
            {
                Line lineLine = new Line();
                lineLine.X1 = line.P1.X+offsetX;
                lineLine.Y1 = line.P1.Y+offsetY;
                lineLine.X2 = line.P2.X+offsetX; 
                lineLine.Y2 = line.P2.Y+offsetY;
                lineLine.Stroke = Brushes.Red; 
                pmb.Children.Add(lineLine);
            }

            void handleArc(DxfArc arc)
            {
                double arcCenterX = arc.Center.X+offsetX;
                double arcCenterY = arc.Center.Y+offsetY;
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
                arcGraphic.Center = new Point(correctedXCenter, correctedYCenter);
                arcGraphic.Stroke = Brushes.Red;
                pmb.Children.Add(arcGraphic);

            }
            BoundBox thecurrentBox2 = thecurrentBox;

            if ((thecurrentBox2.upperLeft.X != 0) || (thecurrentBox2.bottomRight.Y != 0))
            {
                offsetX = -thecurrentBox2.upperLeft.X; offsetY = -thecurrentBox2.bottomRight.Y;
            }
            // render code
            foreach (DxfEntity entity in dxfFile.Entities)
            {
                if (ProfileConstructor2D.ignoredLayers.Contains(entity.Layer.ToLower()) == false)
                {
                    switch (entity.EntityType)
                    {

                        case DxfEntityType.Line:
                            {
                                DxfLine line = (DxfLine)entity;
                                handleLine(line);
                                break;
                            }
                        case DxfEntityType.Arc:
                            {
                                DxfArc arc = (DxfArc)entity;
                                handleArc(arc);

                                break;
                            }
                        case DxfEntityType.Circle:
                            {
                                DxfCircle circle = (DxfCircle)entity;
                                double circleCenterX = circle.Center.X;
                                double circleCenterY = circle.Center.Y;
                                double circleRadius = circle.Radius;                                        
                                // TODO add circle
                                break;
                            }
                        case DxfEntityType.LwPolyline:
                        case DxfEntityType.Polyline:
                            {
                                List<DxfEntity> theInternalsOfPolyLine = new List<DxfEntity>();
                                if (entity is DxfLwPolyline)
                                {
                                    theInternalsOfPolyLine = new List<DxfEntity>((entity as DxfLwPolyline).AsSimpleEntities());
                                }
                                else if (entity is DxfPolyline)
                                {
                                    theInternalsOfPolyLine = new List<DxfEntity>(((entity as DxfPolyline).AsSimpleEntities()));
                                }
                                foreach (var itemOfPolyLine in theInternalsOfPolyLine)
                                {
                                    switch (itemOfPolyLine.EntityType)
                                    {
                                        case (DxfEntityType.Line):
                                            {
                                                handleLine(itemOfPolyLine as DxfLine);
                                                break;
                                            }
                                        case (DxfEntityType.Arc):
                                            {
                                                handleArc(itemOfPolyLine as DxfArc);
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                }
                                break;
                            }
                    }
                }
            }
            
            

            out_thecurrentBox2 = thecurrentBox2;
            out_offsetX = offsetX;
            out_OffsetY = offsetY;
            pmb.Width = Math.Abs(thecurrentBox2.bottomRight.X - thecurrentBox2.upperLeft.X);
            pmb.Height= Math.Abs(thecurrentBox2.bottomRight.Y - thecurrentBox2.upperLeft.Y);
            pmb.Name = strCanvasName;
            // borders
            if (useBorders)
            {
                Line l1 = new Line();
                l1.X1 = 0;
                l1.Y1 = 0;
                l1.X2 = pmb.Width;
                l1.Y2 = 0;
                l1.Stroke = Brushes.Green;
                pmb.Children.Add(l1);
                Line l2 = new Line();
                l2.X1 = 0;
                l2.Y1 = 0;
                l2.X2 = 0;
                l2.Y2 = pmb.Height;
                l2.Stroke = Brushes.Green;
                pmb.Children.Add(l2);
            }
            return pmb;

            

        }

    }
}
