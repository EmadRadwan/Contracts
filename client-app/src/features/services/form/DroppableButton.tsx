import * as React from "react";
import {Button} from "@progress/kendo-react-buttons";
import {Icon, NormalizedDragEvent, useDraggable,} from "@progress/kendo-react-common";

export const DraggableButton = (props: any) => {
    const [position, setPosition] = React.useState({x: 50, y: 50});
    const [pressed, setPressed] = React.useState<boolean>(false);
    const [dragged, setDragged] = React.useState<boolean>(false);
    const [initial, setInitial] = React.useState<{ x: number; y: number } | null>(
        null,
    );
    const button = React.useRef<Button | null>(null);

    const handlePress = React.useCallback(() => {
        setPressed(true);
    }, []);

    const handleDragStart = React.useCallback(
        (event: NormalizedDragEvent) => {
            setDragged(true);
            setInitial({
                x: event.clientX - position.x,
                y: event.clientY - position.y,
            });
        },
        [position.x, position.y],
    );

    const handleDrag = React.useCallback(
        (event: NormalizedDragEvent) => {
            if (!button.current || !button.current.element || !initial) {
                return;
            }

            setPosition({
                x: event.clientX - initial.x,
                y: event.clientY - initial.y,
            });
        },
        [initial],
    );

    const handleDragEnd = React.useCallback(() => {
        if (!button.current || !button.current.element) {
            return;
        }

        setDragged(false);
        setInitial(null);
    }, []);

    const handleRelease = React.useCallback(() => {
        setPressed(false);
    }, []);

    useDraggable(button, {
        onPress: handlePress,
        onDragStart: handleDragStart,
        onDrag: handleDrag,
        onDragEnd: handleDragEnd,
        onRelease: handleRelease,
    });

    return (
        <Button
            ref={button}
            style={{
                position: "absolute",
                left: position.x,
                top: position.y,
            }}
        >
            <Icon name="move" size="medium"/>
            Drag Me!
        </Button>
    );
};
