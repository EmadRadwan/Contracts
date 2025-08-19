import React, { useEffect, useState } from "react";
import mermaid from "mermaid";

interface MermaidChartProps {
    chart: string;
}

export default function MermaidChart({ chart }: MermaidChartProps) {
    const [svgContent, setSvgContent] = useState("");

    useEffect(() => {
        if (!chart) return;

        mermaid.initialize({
            startOnLoad: false,
            themeVariables: {
                fontSize: "18px",
                fontFamily: "Arial",
                primaryColor: "#686868",
            },
            flowchart: {
                useMaxWidth: false,
            },
            htmlLabels: true,
        });

        async function renderMermaid() {
            try {
                const { svg } = await mermaid.render("diagramId", chart);
                setSvgContent(svg);
            } catch (err: any) {
                setSvgContent(`Mermaid render error: ${err.message}`);
            }
        }
        renderMermaid();
    }, [chart]);

    return (
        <div
            style={{
                border: "1px solid #ccc",
                overflowX: "auto",   // Let it scroll horizontally if needed
                maxWidth: "100%",    // Ensure it doesn’t exceed its container
                height: "auto",      // Let it size vertically as needed
            }}
            dangerouslySetInnerHTML={{ __html: svgContent }}
        />
    );

}
