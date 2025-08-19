import React, {useEffect, useState} from "react";
import carDiagramImage from "../../../assets/car-diagram2.jpg";
import {Vehicle} from "../../../app/models/service/vehicle";
import {useFetchVehicleAnnotationsQuery} from "../../../app/store/apis";
import Annotation from "./Annotation";
import {DragAndDrop} from "@progress/kendo-react-common";
import {DroppableBox} from "./DroppableBox";
import {DraggableButton} from "./DroppableButton";
import Grid from "@mui/material/Grid";

interface Props {
    vehicle?: Vehicle;
    onClose: () => void;
}

type Annotation = {
    xCoordinate: number;
    yCoordinate: number;
    note: string;
};

export default function VehicleAnnotation({
                                              vehicle,
                                              onClose,
                                          }: Props) {
    const [annotations, setAnnotations] = useState<Annotation[]>([]);

    const {
        data: vehicleAnnotations,
        error: vehicleAnnotationsError,
        isLoading: vehicleAnnotationsLoading,
    } = useFetchVehicleAnnotationsQuery(vehicle?.vehicleId, {
        skip: vehicle === undefined,
    });
    const [isImageLoaded, setIsImageLoaded] = useState<boolean>(false);

    const [box, setBox] = React.useState("ButtonBox");

    const handleDrop = React.useCallback((id: string) => {
        setBox(id);
    }, []);

    useEffect(() => {
        if (vehicleAnnotations) {
            setAnnotations(vehicleAnnotations);
        }
    }, [vehicleAnnotations]);


    return <React.Fragment>
        <Grid container flexDirection={"column"}>
            <DragAndDrop>
                <Grid item xs={12}>
                    <div
                        style={{
                            width: "100%",
                            height: "100%",
                        }}
                    >
                        <DroppableBox
                            selected={box === "ImageBox"}
                            id="ImageBox"
                            onDrop={handleDrop}
                        >
                            {box === "ImageBox" ? <DraggableButton/> : null}
                            <img
                                src={carDiagramImage}
                                alt="Car Diagram"
                                style={{
                                    width: "100%",
                                    height: "100%",
                                    objectFit: "cover",
                                }}
                            />
                        </DroppableBox>
                    </div>
                </Grid>
                <Grid container>
                    <Grid item xs={3}>
                        <DroppableBox
                            selected={box === "Trash"}
                            id="Trash"
                            onDrop={handleDrop}
                        >
                            {box === "Trash" ? <DraggableButton/> : null}
                        </DroppableBox>
                    </Grid>

                    <Grid item xs={3}>
                        <DroppableBox
                            selected={box === "ButtonBox"}
                            id="ButtonBox"
                            onDrop={handleDrop}
                        >
                            {box === "ButtonBox" ? <DraggableButton/> : null}
                        </DroppableBox>
                    </Grid>
                </Grid>

                {/* Car Image DroppableBox */}
            </DragAndDrop>
        </Grid>
    </React.Fragment>


}
