import {Card, CardActions, CardMedia, CircularProgress, Grid, Typography} from '@mui/material';
import React, {SyntheticEvent, useEffect, useState} from 'react';
import agent from '../../../app/api/agent';
import FileUploadWidget from '../../../app/common/imageUpload/FileUploadWidget';
import {VehicleContent} from '../../../app/models/content/vehicleContent';
import {Vehicle} from '../../../app/models/service/vehicle';
import Button from "@mui/material/Button";
import Dialog from '@mui/material/Dialog';

interface Props {
    vehicle: Vehicle | undefined;
    attachedFiles: VehicleContent[] | undefined;
}

const VehicleFiles: React.FC<Props> =
    ({vehicle, attachedFiles}) => {
        const [addFileMode, setAddFileMode] = useState(false);
        const [uploading, setUploading] = useState(false);
        const [loading, setLoading] = useState(false);
        const [target, setTarget] = useState('');
        const [files, setFiles] = useState<VehicleContent[]>([]);
        const [selectedImage, setSelectedImage] = useState<string | null>(null);
        const [imageLoading, setImageLoading] = useState<{ [key: string]: boolean }>({});
        const [contentIdForSelectedImage, setContentIdForSelectedImage] = useState<string | null>(null);

        useEffect(() => {
            setFiles(attachedFiles || []);
        }, [attachedFiles]);

        function handleFileUpload(file: Blob) {
            uploadFile(file).then(() => setAddFileMode(false));
        }

        const handleImageClick = (image: string, contentId: string) => {
            setSelectedImage(image);
            setContentIdForSelectedImage(contentId); // Set contentId here
        };


        const handleCloseDialog = () => {
            setSelectedImage(null);
            setContentIdForSelectedImage(null); // Reset contentId here
        };


        const uploadFile = async (file: Blob) => {
            try {
                if (vehicle) {
                    setUploading(true);
                    const response = await agent.VehicleContents.uploadFile(file, vehicle.vehicleId);
                    const vehicleContent = response.data;
                    setFiles([...files, vehicleContent]);
                }


            } catch (error) {
                console.log(error);
                setUploading(false);
            }
        }

        function handleDeleteFile(vehicleContent: VehicleContent, e: SyntheticEvent<HTMLButtonElement>) {
            setTarget(e.currentTarget.name);
            deleteFile(vehicleContent);
        }


        const deleteFile = async (file: VehicleContent) => {
            try {
                setLoading(true);
                await agent.VehicleContents.deleteFile(file.contentId);
                setFiles([...files.filter(p => p.contentId !== file.contentId)]);
            } catch (error) {
                console.log(error);
                setLoading(false);
            }
        }

        const styles = {
            media: {
                height: 0,
                paddingTop: '56.25%', // 16:9,
                marginTop: '30'
            }
        };

        const handleImageLoad = (contentId: string) => {
            setImageLoading((prevLoading) => ({
                ...prevLoading,
                [contentId]: false,
            }));
        };


        return (
            <Grid container spacing={3}>
                <Grid item xs={4}>
                    <Typography variant="h6" component="h2">
                        Attachments
                    </Typography>
                    <Button
                        variant="outlined"
                        onClick={() => setAddFileMode(!addFileMode)}
                    >
                        {addFileMode ? 'Cancel' : 'Add File'}
                    </Button>
                </Grid>
                <Grid item xs={12}>
                    {addFileMode ? (
                        <FileUploadWidget uploadFile={handleFileUpload} loading={uploading}/>
                    ) : (
                        <Grid container spacing={2}>
                            {files?.map((vehicleContent) => {
                                if (imageLoading[vehicleContent.contentId] === undefined) {
                                    setImageLoading((prevLoading) => ({
                                        ...prevLoading,
                                        [vehicleContent.contentId]: true,
                                    }));
                                }
                                return (
                                    <Grid item xs={3} key={vehicleContent.contentId}>
                                        <Card>
                                            <CardMedia
                                                style={{
                                                    ...styles.media,
                                                    position: 'relative'
                                                }}
                                            >
                                                <img
                                                    src={vehicleContent.objectInfo}
                                                    style={{
                                                        width: '100%',
                                                        height: '100%',
                                                        position: 'absolute',
                                                        top: 0,
                                                        left: 0
                                                    }}
                                                    onClick={() => handleImageClick(vehicleContent.objectInfo, vehicleContent.contentId)}
                                                    onLoad={() => handleImageLoad(vehicleContent.contentId)}
                                                    alt=""
                                                />
                                                {imageLoading[vehicleContent.contentId] && (
                                                    <CircularProgress
                                                        style={{
                                                            position: 'absolute',
                                                            top: '50%',
                                                            left: '50%',
                                                            transform: 'translate(-50%, -50%)',
                                                        }}
                                                    /> // CircularProgress centered
                                                )}
                                            </CardMedia>

                                            <CardActions>
                                                <Button
                                                    color="error"
                                                    variant="contained"
                                                    size="small"
                                                    onClick={(e) => handleDeleteFile(vehicleContent, e)}
                                                    disabled={target === vehicleContent.contentId && loading}
                                                >
                                                    Delete
                                                </Button>
                                            </CardActions>
                                        </Card>
                                    </Grid>

                                );
                            })}
                            <Dialog
                                open={selectedImage !== null}
                                onClose={handleCloseDialog}
                                maxWidth="md"
                            >
                                <img
                                    src={selectedImage || ''}
                                    alt="Enlarged"
                                    style={{width: '100%'}}
                                    onLoad={() => contentIdForSelectedImage && handleImageLoad(contentIdForSelectedImage)} // Check if contentIdForSelectedImage is not null
                                />
                            </Dialog>

                        </Grid>

                    )}
                </Grid>
            </Grid>
        );
    };

export default VehicleFiles;