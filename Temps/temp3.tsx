<Worker workerUrl={`https://unpkg.com/pdfjs-dist@${pdfjsVersion}/build/pdf.worker.min.mjs`}>
    <Viewer
        fileUrl={pdfBlobUrl}
        plugins={[defaultLayoutPluginInstance]}
    />
</Worker>