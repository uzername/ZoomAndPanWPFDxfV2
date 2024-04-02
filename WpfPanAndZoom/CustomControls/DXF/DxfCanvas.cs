using IxMilia.Dxf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfPanAndZoom.CustomControls.DXF
{
    /// <summary>
    /// Control that contains all of the DXF related contours
    /// </summary>
    public class DxfCanvas: Canvas {
        DxfFile dxfFileCurrent = null;
        List<RenderFigureCommon> renderFigures = null;
        public void N1_PreloadDxfFile(String inFname)
        {            
                dxfFileCurrent = DxfFile.Load(inFname);            
        }
        public void N2_prepareRenderFiguresForCurrentDxfFile()
        {

        }
    }
}
