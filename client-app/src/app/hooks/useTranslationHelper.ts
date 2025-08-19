import {useLocalization} from "@progress/kendo-react-intl";


export function useTranslationHelper() {
    const localization = useLocalization();

    function getTranslatedLabel(messageKey: string, defaultMessage: string) {
        return localization.toLanguageString(messageKey, defaultMessage)
    }

    return {getTranslatedLabel};
}
