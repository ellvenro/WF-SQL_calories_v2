using System;

namespace WindowsFormsApp1
{
    /// <summary>
    /// Класс для сохранения данных пользователя
    /// </summary>
    [Serializable]
    public class OptionsClass
    {
        public double gender;
        public int weight;
        public int height;
        public int age;
        public double activity;
        public double target;
        public string groupBox1;
        public string groupBox2;
        public string groupBox3;
        public double squirrels = 0.3;
        public double fats = 0.3;
        public double carbohydrates = 0.4;

        /// <summary>
        /// Расчет по формуле Харриса-Бенедикта
        /// </summary>
        /// <returns></returns>
        public int calculation()
        {
            double k2 = (groupBox1 == "radioButton1") ? 9.247 : 13.397;
            double k3 = (groupBox1 == "radioButton1") ? 3.098 : 4.799;
            double k4 = (groupBox1 == "radioButton1") ? 4.330 : 5.677;
            return (int)Math.Round((gender + (k2 * (float)weight) + (k3 * (float)height) - (k4 * (float)age)) * activity * target);
        }

    }

}
