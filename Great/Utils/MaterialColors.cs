/**
 * Google Material Design Color Palette for use in C#, http://www.google.com/design/spec/style/color.html#color-ui-color-palette
 * Color.xml file used to create this resource - http://bit.ly/mdcolorsxml
 * Link to this MaterialColor.cs resource file - https://goo.gl/2x6WgH
 * 
 * Author: Cyrus Dargahi
 * https://github.com/cyrusdargahi
 */
using System.Collections.Generic;
using System.Linq;

namespace Great
{
    public static class MaterialColors
    {
        public const string RED_50 = "#FFEBEE";
        public const string RED_100 = "#FFCDD2";
        public const string RED_200 = "#EF9A9A";
        public const string RED_300 = "#E57373";
        public const string RED_400 = "#EF5350";
        public const string RED_500 = "#F44336";
        public const string RED_600 = "#E53935";
        public const string RED_700 = "#D32F2F";
        public const string RED_800 = "#C62828";
        public const string RED_900 = "#B71C1C";
        public const string RED_A100 = "#FF8A80";
        public const string RED_A200 = "#FF5252";
        public const string RED_A400 = "#FF1744";
        public const string RED_A700 = "#D50000";

        public const string DEEP_PURPLE_50 = "#EDE7F6";
        public const string DEEP_PURPLE_100 = "#D1C4E9";
        public const string DEEP_PURPLE_200 = "#B39DDB";
        public const string DEEP_PURPLE_300 = "#9575CD";
        public const string DEEP_PURPLE_400 = "#7E57C2";
        public const string DEEP_PURPLE_500 = "#673AB7";
        public const string DEEP_PURPLE_600 = "#5E35B1";
        public const string DEEP_PURPLE_700 = "#512DA8";
        public const string DEEP_PURPLE_800 = "#4527A0";
        public const string DEEP_PURPLE_900 = "#311B92";
        public const string DEEP_PURPLE_A100 = "#B388FF";
        public const string DEEP_PURPLE_A200 = "#7C4DFF";
        public const string DEEP_PURPLE_A400 = "#651FFF";
        public const string DEEP_PURPLE_A700 = "#6200EA";

        public const string LIGHT_BLUE_50 = "#E1F5FE";
        public const string LIGHT_BLUE_100 = "#B3E5FC";
        public const string LIGHT_BLUE_200 = "#81D4FA";
        public const string LIGHT_BLUE_300 = "#4FC3F7";
        public const string LIGHT_BLUE_400 = "#29B6F6";
        public const string LIGHT_BLUE_500 = "#03A9F4";
        public const string LIGHT_BLUE_600 = "#039BE5";
        public const string LIGHT_BLUE_700 = "#0288D1";
        public const string LIGHT_BLUE_800 = "#0277BD";
        public const string LIGHT_BLUE_900 = "#01579B";
        public const string LIGHT_BLUE_A100 = "#80D8FF";
        public const string LIGHT_BLUE_A200 = "#40C4FF";
        public const string LIGHT_BLUE_A400 = "#00B0FF";
        public const string LIGHT_BLUE_A700 = "#0091EA";

        public const string GREEN_50 = "#E8F5E9";
        public const string GREEN_100 = "#C8E6C9";
        public const string GREEN_200 = "#A5D6A7";
        public const string GREEN_300 = "#81C784";
        public const string GREEN_400 = "#66BB6A";
        public const string GREEN_500 = "#4CAF50";
        public const string GREEN_600 = "#43A047";
        public const string GREEN_700 = "#388E3C";
        public const string GREEN_800 = "#2E7D32";
        public const string GREEN_900 = "#1B5E20";
        public const string GREEN_A100 = "#B9F6CA";
        public const string GREEN_A200 = "#69F0AE";
        public const string GREEN_A400 = "#00E676";
        public const string GREEN_A700 = "#00C853";

        public const string YELLOW_50 = "#FFFDE7";
        public const string YELLOW_100 = "#FFF9C4";
        public const string YELLOW_200 = "#FFF59D";
        public const string YELLOW_300 = "#FFF176";
        public const string YELLOW_400 = "#FFEE58";
        public const string YELLOW_500 = "#FFEB3B";
        public const string YELLOW_600 = "#FDD835";
        public const string YELLOW_700 = "#FBC02D";
        public const string YELLOW_800 = "#F9A825";
        public const string YELLOW_900 = "#F57F17";
        public const string YELLOW_A100 = "#FFFF8D";
        public const string YELLOW_A200 = "#FFFF00";
        public const string YELLOW_A400 = "#FFEA00";
        public const string YELLOW_A700 = "#FFD600";

        public const string DEEP_ORANAGE_50 = "#FBE9E7";
        public const string DEEP_ORANAGE_100 = "#FFCCBC";
        public const string DEEP_ORANAGE_200 = "#FFAB91";
        public const string DEEP_ORANAGE_300 = "#FF8A65";
        public const string DEEP_ORANAGE_400 = "#FF7043";
        public const string DEEP_ORANAGE_500 = "#FF5722";
        public const string DEEP_ORANAGE_600 = "#F4511E";
        public const string DEEP_ORANAGE_700 = "#E64A19";
        public const string DEEP_ORANAGE_800 = "#D84315";
        public const string DEEP_ORANAGE_900 = "#BF360C";
        public const string DEEP_ORANAGE_A100 = "#FF9E80";
        public const string DEEP_ORANAGE_A200 = "#FF6E40";
        public const string DEEP_ORANAGE_A400 = "#FF3D00";
        public const string DEEP_ORANAGE_A700 = "#DD2C00";

        public const string BLUE_GREY_50 = "#ECEFF1";
        public const string BLUE_GREY_100 = "#CFD8DC";
        public const string BLUE_GREY_200 = "#B0BEC5";
        public const string BLUE_GREY_300 = "#90A4AE";
        public const string BLUE_GREY_400 = "#78909C";
        public const string BLUE_GREY_500 = "#607D8B";
        public const string BLUE_GREY_600 = "#546E7A";
        public const string BLUE_GREY_700 = "#455A64";
        public const string BLUE_GREY_800 = "#37474F";
        public const string BLUE_GREY_900 = "#263238";

        public const string PINK_50 = "#FCE4EC";
        public const string PINK_100 = "#F8BBD0";
        public const string PINK_200 = "#F48FB1";
        public const string PINK_300 = "#F06292";
        public const string PINK_400 = "#EC407A";
        public const string PINK_500 = "#E91E63";
        public const string PINK_600 = "#D81B60";
        public const string PINK_700 = "#C2185B";
        public const string PINK_800 = "#AD1457";
        public const string PINK_900 = "#880E4F";
        public const string PINK_A100 = "#FF80AB";
        public const string PINK_A200 = "#FF4081";
        public const string PINK_A400 = "#F50057";
        public const string PINK_A700 = "#C51162";

        public const string INDIGO_50 = "#E8EAF6";
        public const string INDIGO_100 = "#C5CAE9";
        public const string INDIGO_200 = "#9FA8DA";
        public const string INDIGO_300 = "#7986CB";
        public const string INDIGO_400 = "#5C6BC0";
        public const string INDIGO_500 = "#3F51B5";
        public const string INDIGO_600 = "#3949AB";
        public const string INDIGO_700 = "#303F9F";
        public const string INDIGO_800 = "#283593";
        public const string INDIGO_900 = "#1A237E";
        public const string INDIGO_A100 = "#8C9EFF";
        public const string INDIGO_A200 = "#536DFE";
        public const string INDIGO_A400 = "#3D5AFE";
        public const string INDIGO_A700 = "#304FFE";

        public const string CYAN_50 = "#E0F7FA";
        public const string CYAN_100 = "#B2EBF2";
        public const string CYAN_200 = "#80DEEA";
        public const string CYAN_300 = "#4DD0E1";
        public const string CYAN_400 = "#26C6DA";
        public const string CYAN_500 = "#00BCD4";
        public const string CYAN_600 = "#00ACC1";
        public const string CYAN_700 = "#0097A7";
        public const string CYAN_800 = "#00838F";
        public const string CYAN_900 = "#006064";
        public const string CYAN_A100 = "#84FFFF";
        public const string CYAN_A200 = "#18FFFF";
        public const string CYAN_A400 = "#00E5FF";
        public const string CYAN_A700 = "#00B8D4";

        public const string LIGHT_GREEN_50 = "#F1F8E9";
        public const string LIGHT_GREEN_100 = "#DCEDC8";
        public const string LIGHT_GREEN_200 = "#C5E1A5";
        public const string LIGHT_GREEN_300 = "#AED581";
        public const string LIGHT_GREEN_400 = "#9CCC65";
        public const string LIGHT_GREEN_500 = "#8BC34A";
        public const string LIGHT_GREEN_600 = "#7CB342";
        public const string LIGHT_GREEN_700 = "#689F38";
        public const string LIGHT_GREEN_800 = "#558B2F";
        public const string LIGHT_GREEN_900 = "#33691E";
        public const string LIGHT_GREEN_A100 = "#CCFF90";
        public const string LIGHT_GREEN_A200 = "#B2FF59";
        public const string LIGHT_GREEN_A400 = "#76FF03";
        public const string LIGHT_GREEN_A700 = "#64DD17";

        public const string AMBER_50 = "#FFF8E1";
        public const string AMBER_100 = "#FFECB3";
        public const string AMBER_200 = "#FFE082";
        public const string AMBER_300 = "#FFD54F";
        public const string AMBER_400 = "#FFCA28";
        public const string AMBER_500 = "#FFC107";
        public const string AMBER_600 = "#FFB300";
        public const string AMBER_700 = "#FFA000";
        public const string AMBER_800 = "#FF8F00";
        public const string AMBER_900 = "#FF6F00";
        public const string AMBER_A100 = "#FFE57F";
        public const string AMBER_A200 = "#FFD740";
        public const string AMBER_A400 = "#FFC400";
        public const string AMBER_A700 = "#FFAB00";

        public const string BROWN_50 = "#EFEBE9";
        public const string BROWN_100 = "#D7CCC8";
        public const string BROWN_200 = "#BCAAA4";
        public const string BROWN_300 = "#A1887F";
        public const string BROWN_400 = "#8D6E63";
        public const string BROWN_500 = "#795548";
        public const string BROWN_600 = "#6D4C41";
        public const string BROWN_700 = "#5D4037";
        public const string BROWN_800 = "#4E342E";
        public const string BROWN_900 = "#3E2723";

        public const string PURPLE_50 = "#F3E5F5";
        public const string PURPLE_100 = "#E1BEE7";
        public const string PURPLE_200 = "#CE93D8";
        public const string PURPLE_300 = "#BA68C8";
        public const string PURPLE_400 = "#AB47BC";
        public const string PURPLE_500 = "#9C27B0";
        public const string PURPLE_600 = "#8E24AA";
        public const string PURPLE_700 = "#7B1FA2";
        public const string PURPLE_800 = "#6A1B9A";
        public const string PURPLE_900 = "#4A148C";
        public const string PURPLE_A100 = "#EA80FC";
        public const string PURPLE_A200 = "#E040FB";
        public const string PURPLE_A400 = "#D500F9";
        public const string PURPLE_A700 = "#AA00FF";

        public const string BLUE_50 = "#E3F2FD";
        public const string BLUE_100 = "#BBDEFB";
        public const string BLUE_200 = "#90CAF9";
        public const string BLUE_300 = "#64B5F6";
        public const string BLUE_400 = "#42A5F5";
        public const string BLUE_500 = "#2196F3";
        public const string BLUE_600 = "#1E88E5";
        public const string BLUE_700 = "#1976D2";
        public const string BLUE_800 = "#1565C0";
        public const string BLUE_900 = "#0D47A1";
        public const string BLUE_A100 = "#82B1FF";
        public const string BLUE_A200 = "#448AFF";
        public const string BLUE_A400 = "#2979FF";
        public const string BLUE_A700 = "#2962FF";

        public const string TEAL_50 = "#E0F2F1";
        public const string TEAL_100 = "#B2DFDB";
        public const string TEAL_200 = "#80CBC4";
        public const string TEAL_300 = "#4DB6AC";
        public const string TEAL_400 = "#26A69A";
        public const string TEAL_500 = "#009688";
        public const string TEAL_600 = "#00897B";
        public const string TEAL_700 = "#00796B";
        public const string TEAL_800 = "#00695C";
        public const string TEAL_900 = "#004D40";
        public const string TEAL_A100 = "#A7FFEB";
        public const string TEAL_A200 = "#64FFDA";
        public const string TEAL_A400 = "#1DE9B6";
        public const string TEAL_A700 = "#00BFA5";

        public const string LIME_50 = "#F9FBE7";
        public const string LIME_100 = "#F0F4C3";
        public const string LIME_200 = "#E6EE9C";
        public const string LIME_300 = "#DCE775";
        public const string LIME_400 = "#D4E157";
        public const string LIME_500 = "#CDDC39";
        public const string LIME_600 = "#C0CA33";
        public const string LIME_700 = "#AFB42B";
        public const string LIME_800 = "#9E9D24";
        public const string LIME_900 = "#827717";
        public const string LIME_A100 = "#F4FF81";
        public const string LIME_A200 = "#EEFF41";
        public const string LIME_A400 = "#C6FF00";
        public const string LIME_A700 = "#AEEA00";

        public const string ORANGE_50 = "#FFF3E0";
        public const string ORANGE_100 = "#FFE0B2";
        public const string ORANGE_200 = "#FFCC80";
        public const string ORANGE_300 = "#FFB74D";
        public const string ORANGE_400 = "#FFA726";
        public const string ORANGE_500 = "#FF9800";
        public const string ORANGE_600 = "#FB8C00";
        public const string ORANGE_700 = "#F57C00";
        public const string ORANGE_800 = "#EF6C00";
        public const string ORANGE_900 = "#E65100";
        public const string ORANGE_A100 = "#FFD180";
        public const string ORANGE_A200 = "#FFAB40";
        public const string ORANGE_A400 = "#FF9100";
        public const string ORANGE_A700 = "#FF6D00";

        public const string GREY_50 = "#FAFAFA";
        public const string GREY_100 = "#F5F5F5";
        public const string GREY_200 = "#EEEEEE";
        public const string GREY_300 = "#E0E0E0";
        public const string GREY_400 = "#BDBDBD";
        public const string GREY_500 = "#9E9E9E";
        public const string GREY_600 = "#757575";
        public const string GREY_700 = "#616161";
        public const string GREY_800 = "#424242";
        public const string GREY_900 = "#212121";

        public const string BLACK = "#000000";
        public const string WHITE = "#FFFFFF";

        public static Dictionary<string, string> Colors { get; } = new Dictionary<string, string>
        {
            { "RED_50", RED_50 },
            { "RED_100", RED_100 },
            { "RED_200", RED_200 },
            { "RED_300", RED_300 },
            { "RED_400", RED_400 },
            { "RED_500", RED_500 },
            { "RED_600", RED_600 },
            { "RED_700", RED_700 },
            { "RED_800", RED_800 },
            { "RED_900", RED_900 },
            { "RED_A100", RED_A100 },
            { "RED_A200", RED_A200 },
            { "RED_A400", RED_A400 },
            { "RED_A700", RED_A700 },

            { "DEEP_PURPLE_50", DEEP_PURPLE_50 },
            { "DEEP_PURPLE_100", DEEP_PURPLE_100 },
            { "DEEP_PURPLE_200", DEEP_PURPLE_200 },
            { "DEEP_PURPLE_300", DEEP_PURPLE_300 },
            { "DEEP_PURPLE_400", DEEP_PURPLE_400 },
            { "DEEP_PURPLE_500", DEEP_PURPLE_500 },
            { "DEEP_PURPLE_600", DEEP_PURPLE_600 },
            { "DEEP_PURPLE_700", DEEP_PURPLE_700 },
            { "DEEP_PURPLE_800", DEEP_PURPLE_800 },
            { "DEEP_PURPLE_900", DEEP_PURPLE_900 },
            { "DEEP_PURPLE_A100", DEEP_PURPLE_A100 },
            { "DEEP_PURPLE_A200", DEEP_PURPLE_A200 },
            { "DEEP_PURPLE_A400", DEEP_PURPLE_A400 },
            { "DEEP_PURPLE_A700", DEEP_PURPLE_A700 },

            { "LIGHT_BLUE_50", LIGHT_BLUE_50 },
            { "LIGHT_BLUE_100", LIGHT_BLUE_100 },
            { "LIGHT_BLUE_200", LIGHT_BLUE_200 },
            { "LIGHT_BLUE_300", LIGHT_BLUE_300 },
            { "LIGHT_BLUE_400", LIGHT_BLUE_400 },
            { "LIGHT_BLUE_500", LIGHT_BLUE_500 },
            { "LIGHT_BLUE_600", LIGHT_BLUE_600 },
            { "LIGHT_BLUE_700", LIGHT_BLUE_700 },
            { "LIGHT_BLUE_800", LIGHT_BLUE_800 },
            { "LIGHT_BLUE_900", LIGHT_BLUE_900 },
            { "LIGHT_BLUE_A100", LIGHT_BLUE_A100 },
            { "LIGHT_BLUE_A200", LIGHT_BLUE_A200 },
            { "LIGHT_BLUE_A400", LIGHT_BLUE_A400 },
            { "LIGHT_BLUE_A700", LIGHT_BLUE_A700 },

            { "GREEN_50", GREEN_50 },
            { "GREEN_100", GREEN_100 },
            { "GREEN_200", GREEN_200 },
            { "GREEN_300", GREEN_300 },
            { "GREEN_400", GREEN_400 },
            { "GREEN_500", GREEN_500 },
            { "GREEN_600", GREEN_600 },
            { "GREEN_700", GREEN_700 },
            { "GREEN_800", GREEN_800 },
            { "GREEN_900", GREEN_900 },
            { "GREEN_A100", GREEN_A100 },
            { "GREEN_A200", GREEN_A200 },
            { "GREEN_A400", GREEN_A400 },
            { "GREEN_A700", GREEN_A700 },

            { "YELLOW_50", YELLOW_50 },
            { "YELLOW_100", YELLOW_100 },
            { "YELLOW_200", YELLOW_200 },
            { "YELLOW_300", YELLOW_300 },
            { "YELLOW_400", YELLOW_400 },
            { "YELLOW_500", YELLOW_500 },
            { "YELLOW_600", YELLOW_600 },
            { "YELLOW_700", YELLOW_700 },
            { "YELLOW_800", YELLOW_800 },
            { "YELLOW_900", YELLOW_900 },
            { "YELLOW_A100", YELLOW_A100 },
            { "YELLOW_A200", YELLOW_A200 },
            { "YELLOW_A400", YELLOW_A400 },
            { "YELLOW_A700", YELLOW_A700 },

            { "DEEP_ORANAGE_50", DEEP_ORANAGE_50 },
            { "DEEP_ORANAGE_100", DEEP_ORANAGE_100 },
            { "DEEP_ORANAGE_200", DEEP_ORANAGE_200 },
            { "DEEP_ORANAGE_300", DEEP_ORANAGE_300 },
            { "DEEP_ORANAGE_400", DEEP_ORANAGE_400 },
            { "DEEP_ORANAGE_500", DEEP_ORANAGE_500 },
            { "DEEP_ORANAGE_600", DEEP_ORANAGE_600 },
            { "DEEP_ORANAGE_700", DEEP_ORANAGE_700 },
            { "DEEP_ORANAGE_800", DEEP_ORANAGE_800 },
            { "DEEP_ORANAGE_900", DEEP_ORANAGE_900 },
            { "DEEP_ORANAGE_A100", DEEP_ORANAGE_A100 },
            { "DEEP_ORANAGE_A200", DEEP_ORANAGE_A200 },
            { "DEEP_ORANAGE_A400", DEEP_ORANAGE_A400 },
            { "DEEP_ORANAGE_A700", DEEP_ORANAGE_A700 },

            { "PINK_50", PINK_50 },
            { "PINK_100", PINK_100 },
            { "PINK_200", PINK_200 },
            { "PINK_300", PINK_300 },
            { "PINK_400", PINK_400 },
            { "PINK_500", PINK_500 },
            { "PINK_600", PINK_600 },
            { "PINK_700", PINK_700 },
            { "PINK_800", PINK_800 },
            { "PINK_900", PINK_900 },
            { "PINK_A100", PINK_A100 },
            { "PINK_A200", PINK_A200 },
            { "PINK_A400", PINK_A400 },
            { "PINK_A700", PINK_A700 },

            { "INDIGO_50", INDIGO_50 },
            { "INDIGO_100", INDIGO_100 },
            { "INDIGO_200", INDIGO_200 },
            { "INDIGO_300", INDIGO_300 },
            { "INDIGO_400", INDIGO_400 },
            { "INDIGO_500", INDIGO_500 },
            { "INDIGO_600", INDIGO_600 },
            { "INDIGO_700", INDIGO_700 },
            { "INDIGO_800", INDIGO_800 },
            { "INDIGO_900", INDIGO_900 },
            { "INDIGO_A100", INDIGO_A100 },
            { "INDIGO_A200", INDIGO_A200 },
            { "INDIGO_A400", INDIGO_A400 },
            { "INDIGO_A700", INDIGO_A700 },

            { "CYAN_50", CYAN_50 },
            { "CYAN_100", CYAN_100 },
            { "CYAN_200", CYAN_200 },
            { "CYAN_300", CYAN_300 },
            { "CYAN_400", CYAN_400 },
            { "CYAN_500", CYAN_500 },
            { "CYAN_600", CYAN_600 },
            { "CYAN_700", CYAN_700 },
            { "CYAN_800", CYAN_800 },
            { "CYAN_900", CYAN_900 },
            { "CYAN_A100", CYAN_A100 },
            { "CYAN_A200", CYAN_A200 },
            { "CYAN_A400", CYAN_A400 },
            { "CYAN_A700", CYAN_A700 },

            { "LIGHT_GREEN_50", LIGHT_GREEN_50 },
            { "LIGHT_GREEN_100", LIGHT_GREEN_100 },
            { "LIGHT_GREEN_200", LIGHT_GREEN_200 },
            { "LIGHT_GREEN_300", LIGHT_GREEN_300 },
            { "LIGHT_GREEN_400", LIGHT_GREEN_400 },
            { "LIGHT_GREEN_500", LIGHT_GREEN_500 },
            { "LIGHT_GREEN_600", LIGHT_GREEN_600 },
            { "LIGHT_GREEN_700", LIGHT_GREEN_700 },
            { "LIGHT_GREEN_800", LIGHT_GREEN_800 },
            { "LIGHT_GREEN_900", LIGHT_GREEN_900 },
            { "LIGHT_GREEN_A100", LIGHT_GREEN_A100 },
            { "LIGHT_GREEN_A200", LIGHT_GREEN_A200 },
            { "LIGHT_GREEN_A400", LIGHT_GREEN_A400 },
            { "LIGHT_GREEN_A700", LIGHT_GREEN_A700 },

            { "AMBER_50", AMBER_50 },
            { "AMBER_100", AMBER_100 },
            { "AMBER_200", AMBER_200 },
            { "AMBER_300", AMBER_300 },
            { "AMBER_400", AMBER_400 },
            { "AMBER_500", AMBER_500 },
            { "AMBER_600", AMBER_600 },
            { "AMBER_700", AMBER_700 },
            { "AMBER_800", AMBER_800 },
            { "AMBER_900", AMBER_900 },
            { "AMBER_A100", AMBER_A100 },
            { "AMBER_A200", AMBER_A200 },
            { "AMBER_A400", AMBER_A400 },
            { "AMBER_A700", AMBER_A700 },

            { "PURPLE_50", PURPLE_50 },
            { "PURPLE_100", PURPLE_100 },
            { "PURPLE_200", PURPLE_200 },
            { "PURPLE_300", PURPLE_300 },
            { "PURPLE_400", PURPLE_400 },
            { "PURPLE_500", PURPLE_500 },
            { "PURPLE_600", PURPLE_600 },
            { "PURPLE_700", PURPLE_700 },
            { "PURPLE_800", PURPLE_800 },
            { "PURPLE_900", PURPLE_900 },
            { "PURPLE_A100", PURPLE_A100 },
            { "PURPLE_A200", PURPLE_A200 },
            { "PURPLE_A400", PURPLE_A400 },
            { "PURPLE_A700", PURPLE_A700 },

            { "BLUE_50", BLUE_50 },
            { "BLUE_100", BLUE_100 },
            { "BLUE_200", BLUE_200 },
            { "BLUE_300", BLUE_300 },
            { "BLUE_400", BLUE_400 },
            { "BLUE_500", BLUE_500 },
            { "BLUE_600", BLUE_600 },
            { "BLUE_700", BLUE_700 },
            { "BLUE_800", BLUE_800 },
            { "BLUE_900", BLUE_900 },
            { "BLUE_A100", BLUE_A100 },
            { "BLUE_A200", BLUE_A200 },
            { "BLUE_A400", BLUE_A400 },
            { "BLUE_A700", BLUE_A700 },

            { "TEAL_50", TEAL_50 },
            { "TEAL_100", TEAL_100 },
            { "TEAL_200", TEAL_200 },
            { "TEAL_300", TEAL_300 },
            { "TEAL_400", TEAL_400 },
            { "TEAL_500", TEAL_500 },
            { "TEAL_600", TEAL_600 },
            { "TEAL_700", TEAL_700 },
            { "TEAL_800", TEAL_800 },
            { "TEAL_900", TEAL_900 },
            { "TEAL_A100", TEAL_A100 },
            { "TEAL_A200", TEAL_A200 },
            { "TEAL_A400", TEAL_A400 },
            { "TEAL_A700", TEAL_A700 },

            { "LIME_50", LIME_50 },
            { "LIME_100", LIME_100 },
            { "LIME_200", LIME_200 },
            { "LIME_300", LIME_300 },
            { "LIME_400", LIME_400 },
            { "LIME_500", LIME_500 },
            { "LIME_600", LIME_600 },
            { "LIME_700", LIME_700 },
            { "LIME_800", LIME_800 },
            { "LIME_900", LIME_900 },
            { "LIME_A100", LIME_A100 },
            { "LIME_A200", LIME_A200 },
            { "LIME_A400", LIME_A400 },
            { "LIME_A700", LIME_A700 },

            { "ORANGE_50", ORANGE_50 },
            { "ORANGE_100", ORANGE_100 },
            { "ORANGE_200", ORANGE_200 },
            { "ORANGE_300", ORANGE_300 },
            { "ORANGE_400", ORANGE_400 },
            { "ORANGE_500", ORANGE_500 },
            { "ORANGE_600", ORANGE_600 },
            { "ORANGE_700", ORANGE_700 },
            { "ORANGE_800", ORANGE_800 },
            { "ORANGE_900", ORANGE_900 },
            { "ORANGE_A100", ORANGE_A100 },
            { "ORANGE_A200", ORANGE_A200 },
            { "ORANGE_A400", ORANGE_A400 },
            { "ORANGE_A700", ORANGE_A700 },

            { "BROWN_50", BROWN_50 },
            { "BROWN_100", BROWN_100 },
            { "BROWN_200", BROWN_200 },
            { "BROWN_300", BROWN_300 },
            { "BROWN_400", BROWN_400 },
            { "BROWN_500", BROWN_500 },
            { "BROWN_600", BROWN_600 },
            { "BROWN_700", BROWN_700 },
            { "BROWN_800", BROWN_800 },
            { "BROWN_900", BROWN_900 },

            { "BLUE_GREY_50", BLUE_GREY_50 },
            { "BLUE_GREY_100", BLUE_GREY_100 },
            { "BLUE_GREY_200", BLUE_GREY_200 },
            { "BLUE_GREY_300", BLUE_GREY_300 },
            { "BLUE_GREY_400", BLUE_GREY_400 },
            { "BLUE_GREY_500", BLUE_GREY_500 },
            { "BLUE_GREY_600", BLUE_GREY_600 },
            { "BLUE_GREY_700", BLUE_GREY_700 },
            { "BLUE_GREY_800", BLUE_GREY_800 },
            { "BLUE_GREY_900", BLUE_GREY_900 },

            { "GREY_50", GREY_50 },
            { "GREY_100", GREY_100 },
            { "GREY_200", GREY_200 },
            { "GREY_300", GREY_300 },
            { "GREY_400", GREY_400 },
            { "GREY_500", GREY_500 },
            { "GREY_600", GREY_600 },
            { "GREY_700", GREY_700 },
            { "GREY_800", GREY_800 },
            { "GREY_900", GREY_900 },

            { "BLACK", BLACK },
            { "WHITE", WHITE }
        };

        private static Dictionary<string, string> _colors_50;
        public static Dictionary<string, string> Colors_50 => _colors_50 ?? (_colors_50 = Colors.Where(item => item.Key.EndsWith(_50)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_100;
        public static Dictionary<string, string> Colors_100 => _colors_100 ?? (_colors_100 = Colors.Where(item => item.Key.EndsWith(_100)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_200;
        public static Dictionary<string, string> Colors_200 => _colors_200 ?? (_colors_200 = Colors.Where(item => item.Key.EndsWith(_200)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_300;
        public static Dictionary<string, string> Colors_300 => _colors_300 ?? (_colors_300 = Colors.Where(item => item.Key.EndsWith(_300)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_400;
        public static Dictionary<string, string> Colors_400 => _colors_400 ?? (_colors_400 = Colors.Where(item => item.Key.EndsWith(_400)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_500;
        public static Dictionary<string, string> Colors_500 => _colors_500 ?? (_colors_500 = Colors.Where(item => item.Key.EndsWith(_500)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_600;
        public static Dictionary<string, string> Colors_600 => _colors_600 ?? (_colors_600 = Colors.Where(item => item.Key.EndsWith(_600)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_700;
        public static Dictionary<string, string> Colors_700 => _colors_700 ?? (_colors_700 = Colors.Where(item => item.Key.EndsWith(_700)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_800;
        public static Dictionary<string, string> Colors_800 => _colors_800 ?? (_colors_800 = Colors.Where(item => item.Key.EndsWith(_800)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_900;
        public static Dictionary<string, string> Colors_900 => _colors_900 ?? (_colors_900 = Colors.Where(item => item.Key.EndsWith(_900)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_A100;
        public static Dictionary<string, string> Colors_A100 => _colors_A100 ?? (_colors_A100 = Colors.Where(item => item.Key.EndsWith(_A100)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_A200;
        public static Dictionary<string, string> Colors_A200 => _colors_A200 ?? (_colors_A200 = Colors.Where(item => item.Key.EndsWith(_A200)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_A400;
        public static Dictionary<string, string> Colors_A400 => _colors_A400 ?? (_colors_A400 = Colors.Where(item => item.Key.EndsWith(_A400)).ToDictionary(t => t.Key, t => t.Value));

        private static Dictionary<string, string> _colors_A700;
        public static Dictionary<string, string> Colors_A700 => _colors_A700 ?? (_colors_A700 = Colors.Where(item => item.Key.EndsWith(_A700)).ToDictionary(t => t.Key, t => t.Value));

        private const string _50 = "50";
        private const string _100 = "100";
        private const string _200 = "200";
        private const string _300 = "300";
        private const string _400 = "400";
        private const string _500 = "500";
        private const string _600 = "600";
        private const string _700 = "700";
        private const string _800 = "800";
        private const string _900 = "900";

        private const string _A100 = "A100";
        private const string _A200 = "A200";
        private const string _A400 = "A400";
        private const string _A700 = "A700";
    }
}