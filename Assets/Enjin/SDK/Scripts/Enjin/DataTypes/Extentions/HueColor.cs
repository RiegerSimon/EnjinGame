namespace EnjinSDK
{
    using UnityEngine;
    using System.Collections;

    public class HueColour
    {
        public enum HueColorNames
        {
            Lime,
            Green,
            Aqua,
            Blue,
            Navy,
            Purple,
            Pink,
            Red,
            Orange,
            Yellow,
            Violet
        }

        private static Hashtable hueColourValues = new Hashtable{
         { HueColorNames.Lime,     new Color32( 166 , 254 , 0, 1 ) },
         { HueColorNames.Green,     new Color32( 0 , 254 , 111, 1 ) },
         { HueColorNames.Aqua,     new Color32( 0 , 201 , 254, 1 ) },
         { HueColorNames.Blue,     new Color32( 0 , 122 , 254, 1 ) },
         { HueColorNames.Navy,     new Color32( 60 , 0 , 254, 1 ) },
         { HueColorNames.Purple, new Color32( 143 , 0 , 254, 1 ) },
         { HueColorNames.Pink,     new Color32( 232 , 0 , 254, 1 ) },
         { HueColorNames.Red,     new Color32( 254 , 9 , 0, 1 ) },
         { HueColorNames.Orange, new Color32( 254 , 161 , 0, 1 ) },
         { HueColorNames.Yellow, new Color32( 254 , 224 , 0, 1 ) },
         { HueColorNames.Violet, new Color32( 204, 153, 255, 1 ) },
     };

        public static Color32 HueColourValue(HueColorNames color)
        {
            return (Color32)hueColourValues[color];
        }
    }
}