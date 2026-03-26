import {useLocalization} from "@progress/kendo-react-intl";
import { useCallback } from "react";


export function useTranslationHelper() {
    const localization = useLocalization();

    const getTranslatedLabel = useCallback((messageKey: string, defaultMessage: string) => {
        return localization.toLanguageString(messageKey, defaultMessage)
    }, [localization]);

    return {getTranslatedLabel};
}
