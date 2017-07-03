using System;
using System.Collections.Generic;
using System.Text;

namespace Dream_Protector_Free
{
    class WordGen
    {

        internal static Random R = new Random();

        public static string GenWord(int len)
        {
            StringBuilder sb = new StringBuilder();

            while(sb.Length <= len)
            {
                if(R.Next(0,100) > 50)
                {
                    //sb.Append(" ");
                }
                else
                {
                    sb.Append(Words.WordList[R.Next(0, Words.WordList.Length)]);
                }
            }

            return sb.ToString();
        }
    }
}
