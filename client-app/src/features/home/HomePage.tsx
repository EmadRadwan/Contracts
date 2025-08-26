import { Box, Typography } from "@mui/material";
import Slider from "react-slick";
import { useTranslationHelper } from "../../app/hooks/useTranslationHelper";

export default function HomePage() {
  const settings = {
    dots: true,
    infinite: true,
    speed: 500,
    slidesToShow: 1,
    slidesToScroll: 1,
  };
  const { getTranslatedLabel } = useTranslationHelper();

  return (
    <>
        <div className="slider-container">
            <Slider {...settings}>
                <div>
                    <img
                        src="/images/BusinessOneGoldenLand.jpg"
                        alt="hero"
                        className="centered-image"
                    />
                </div>
            </Slider>
        </div>
      <Box display="flex" justifyContent="center" sx={{ p: 2 }}>
        <Typography variant="h1">
          {getTranslatedLabel("general.homepage", "Welcome Back")}
        </Typography>
      </Box>
    </>
  );
}
