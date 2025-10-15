using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class InformationForm : Form
    {

        /// <summary>
        /// Данные пользователя
        /// </summary>
        OptionsClass options = new OptionsClass();

        /// <summary>
        /// Строка подключения к MS Access
        /// </summary>
        public string fileNameString;

        /// <summary>
        /// Инициализация формы
        /// </summary>
        /// <param name="fileNameString"></param>
        public InformationForm(string fileNameString)
        {
            this.fileNameString = fileNameString;
            InitializeComponent();
            //Добавление подписи к радиобаттон
            ToolTip toolTipRB3 = new ToolTip();
            toolTipRB3.SetToolTip(radioButton3, "Редко выхожу из дома, почти весь день сижу");
            ToolTip toolTipRB4 = new ToolTip();
            toolTipRB4.SetToolTip(radioButton4, "Хожу в магазин или недолго прогуливаюсь");
            ToolTip toolTipRB5 = new ToolTip();
            toolTipRB5.SetToolTip(radioButton5, "Ежедневно гуляю не меньше часа");
            ToolTip toolTipRB6 = new ToolTip();
            toolTipRB6.SetToolTip(radioButton6, "Занимаюсь активными видами спорта/досуга (велосипед, ролики, лыжи, коньки и др.) 2-3 раза в неделю");
            ToolTip toolTipRB7 = new ToolTip();
            toolTipRB7.SetToolTip(radioButton7, "Регулярно занимаюсь спортом (бег, гимнастика, тренажерный зал), минимум 5 раз в неделю");

            //Считывание из файла
            Stream stream = null;
            IFormatter formatter = null;
            bool flag = false;
            try
            {
                formatter = new BinaryFormatter();
                stream = new FileStream(fileNameString, FileMode.Open, FileAccess.Read);
                flag = true;
            }
            catch (Exception exp)
            {
                flag = false;
            } 
            //заполнение полей
            if (flag)
            {
                options = (OptionsClass)formatter.Deserialize(stream);
                stream.Close();

                RadioButton rb = groupBox1.Controls[options.groupBox1] as RadioButton;

                (groupBox1.Controls[options.groupBox1] as RadioButton).Checked = true;
                (groupBox2.Controls[options.groupBox2] as RadioButton).Checked = true;
                (groupBox3.Controls[options.groupBox3] as RadioButton).Checked = true;

                textBox1.Text = options.age.ToString();
                textBox2.Text = options.height.ToString();
                textBox3.Text = options.weight.ToString();

            }

            textBox4.Text = options.squirrels.ToString();
            textBox5.Text = options.fats.ToString();
            textBox6.Text = options.carbohydrates.ToString();
        }


        /// <summary>
        /// Кнопка расчета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //Считывание пола
                var buttons = groupBox1.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked);
                var name = buttons.Name;

                //Выбор коэффициентов в зависимости от пола
                if (name == "radioButton1")
                {
                    options.gender = 447.593;
                    options.groupBox1 = "radioButton1";
                }
                else
                {
                    options.gender = 88.362;
                    options.groupBox1 = "radioButton2";
                }

                //Считывание возраста, роста и веса
                options.age = int.Parse(textBox1.Text);
                options.height = int.Parse(textBox2.Text);
                options.weight = int.Parse(textBox3.Text);

                //Считывание норм БЖУ
                options.squirrels = Math.Round(float.Parse(textBox4.Text),2);
                options.fats = Math.Round(float.Parse(textBox5.Text), 2);
                options.carbohydrates = Math.Round(float.Parse(textBox6.Text), 2);

                //Считывание активности
                buttons = groupBox2.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked);
                name = buttons.Name;

                //Выбор коэффициента активности
                switch (name)
                {
                    case "radioButton3":
                        options.activity = 1.2;
                        options.groupBox2 = "radioButton3";
                        break;
                    case "radioButton4":
                        options.activity = 1.375;
                        options.groupBox2 = "radioButton4";
                        break;
                    case "radioButton5":
                        options.activity = 1.55;
                        options.groupBox2 = "radioButton5";
                        break;
                    case "radioButton6":
                        options.activity = 1.725;
                        options.groupBox2 = "radioButton6";
                        break;
                    case "radioButton7":
                        options.activity = 1.9;
                        options.groupBox2 = "radioButton7";
                        break;

                }

                //Считывание цели
                buttons = groupBox3.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked);
                name = buttons.Name;
                
                //Выбор коэффициента в зависимости от цели
                switch (name)
                {
                    case "radioButton8":
                        options.target = 0.8;
                        options.groupBox3 = "radioButton8";
                        break;
                    case "radioButton9":
                        options.target = 1;
                        options.groupBox3 = "radioButton9";
                        break;
                    case "radioButton10":
                        options.target = 1.2;
                        options.groupBox3 = "radioButton10";
                        break;

                }
                //Созранение в файл
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(fileNameString, FileMode.Create, FileAccess.Write);
                formatter.Serialize(stream, options);
                stream.Close();
                
            }
            catch (Exception exp)
            {
                MessageBox.Show("Проверьте корректность введенных данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Close();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8 && number != ',')
            {
                e.Handled = true;
            }
        }
    }
}
