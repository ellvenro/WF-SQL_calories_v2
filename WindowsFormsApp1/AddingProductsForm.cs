using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Data.OleDb;
using HtmlAgilityPack;

namespace WindowsFormsApp1
{
    public partial class AddingProductsForm : Form
    {
        /// <summary>
        /// Поле - ссылка на экземпляр класса OleDbConnection для соединения с БД
        /// </summary>
        public OleDbConnection myConnection;

        /// <summary>
        /// Таблица с данными сайта
        /// </summary>
        public string pageContent = "";

        bool findInDB = false;

        bool connectInternet = false;

        /// <summary>
        /// Проверка ячеек строки на пустоту
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool IsRowEmpty(DataGridViewRow row)
        {
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (string.IsNullOrWhiteSpace(cell.Value?.ToString()))
                {
                    // Если хотя бы одна ячейка не пуста, возвращаем false.
                    return false;
                }
            }
            // Если все ячейки пусты, возвращаем true.
            return true;
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public AddingProductsForm(string connectString)
        {
            InitializeComponent();
            //Получение данных сайта
            try
            {
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("https://1000.menu/food-table");
                HttpWebResponse myres = (HttpWebResponse)myReq.GetResponse();

                using (StreamReader sr = new StreamReader(myres.GetResponseStream()))
                {
                    pageContent = sr.ReadToEnd();
                }

                myReq = null;
                myres = null;

                string substring = "<table";
                pageContent = RemoveStartOfStringUntilSubstring(pageContent, substring);
                substring = "</table>";
                pageContent = RemoveAfterSubstring(pageContent, substring);
                connectInternet = true;
            }
            catch (Exception exp)
            {
                MessageBox.Show("Ошибка подключения к интернету", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                connectInternet = false;
            }

            //Соединение с базой данныъ
            try
            {
                myConnection = new OleDbConnection(connectString);
                myConnection.Open();
            }
            catch (Exception exp)
            {
                MessageBox.Show("Ошибка подключения к базе данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }

            //Заполнение списка приемов пищи
            string query = "SELECT category FROM FCategories ORDER BY FCategories.category";
            OleDbCommand command = new OleDbCommand(query, myConnection);
            OleDbDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Column1.Items.Add(reader[0].ToString());
            }
            reader.Close();

        }

        /// <summary>
        /// Удаление начала строки 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        public string RemoveStartOfStringUntilSubstring(string input, string substring)
        {
            int index = input.IndexOf(substring);

            // Если подстрока найдена, обрезаем строку
            if (index != -1)
            {
                return input.Substring(index);
            }

            // Если подстрока не найдена, возвращаем исходную строку
            return input;
        }

        /// <summary>
        /// Удаление конца строки
        /// </summary>
        /// <param name="input"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        public string RemoveAfterSubstring(string input, string substring)
        {
            int index = input.IndexOf(substring);

            // Если подстрока найдена, удаляем все после нее
            if (index != -1)
            {
                return input.Remove(index + substring.Length);
            }

            // Если подстрока не найдена, возвращаем исходную строку
            return input;
        }

        /// <summary>
        /// Нажатие кнопки поиска по БД
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            findInDB = true;
            dataGridView1.Rows.Clear();
            //Заполнение списка приемов пищи
            string query = $"SELECT FCategories.category, Products.product, Products.kkal, Products.squirrels, Products.fats, Products.carbohydrates " +
                $"FROM FCategories INNER JOIN Products ON FCategories.id_fc = Products.id_fc " +
                $"WHERE( Lcase(Products.product) Like Lcase(\"%{textBox1.Text}%\")) ORDER BY FCategories.category";
            OleDbCommand command = new OleDbCommand(query, myConnection);
            OleDbDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                dataGridView1.Rows.Add(reader[0].ToString(), reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString(), reader[5].ToString());
            }
            reader.Close();
        }

        /// <summary>
        /// Закрытие формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddingProductsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            myConnection.Close();
            pageContent = null;
        }

        /// <summary>
        /// Нажатие кнопки поиска по сайту
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            findInDB = false;
            dataGridView1.Rows.Clear();
            if (!connectInternet)
                return;
            if (pageContent.Contains(textBox1.Text))
            {
                var parser = new HtmlTableParser();
                string htmlContent = pageContent;
                string columnName = "Продукт"; // Имя колонки для поиска
                string substring = textBox1.Text; // Подстрока для поиска

                List<HtmlNode> foundRows = parser.FindRowsWithSubstring(htmlContent, columnName, substring);
                for (int i = 0; i < foundRows.Count; i++)
                {
                    // Получаем все ячейки в строке.
                    var cells = foundRows[i].SelectNodes(".//td").Select(td => td.InnerText).ToList();
                    dataGridView1.Rows.Add("", cells[0], cells[4], cells[1], cells[2], cells[3]);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            string query;
            OleDbCommand command;
            if (findInDB)
            {
                //Удаление продуктов, соответствующих поиску
                query = $"DELETE * FROM Products " +
                    $"WHERE( Products.product Like \"%{textBox1.Text}%\")";
                command = new OleDbCommand(query, myConnection);
                command.ExecuteNonQuery();
            }

            //Запись в БД
            for (int i = 0; i < dataGridView1.RowCount - 1; i++)
            {
                if (!IsRowEmpty(dataGridView1.Rows[i]))
                    continue;
                //Получение индекса категории пищи
                query = $"SELECT FCategories.id_fc FROM FCategories WHERE FCategories.category=\"{dataGridView1[0, i].Value.ToString()}\"";
                command = new OleDbCommand(query, myConnection);
                OleDbDataReader reader = command.ExecuteReader();
                reader.Read();
                var indexCategory = reader[0].ToString();
                reader.Close();

                query = $"SELECT Products.product FROM Products WHERE(Products.product = \"{dataGridView1[1, i].Value.ToString()}\")";
                command = new OleDbCommand(query, myConnection);
                reader = command.ExecuteReader();
                try
                {
                    if (!reader.HasRows)
                    {
                        query = "INSERT INTO Products ( id_fc, product, kkal, squirrels, fats, carbohydrates )" +
                                   $"VALUES ({indexCategory}, " +
                                   $"\"{dataGridView1[1, i].Value.ToString()}\", " +
                                   $"\"{dataGridView1[2, i].Value.ToString()}\", " +
                                   $"\"{dataGridView1[3, i].Value.ToString()}\", " +
                                   $"\"{dataGridView1[4, i].Value.ToString()}\", " +
                                   $"\"{dataGridView1[5, i].Value.ToString()}\")";

                        command = new OleDbCommand(query, myConnection);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        MessageBox.Show("Данный продукт уже существует", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    reader.Close();
                }
                catch (Exception exp)
                {
                    MessageBox.Show("Не верные данные, строка не добавлена", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            findInDB = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Отчистка
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            findInDB = false;
            dataGridView1.Rows.Clear();
        }
    }
}
