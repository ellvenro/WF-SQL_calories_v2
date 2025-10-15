using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace WindowsFormsApp1
{
    public partial class ReportForm : Form
    {

        /// <summary>
        /// Поле - ссылка на экземпляр класса OleDbConnection для соединения с БД
        /// </summary>
        public OleDbConnection myConnection;

        public ReportForm(string connectString)
        {
            InitializeComponent();

            //Соединение с базой данныъ
            try
            {
                myConnection = new OleDbConnection(connectString);
                myConnection.Open();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка подключения к базе данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }

            LoadData();
        }

        /// <summary>
        /// Изменение даты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// Загрузка таблицы
        /// </summary>
        public void LoadData()
        {
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            int n = 4;
            float[] sum1 = new float[n];
            float[] sum2 = new float[n];

            for (int i = 0; i < n; i++)
                sum2[i] = 0;

            //Получение даты
            var selectIndexDateTimePicker = dateTimePicker1.Value.Date.ToString("MM/dd/yyy").Replace('.', '/');
            //Запрос на выборку
            string query = "SELECT meal FROM Meals";
            OleDbCommand command = new OleDbCommand(query, myConnection);
            OleDbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {

                for (int i = 0; i < n; i++)
                    sum1[i] = 0;

                
                //Запрос на выборку
                string query1 = $"SELECT Products.product, Diet.weight, Products.kkal, Products.squirrels, Products.fats, Products.carbohydrates " +
                    $"FROM(FCategories INNER JOIN Products ON FCategories.id_fc = Products.id_fc) INNER JOIN(Meals INNER JOIN Diet ON Meals.id_m = Diet.id_m) ON Products.id_p = Diet.id_p " +
                    $"WHERE(((Meals.meal) = \"{reader[0].ToString()}\") AND((Diet.ddate) =#{selectIndexDateTimePicker}#))";
                OleDbCommand command1 = new OleDbCommand(query1, myConnection);
                OleDbDataReader reader1 = command1.ExecuteReader();
                if (reader1.HasRows)
                {
                    reader1.Read();
                    dataGridView1.Rows.Add(reader[0].ToString(), reader1[0].ToString(),
                            reader1[1].ToString(),
                            Math.Round(float.Parse(reader1[1].ToString()) * float.Parse(reader1[2].ToString()) / 100, 2).ToString(),
                            Math.Round(float.Parse(reader1[1].ToString()) * float.Parse(reader1[3].ToString()) / 100, 2).ToString(),
                            Math.Round(float.Parse(reader1[1].ToString()) * float.Parse(reader1[4].ToString()) / 100, 2).ToString(),
                            Math.Round(float.Parse(reader1[1].ToString()) * float.Parse(reader1[5].ToString()) / 100, 2).ToString());

                    while (reader1.Read())
                    {
                        for (int i = 0; i < n; i++)
                            sum1[i] += (float)Math.Round(float.Parse(reader1[1].ToString()) * float.Parse(reader1[i + 2].ToString()) / 100, 2);

                        dataGridView1.Rows.Add("", reader1[0].ToString(),
                            reader1[1].ToString(),
                            Math.Round(float.Parse(reader1[1].ToString()) * float.Parse(reader1[2].ToString()) / 100, 2).ToString(),
                            Math.Round(float.Parse(reader1[1].ToString()) * float.Parse(reader1[3].ToString()) / 100, 2).ToString(),
                            Math.Round(float.Parse(reader1[1].ToString()) * float.Parse(reader1[4].ToString()) / 100, 2).ToString(),
                            Math.Round(float.Parse(reader1[1].ToString()) * float.Parse(reader1[5].ToString()) / 100, 2).ToString());
                    }
                    reader1.Close();


                    dataGridView1.Rows.Add("", "Итого", "", sum1[0], sum1[1], sum1[2], sum1[3]);
                }

                for (int i = 0; i < n; i++)
                    sum2[i] += sum1[i];
            }
            reader.Close();
            dataGridView2.Rows.Add("Итого за день", sum2[0], sum2[1], sum2[2], sum2[3]);
        }

        /// <summary>
        /// Закрытие формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReportForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            myConnection.Close();
        }

        /// <summary>
        /// Закрытие формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
