import React from "react";
import { DropDownList } from "@progress/kendo-react-dropdowns";
import { useAppSelector } from "../store/configureStore";
import { ListItem, ListItemIcon, ListItemText } from "@mui/material";
import "./LanguageChooser.css";

interface LanguageItem {
  text: string;
  value: string;
}

const LanguageChooser = () => {
  // get languages from store using useAppSelector
  const languages = ['en', 'ar']
  const language = useAppSelector((state) => state.localization.language);
  const flagsMap = [
    { value: "en", icon: "../../../svg/us.svg", text: "English" },
    { value: "ar", icon: "../../../svg/egypt.svg", text: "العربية" },
  ];
  const languageItems: LanguageItem[] = languages.map((lang) => ({
    text: flagsMap.find(item => item.value === lang)?.text as string,
    value: lang,
  }));
  // const dispatch = useAppDispatch();

  const itemRender = (
    li: React.ReactElement<HTMLLIElement>,
    itemProps: any
  ) => {
    const lang = itemProps.dataItem;
    const flag = flagsMap.find((flag) => flag.value === lang)?.icon;

    return (
      <ListItem
        {...li.props}
        key={lang}
        sx={{ padding: 0, display: "flex", alignItems: "center" }}
        onClick={() => onChange(lang)}
      >
        <ListItemIcon sx={{ minWidth: "30px" }}>
          <img
            src={flag}
            alt={lang}
            style={{ width: "20px", height: "20px", border: "none" }}
          />
        </ListItemIcon>
        <ListItemText sx={{ marginInlineStart: "4px" }}>
          {
            languageItems.find((lang: any) => lang.value === itemProps.dataItem)
              ?.text
          }
        </ListItemText>
      </ListItem>
    );
  };

  const valueRender = (element: any, value: any) => {
    if (!value) {
      return element;
    }
    const children = [
      <span
        key={1}
        style={{
          display: "inline-block",
          color: "#fff",
          borderRadius: "50%",
          width: "20px",
          height: "20px",
          textAlign: "center",
          overflow: "hidden",
        }}
      >
        <img
          src={flagsMap.find((flag) => flag.value === value)?.icon}
          alt={value}
          style={{ width: "20px", height: "20px", border: "none" }}
        />
      </span>,
      <span key={2}>&nbsp; &nbsp; {languageItems.find((lang: any) => lang.value === value)?.text}</span>,
    ];
    return React.cloneElement(
      element,
      {
        ...element.props,
      },
      children
    );
  };

  const onChange = (lang: string) => {
    if (language === lang) return
    localStorage.setItem("selectedLang", lang)
    setTimeout(() => window.history.go(0), 500)
  };

  return (
    <div>
      <DropDownList
        style={{ width: "10vw" }}
        data={languages}
        valueRender={valueRender}
        dataItemKey="value"
        defaultValue={
          languageItems.find((item) => item.value === language)?.value
        }
        itemRender={itemRender}
      />
    </div>
  );
};

export default LanguageChooser;
