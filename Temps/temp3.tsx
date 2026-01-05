/* Fix empty print preview in @react-pdf-viewer */
@media print {
    /* Force the viewer container to be visible and sized in print */
.rpv-core__inner-pages,
.rpv-core__inner-page {
        transform: none !important;
        position: static !important;
        page-break-inside: avoid;
    }

    /* Ensure canvases are printed */
    canvas {
        -webkit-print-color-adjust: exact !important;
        print-color-adjust: exact !important;
    }

    /* Hide toolbar & sidebar during print */
.rpv-toolbar,
.rpv-sidebar {
        display: none !important;
    }

    /* Full-width pages in print */
.rpv-core__viewer {
        width: 100% !important;
        height: auto !important;
    }
}