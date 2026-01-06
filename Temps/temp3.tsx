import { SpecialZoomLevel } from '@react-pdf-viewer/core';

// ...

<Viewer
    fileUrl={pdfBlobUrl}
    plugins={[defaultLayoutPluginInstance]}
    defaultScale={SpecialZoomLevel.PageFit}  // <-- Add this
/>