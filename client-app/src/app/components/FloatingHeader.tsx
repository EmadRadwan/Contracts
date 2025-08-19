import React from "react";
import { GridHeaderCellProps } from "@progress/kendo-react-grid";
import withFloatingLabelColumnHeader from "./withFloatingLabelColumnHeader";

interface FloatingHeaderProps extends GridHeaderCellProps {
  label: string;
  translationKey: string;
}

const FloatingHeader: React.FC<FloatingHeaderProps> = (props) => {
  const WrappedHeader = withFloatingLabelColumnHeader(() => (
    <span style={{ cursor: "pointer" }}>{props.title}</span>
  ));

  return (
    <WrappedHeader
      {...props}
      label={props.label}
      translationKey={props.translationKey}
    />
  );
};

export default FloatingHeader;
