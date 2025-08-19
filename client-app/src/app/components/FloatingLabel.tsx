import React, { useState, ComponentType } from "react";
import { useFloating, useHover, useInteractions, offset } from "@floating-ui/react";
import { RootState, useAppSelector } from "../store/configureStore";
import { LocalizationService } from "@progress/kendo-react-intl";

interface WithFloatingLabelFlexibleProps {
  label: string;
  translationKey: string;
  placement?: 'top' | 'bottom'
  isField?: boolean
}

const withFloatingLabelFlexible = <P extends object>(
  WrappedComponent: ComponentType<P>
): React.FC<P & WithFloatingLabelFlexibleProps> => {
  const WithFloatingLabelFlexible: React.FC<P & WithFloatingLabelFlexibleProps> = (props) => {
    const { placement = "top", isField = false } = props
    const [isLabelVisible, setIsLabelVisible] = useState(false);
    const {user} = useAppSelector(state => state.account)

    const { refs, floatingStyles, context } = useFloating({
      open: isLabelVisible,
      onOpenChange: setIsLabelVisible,
      middleware: [offset(4)],
      placement: placement,
    });

    const hover = useHover(context);
    const { getReferenceProps, getFloatingProps } = useInteractions([hover]);

    const selectedLanguage = useAppSelector(
      (state: RootState) => state.localization.language
    );

    const getTextDirection = (language: string) => {
      // Check for RTL languages
      return language === "ar" ? "rtl" : "ltr";
    };

    if (selectedLanguage === "en" || (selectedLanguage === "ar" && ((user && user.dualLanguage === "N") || !user) )) {
      return <WrappedComponent {...(props as P)} />;
    }

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
    const floatingLabel = getFloatingLabel()

    // Determine the direction of the floating label based on the selected language
    const direction = getTextDirection(selectedLanguage);
    const isTop = placement === "top"
    return (
      <div style={{ position: "relative", direction: direction }}>
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
            top: isField ? "50%" : isTop ? "0" : "100%",
              left: isField ? "10rem" : "50%",
              transform: isField
                ? "translate(-20px, -3rem)"
                : `translate(-50%, ${isTop ? "-100%" : "8px"})`,
            backgroundColor: "rgba(0, 0, 0, 0.8)",
            color: "white",
            padding: "4px 8px",
            borderRadius: "4px",
            fontSize: "0.95rem",
            zIndex: 999,
            whiteSpace: "nowrap",
          }}
            {...getFloatingProps()}
          >
            {floatingLabel}
          </div>
        )}
      </div>
    );
  };

  return WithFloatingLabelFlexible;
};

export default withFloatingLabelFlexible;
