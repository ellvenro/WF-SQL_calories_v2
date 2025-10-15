using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.IO;
using System.Data.OleDb;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Данные пользователя
        /// </summary>
        OptionsClass options = new OptionsClass();

        /// <summary>
        /// Строка подключения к MS Access
        /// </summary>
        public static string connectString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=DB.mdb;";

        /// <summary>
        /// Файл для сохранения
        /// </summary>
        public static string fileNameString = "options.txt";

        /// <summary>
        /// Поле - ссылка на экземпляр класса OleDbConnection для соединения с БД
        /// </summary>
        public OleDbConnection myConnection;

        public MainForm()
        {
            InitializeComponent();
            PrintText();

        }

        /// <summary>
        /// Вывод суточной нормы
        /// </summary>
        private void PrintText()
        {
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
                int kkal = options.calculation();
                label1.Text = "Ваша суточная норма калорий: ";
                label4.Text = kkal.ToString() + " кКал";

                label2.Text = "Из которых";
                label5.Text = "Белки:\n" + (kkal * options.squirrels).ToString() + " г";
                label6.Text = "Жиры:\n" + (kkal * options.fats).ToString() + " г";
                label7.Text = "Углеводы:\n" + (kkal * options.carbohydrates).ToString() + " г";
            }
            else
            {
                label1.Text = "Введите свои данные в пункте \"Общая информация\", \nчтобы узнать свою суточную норму калорий";
                label2.Text = "";
                label4.Text = "";
                label5.Text = "";
                label6.Text = "";
                label7.Text = "";
            }

        }

        /// <summary>
        /// Открытие формы с обзей информацией
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            InformationForm form = new InformationForm(fileNameString);
            form.ShowDialog();
            PrintText();
        }

        /// <summary>
        /// Открытие формы дневника питания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            AddDietForm form = new AddDietForm(connectString);
            form.Show();
        }

        /// <summary>
        /// Открытие формы добавления категорий
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItem31_Click_1(object sender, EventArgs e)
        {
            CategoryForm form = new CategoryForm(connectString);
            form.Show();
        }

        /// <summary>
        /// Открфтие формы добавлени продуктов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItem32_Click(object sender, EventArgs e)
        {
            AddingProductsForm form = new AddingProductsForm(connectString);
            form.Show();
        }

        /// <summary>
        /// Хакрытие формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Открытие отчета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            ReportForm form = new ReportForm(connectString);
            form.Show();
        }
    }
}
