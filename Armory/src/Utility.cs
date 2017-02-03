using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Armory {
    static class Utility {

        /// <summary>
        /// Turns 0.1 into 10% etc.
        /// </summary>
        /// <param name="fraction"></param>
        /// <returns></returns>
        public static string fractionToPercent(string fraction) {
            double val;
            if (double.TryParse(fraction, out val)) {
                return String.Format("{0:P0}", val);
            }

            return fraction;
        }

        /// <summary>
        /// Many values, notably including movement speed, can be divided by 52
        /// to improve readability. This function also appends 'm' to the result.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string divideBy52AndAppendMeters(string value) {
            return divideBy52(value) + "m";
        }

        /// <summary>
        /// Many values, notably including movement speed, can be divided by 52
        /// to improve readability. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string divideBy52(string value) {
            int transform;
            if (int.TryParse(value, out transform)) {
                transform /= 52;
                value = transform.ToString();
            }
            return value;
        }

        public static string multiplyStrings(string fst, string snd) {
            double first, second;
            if (double.TryParse(fst, out first)) {
                if (double.TryParse(snd, out second)) {
                    return (first * second).ToString("#.##");
                }
            }

            return "idk";
        }

        public static string divideStrings(string fst, string snd) {
            double first, second;
            if (double.TryParse(fst, out first)) {
                if (double.TryParse(snd, out second)) {
                    return (first / second).ToString("#.##");
                }
            }

            return "idk";
        }
    }
}
