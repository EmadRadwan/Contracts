import React, {useEffect, useState} from 'react';
import {Button, CircularProgress, Grid, Typography} from '@mui/material';
import FileWidgetDropzone from './FileWidgetDropzone';

interface Props {
    loading: boolean;
    uploadFile: (file: Blob) => void;
}

export default function FileUploadWidget({loading, uploadFile}: Props) {
    const [files, setFiles] = useState<any>([]);

    function localUploadFile() {
        uploadFile(files[0]);
    }

    useEffect(() => {
        return () => {
            files.forEach((file: any) => URL.revokeObjectURL(file.preview));
        };
    }, [files]);

    return (
        <Grid container spacing={3}>
            <Grid item xs={12}>
                <Typography variant="h6" color="primary">
                    Add File
                </Typography>
                <FileWidgetDropzone files={files} setFiles={setFiles}/>
            </Grid>
            <Grid item xs={12}>
                {files && files.length > 0 && (
                    <div style={{display: 'flex', gap: '10px'}}>
                        <Button
                            variant="contained"
                            color="primary"
                            onClick={localUploadFile}
                            size="small"
                            startIcon={loading ? <CircularProgress size={20}/> : null}
                            disabled={loading}
                        >
                            Upload
                        </Button>
                        <Button
                            variant="contained"
                            color="secondary"
                            onClick={() => setFiles([])}
                            size="small"
                            disabled={loading}
                        >
                            Cancel
                        </Button>
                    </div>
                )}
            </Grid>
        </Grid>
    );
}
