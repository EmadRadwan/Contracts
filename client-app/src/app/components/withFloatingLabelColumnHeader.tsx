import React, { useState, ComponentType } from "react";
import { useFloating, useHover, useInteractions, offset, shift } from "@floating-ui/react";
import { RootState, useAppSelector } from "../store/configureStore";
import { LocalizationService } from "@progress/kendo-react-intl";

interface WithFloatingLabelProps {
  label: string;
  translationKey: string;
}

const withFloatingLabelColumnHeader = <P extends object>(
  WrappedComponent: ComponentType<P>
): React.FC<P & WithFloatingLabelProps> => {
  const WithFloatingLabel: React.FC<P & WithFloatingLabelProps> = (props) => {
    const [isLabelVisible, setIsLabelVisible] = useState(false);

    const { refs, floatingStyles, context } = useFloating({
      open: isLabelVisible,
      onOpenChange: setIsLabelVisible,
      middleware: [offset(-1), shift({padding: 3})], // Ensure the label doesn't overflow
      placement: "left",
    });

    const hover = useHover(context);
    const { getReferenceProps, getFloatingProps } = useInteractions([hover]);

    const selectedLanguage = useAppSelector(
      (state: RootState) => state.localization.language
    );

    if (selectedLanguage === "en") {
      return <WrappedComponent {...(props as P)} />;
    }

    const getTextDirection = (language: string) => {
      return language === "ar" ? "rtl" : "ltr";
    };

    const getFloatingLabel = () => {
        const service = new LocalizationService(alternativeLanguage)
        if (selectedLanguage === "tr") {
            return service.toLanguageString(props.translationKey, props.label)
        }
        const translation = service.toLanguageString(
            `${props.translationKey}.translation`,
            props.label
        );
        const pronunciation = service.toLanguageString(
            `${props.translationKey}.pronunciation`,
            props.label
        );
        return `${translation} - ${pronunciation}`
    }
  
    const alternativeLanguage = selectedLanguage === "tr" ? "ar" : "tr";
    const floatingLabel = getFloatingLabel();

    return (
      <div style={{ position: "relative" }}>
        {/* Wrapped Component */}
        <div
          ref={refs.setReference}
          {...getReferenceProps({
            onMouseEnter: () => setIsLabelVisible(true),
            onMouseLeave: () => setIsLabelVisible(false),
          })}
        >
          <WrappedComponent {...(props as P)} />
        </div>

        {/* Floating Label */}
        {isLabelVisible && (
          <div
            ref={refs.setFloating}
            style={{
              ...floatingStyles,
              position: "absolute",
              top: 0,
              left: "50%",
              transform: "translate(-55%, 0%)", // Ensure label stays centered above the header
              backgroundColor: "rgba(0, 0, 0, 0.8)",
              color: "white",
              padding: "1px 10px",
              borderRadius: "4px",
              fontSize: "0.85rem",
              whiteSpace: "nowrap",
              zIndex: 9999,
              direction: getTextDirection(selectedLanguage),
            }}
            {...getFloatingProps()}
          >
            {floatingLabel}
          </div>
        )}
      </div>
    );
  };

  return WithFloatingLabel;
};

export default withFloatingLabelColumnHeader;
