// pdfSetup.ts
import { Font } from '@react-pdf/renderer';

// REFACTOR: Register DejaVuSans font
// Purpose: Replaces previous font with DejaVuSans to provide an open-source alternative with Arabic support
// Improvement: Ensures legal distribution and robust RTL rendering for mixed content, addressing potential licensing or corruption issues
Font.register({
    family: 'DejaVuSans',
    fonts: [
        { src: '/fonts/DejaVuSans.ttf', fontWeight: 'normal' },              // Regular weight
        { src: '/fonts/DejaVuSans-Bold.ttf', fontWeight: 'bold' },          // Bold weight
        { src: '/fonts/DejaVuSans-Oblique.ttf', fontWeight: 'normal', fontStyle: 'italic' }, // Italic weight
        { src: '/fonts/DejaVuSans-BoldOblique.ttf', fontWeight: 'bold', fontStyle: 'italic' }, // Bold Italic weight
    ],
});