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

namespace ShPilot2.UI.Graphics3D
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
                    double lineEntityP1YNew = Math.Sin(inRotationAngleRad) * (lineEntityP1X - inRotationCenterX) - Math.Cos(inRotationAngleRad) * (lineEntityP1Y - inRotationCenterY) + inRotationCenterY;
                    double lineEntityP2XNew = Math.Cos(inRotationAngleRad) * (lineEntityP2X - inRotationCenterX) - Math.Sin(inRotationAngleRad) * (lineEntityP2Y - inRotationCenterY) + inRotationCenterX;
                    double lineEntityP2YNew = Math.Sin(inRotationAngleRad) * (lineEntityP2X - inRotationCenterX) - Math.Cos(inRotationAngleRad) * (lineEntityP2Y - inRotationCenterY) + inRotationCenterY;
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
                    double arcCenterYNew = Math.Sin(inRotationAngleRad) * (arcCenterX - inRotationCenterX) - Math.Cos(inRotationAngleRad) * (arcCenterY - inRotationCenterY) + inRotationCenterY;
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
                    double circleCenterYNew = Math.Sin(inRotationAngleRad) * (circleCenterX - inRotationCenterX) - Math.Cos(inRotationAngleRad) * (circleCenterY - inRotationCenterY) + inRotationCenterY;
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

        /// <summary>
        /// BEWARE! MATH AND TRIGONOMETRY RIGHT AHEAD!
        /// </summary>
        /// <param name="inFname">file to parse</param>
        /// <param name="angle">angle to turn</param>
        /// <param name="mirror">do we need to perform mirror of profile vertically</param>
        
        /// <param name="out_thecurrentBox2">final bound box of dxf as it is rendered on scene (Helix Toolkit does not calculate it well for me)</param>
        /// <param name="out_offsetX">offset of DXF as calculated here</param>
        /// <param name="out_OffsetY">offset of DXF as calculated here</param>
        public static Canvas parseAndRenderDXF(String inFname, double angle, bool mirror, out BoundBox out_thecurrentBox2, out double out_offsetX, out double out_OffsetY)
        {

            Canvas pmb = new Canvas();

            DxfFile dxfFile = DxfFile.Load(inFname);
            BoundBox thecurrentBox = getDisplacementOfDxf(dxfFile, false, 0, 0, 0);
            BoundBox rotatedBox = null;
            double offsetX = 0; double offsetY = 0;
            if ((thecurrentBox.upperLeft.X != 0) || (thecurrentBox.bottomRight.Y != 0))
            {
                offsetX = -thecurrentBox.upperLeft.X; offsetY = -thecurrentBox.bottomRight.Y;
            }
            // calculate bound box for rotated figure
            double midPointHorizontal = (thecurrentBox.upperLeft.X + thecurrentBox.bottomRight.X) / 2.0;
            double midPointVertical = (thecurrentBox.upperLeft.Y + thecurrentBox.bottomRight.Y) / 2.0;
            rotatedBox = getDisplacementOfDxf(dxfFile, true, midPointHorizontal, midPointVertical, ProfileConstructor2D.ConvertDegreesToRadians(angle));

            void handleLine(DxfLine line)
            {
                Line lineLine = new Line();
                lineLine.X1 = line.P1.X;
                lineLine.Y1 = line.P1.Y;
                lineLine.X2 = line.P2.X; 
                lineLine.Y2 = line.P2.Y;
                lineLine.Stroke = Brushes.Red; 
                pmb.Children.Add(lineLine);
            }

            void handleArc(DxfArc arc)
            {
                double arcCenterX = arc.Center.X;
                double arcCenterY = arc.Center.Y;
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


            if (angle != 0)
            {
                /*
                Matrix3D mm = mdldxf.Transform.Value;
                // Quaternion constructor accepts angle in degrees. seems like it rotates clockwise, but in vb6 rotation is counterclockwise
                mm.RotateAt(new Quaternion(new Vector3D(0, 1, 0), angle), new Point3D(
                    midPointHorizontal,
                    (OnePlane + AnotherPlane) / 2.0,
                    midPointVertical
                    ));
                mdldxf.Transform = new MatrixTransform3D(mm);
                */
                if (rotatedBox != null)
                {
                    double offsetX2 = -rotatedBox.upperLeft.X; double offsetY2 = -rotatedBox.bottomRight.Y;
                    Matrix3D mm2 = mdldxf.Transform.Value;
                    mm2.Translate(new Vector3D(offsetX2, 0, offsetY2));
                    mdldxf.Transform = new MatrixTransform3D(mm2);
                }
            }
            else
                if ((thecurrentBox.upperLeft.X != 0) || (thecurrentBox.bottomRight.Y != 0))
            {
                Matrix3D mm = mdldxf.Transform.Value;
                mm.Translate(new Vector3D(offsetX, 0, offsetY));
                mdldxf.Transform = new MatrixTransform3D(mm);
            }

            // display bound box
            BoundBox thecurrentBox2 = thecurrentBox;
            if ((rotatedBox != null) && (angle != 0))
            {
                thecurrentBox2 = rotatedBox;
            }

            if ((thecurrentBox2.upperLeft.X != 0) || (thecurrentBox2.bottomRight.Y != 0))
            {
                offsetX = -thecurrentBox2.upperLeft.X; offsetY = -thecurrentBox2.bottomRight.Y;
            }
            //hey, what about mirroring?
            // sorry, I kinda messed with adding vertices while defining figure in 3D space, so everytime I am now going to mirror figure. 
            // apparently in vb 6 coordinate system goes to left. in Autocad coordinate system goes to right. But here user can rotate view of coordinate system however he wants
            //When there is mirror declaration then no mirroring is needed, we just change viewpoint
            // I.... got lost....
            if (mirror == false)
            {
                Matrix3D mm3 = mdldxf.Transform.Value;
                mm3.ScaleAt(new Vector3D(-1, 1, 1), new Point3D(
                    (thecurrentBox.bottomRight.X + offsetX + thecurrentBox.upperLeft.X + offsetX) / 2.0,
                    (OnePlane + AnotherPlane) / 2.0,
                    (thecurrentBox.bottomRight.Y + offsetY + thecurrentBox.upperLeft.Y + offsetY) / 2.0
                    ));
                mdldxf.Transform = new MatrixTransform3D(mm3);
            }
            out_thecurrentBox2 = thecurrentBox2;
            out_offsetX = offsetX;
            out_OffsetY = offsetY;
            return mdldxf;

            

        }

    }
}
