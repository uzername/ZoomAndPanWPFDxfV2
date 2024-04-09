# WpfPanAndZoom

A panning and zooming canvas for WPF to view DXF files. Based on: [SEilers/WpfPanAndZoom](https://github.com/SEilers/WpfPanAndZoom) and uses ixmilia/DXF

In latest version it supports also display with mirroring and rotation applied

## How to use it - DOCUMENTATION

In recent version most processing is done in DXFCanvas.cs but ProfileConstructor2D is also used. DXFCanvas subclasses Canvas class and contains graphical elements of rendered dxf file. To reduce number of file calls I parse dxf file once and then I dynamically add elements to DXFCanvas and specify bindings to List that contains elements of RenderFigureCommon class.

At first you should add in XAML an instance of PanAndZoomCanvas to achieve that panning and zooming capability. Like that:

```
<PanAndZoomCanvas x:Name="canvas" ClipToBounds="True" >   
</PanAndZoomCanvas>
```

Then in code you add actual DXFCanvas instance as child, Pan And Zoom Canvas will contain DXF Canvas, right.

```
DXFCanvas canvasToShow2 = new DXFCanvas();
canvas.Children.Add(canvasToShow2);
```

If you are opening a dxf file for the first time to show, then you should call methods of DXFCanvas:

+ Start with removing possible figures from dxf canvas `N0_CleanupCanvas` and then use `resetTransform` of Pan And Zoom Canvas (it is important)
+ `N1_PreloadDxfFile` method to parse DXF file
+ `N2_modifyRenderFiguresForDxfFile` method to apply mirroring then apply turning angle. Set third parameter to True - as we are going to init bindings of graphical figures and re-create them (boolean parameter should be True).
+ call `highlightRectangleAreaToDisplay` of Pan And Zoom Canvas to focus on specific area

If you are going to adjust mirroring and angle of dxf file that has already been parsed then check 'renderParsedFile' from MainWindow.xaml.cs

+ DO NOT call cleanup here, all figures remain the same, just need to reposition them, and recalculate bound box as well.
+ call `N2_modifyRenderFiguresForDxfFile` with third parameter set to False, because I am going to recalculate coordinates in list of RenderFigureCommon elements which are bound to graphical elements, instead of recreating them (this improves speed of processing)
+ It is required to properly position of DXFCanvas inside parent element, so I refer to bound box of figure before turning and after turning and then move DXFCanvas to up by difference.

picture:

![canvasclip](https://github.com/uzername/ZoomAndPanWPFDxfV2/blob/master/imageR/Animation2.gif)
