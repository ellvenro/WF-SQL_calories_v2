using System;
using System.Windows.Forms;
using System.Data.OleDb;

namespace WindowsFormsApp1
{
    public partial class AddDietForm : Form
    {
        /// <summary>
        /// Выбранный прием пищи
        /// </summary>
        public string selectIndexComboBox;

        /// <summary>
        /// Выбранная дата
        /// </summary>
        public string selectIndexDateTimePicker;

        /// <summary>
        /// Выбранная дата
        /// </summary>
        public string selectIndexDateTimePickerInsert;

        /// <summary>
        /// Поле - ссылка на экземпляр класса OleDbConnection для соединения с БД
        /// </summary>
        public OleDbConnection myConnection;

        string connectString;

        /// <summary>
        /// Загрузка таблицы
        /// </summary>
        public bool IsLoad = false;

        /// <summary>
        /// Созранение истории
        /// </summary>
        private void SavingTableChanges()
        {
            string query;
            OleDbCommand command;
            try
            {
                //Получение индекса приема пищи
                string indexMeal = GetIndexMeal();
                OleDbDataReader reader = null;

                //Удаление продуктов конкретной даты и категории
                query = $"DELETE * FROM Diet WHERE ((Diet.ddate=#{selectIndexDateTimePicker}#) AND (Diet.id_m={indexMeal}))";
                command = new OleDbCommand(query, myConnection);
                command.ExecuteNonQuery();

                //Добавление продуктов конкретной даты и категории
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {

                    if (dataGridView1[0, i].Value == null || dataGridView1[1, i].Value == null)
                        continue;
                    query = $"SELECT Products.id_p FROM Products WHERE Products.product=\"{dataGridView1.Rows[i].Cells[1].Value.ToString()}\" ";
                    command = new OleDbCommand(query, myConnection);
                    reader = command.ExecuteReader();
                    reader.Read();
                    query = "INSERT INTO Diet ( ddate, id_m, id_p, weight )" +
                               $"VALUES (#{selectIndexDateTimePicker}#, \"{indexMeal}\", \"{reader[0].ToString()}\", \"{dataGridView1.Rows[i].Cells[2].Value.ToString()}\")";

                    command = new OleDbCommand(query, myConnection);
                    command.ExecuteNonQuery();
                }
                if (reader != null)
                    reader.Close();

            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Подсчет итоговой суммы
        /// </summary>
        public void CalculatingTheTotal()
        {
            try
            {
                //Подсчет норм за прием пищи
                //Очистка таблицы с итогами
                dataGridView2.Rows.Clear();
                float sum = 0; float sumb = 0; float sumg = 0; float sumu = 0;
                //Суммирование параметров
                for (int i = 0; i < dataGridView1.RowCount - 1; i++)
                {
                    if (dataGridView1[3, i].Value.ToString() != "")
                        sum += float.Parse(dataGridView1[3, i].Value.ToString());
                    if (dataGridView1[4, i].Value.ToString() != "")
                        sumb += float.Parse(dataGridView1[4, i].Value.ToString());
                    if (dataGridView1[5, i].Value.ToString() != "")
                        sumg += float.Parse(dataGridView1[5, i].Value.ToString());
                    if (dataGridView1[6, i].Value.ToString() != "")
                        sumu += float.Parse(dataGridView1[6, i].Value.ToString());
                }

                //Заполнение таблицы итогов с округлением до второй цифры после запятой
                dataGridView2.Rows.Add(selectIndexComboBox, 
                    Math.Round(sum,2).ToString(), 
                    Math.Round(sumb, 2).ToString(), 
                    Math.Round(sumg, 2).ToString(), 
                    Math.Round(sumu, 2).ToString());

                //Получение индекса приема пищи
                string indexMeal = GetIndexMeal();

                //Подсчет норм за день
                //Получение данных за день
                string query = $"SELECT Sum([kkal]*[weight]/100), Sum([squirrels]*[weight]/100), Sum([fats]*[weight]/100), Sum([carbohydrates]*[weight]/100) " +
                    $"FROM Products INNER JOIN Diet ON Products.id_p = Diet.id_p " +
                    $"GROUP BY Diet.ddate, Diet.id_m HAVING(((Diet.ddate) =#{selectIndexDateTimePicker}#) AND ((Diet.id_m)<>{indexMeal}))";
                OleDbCommand command = new OleDbCommand(query, myConnection);
                OleDbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    sum += float.Parse(reader[0].ToString());
                    sumb += float.Parse(reader[1].ToString());
                    sumg += float.Parse(reader[2].ToString());
                    sumu += float.Parse(reader[3].ToString());
                }
                dataGridView2.Rows.Add("Всего за день",
                    Math.Round(sum, 2).ToString(),
                    Math.Round(sumb, 2).ToString(),
                    Math.Round(sumg, 2).ToString(),
                    Math.Round(sumu, 2).ToString());
                reader.Close();

            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Получение индекса приема пищи
        /// </summary>
        /// <returns>Индекс приема пищи</returns>
        public string GetIndexMeal()
        {
            string query;
            OleDbCommand command;
            string indexMeal;
            try
            {
                //Получение индекса приема пищи
                query = $"SELECT Meals.id_m FROM Meals WHERE Meals.meal=\"{selectIndexComboBox}\"";
                command = new OleDbCommand(query, myConnection);
                OleDbDataReader reader = command.ExecuteReader();
                reader.Read();
                indexMeal = reader[0].ToString();
                reader.Close();
            }
            catch (Exception exp)
            {
                indexMeal = "";
            }
            return indexMeal;
        }

        /// <summary>
        /// Инициализация формы
        /// </summary>
        public AddDietForm(string connectString)
        {
            InitializeComponent();
            selectIndexDateTimePicker = dateTimePicker1.Value.Date.ToString("MM/dd/yyy").Replace('.', '/');
            selectIndexDateTimePickerInsert = dateTimePicker1.Value.Date.ToString("dd/MM/yyy").Replace('.', '/');
            this.connectString = connectString;
            

            InitForm();
        }

        /// <summary>
        /// Инициальзация
        /// </summary>
        /// <param name="i"></param>
        public void InitForm(int i = 0)
        {
            //Соединение с базой данныъ
            try
            {
                myConnection = new OleDbConnection(connectString);
                myConnection.Open();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка подключения к базе данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show("Ошибка подключения к базе данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
            //Заполнение списка приемов пищи
            string query = "SELECT meal FROM Meals";
            OleDbCommand command = new OleDbCommand(query, myConnection);
            OleDbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                comboBox1.Items.Add(reader[0].ToString());
            }
            reader.Close();
            comboBox1.SelectedIndex = i;

            //Заполнение списка категорий в таблице
            query = "SELECT category FROM FCategories ORDER BY FCategories.category";
            command = new OleDbCommand(query, myConnection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Column1.Items.Add(reader[0].ToString());
            }
            reader.Close();

            IsLoad = true;
        }

        /// <summary>
        /// Поиск в поле о списком
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            ((ComboBox)(sender)).DroppedDown = true;
            if ((char.IsControl(e.KeyChar)))
                return;
            string Str = ((ComboBox)(sender)).Text.Substring(0, ((ComboBox)(sender)).SelectionStart) + e.KeyChar;
            int Index = ((ComboBox)(sender)).FindStringExact(Str);
            if (Index == -1)
                Index = ((ComboBox)(sender)).FindString(Str);
            ((ComboBox)sender).SelectedIndex = Index;
            ((ComboBox)(sender)).SelectionStart = Str.Length;
            ((ComboBox)(sender)).SelectionLength = ((ComboBox)(sender)).Text.Length - ((ComboBox)(sender)).SelectionStart;
            e.Handled = true;
        }

        /// <summary>
        /// Изменение значения ячейки таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (e.ColumnIndex == 0)
            {
                //Заполнение списка продуктов
                string cb = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                string query = $"SELECT Products.product FROM FCategories INNER JOIN Products ON FCategories.id_fc = Products.id_fc WHERE FCategories.category=\"{cb}\" ORDER BY Products.product";
                OleDbCommand command = new OleDbCommand(query, myConnection);
                OleDbDataReader reader = command.ExecuteReader();
                DataGridViewComboBoxCell comboCell = new DataGridViewComboBoxCell();
                while (reader.Read())
                {
                    comboCell.Items.Add(reader[0].ToString());
                }
                dataGridView1.Rows[e.RowIndex].Cells[1] = comboCell;
                comboCell = null;
                reader.Close();
                for (int j = 2; j <= 6; j++)
                    dataGridView1.Rows[e.RowIndex].Cells[j].Value = "0";
            }
            else if (e.ColumnIndex == 1)
            {
                // Заполнение при изменении продукта
                string cb = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                if (cb != "")
                {
                    string query = $"SELECT Products.kkal, Products.squirrels, Products.fats, Products.carbohydrates FROM Products WHERE Products.product=\"{cb}\"";
                    OleDbCommand command = new OleDbCommand(query, myConnection);
                    OleDbDataReader reader = command.ExecuteReader();
                    reader.Read();
                    if (reader.HasRows)
                    {
                        //Записать граммы
                        dataGridView1.Rows[e.RowIndex].Cells[2].Value = "100";
                        //запись балки, жиры, углеводы
                        for (int j = 3; j<=6; j++)
                        {
                            dataGridView1.Rows[e.RowIndex].Cells[j].Value = reader[j-3].ToString();
                        }

                    }
                    reader.Close();
                }

            }
            else if (e.ColumnIndex == 2)
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString() != "0")
                {
                    //Заполнение при изменении граммов
                    string cb = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                    string query = $"SELECT Products.kkal, Products.squirrels, Products.fats, Products.carbohydrates FROM Products WHERE Products.product=\"{cb}\"";
                    OleDbCommand command = new OleDbCommand(query, myConnection);
                    OleDbDataReader reader = command.ExecuteReader();
                    reader.Read();
                    try
                    {
                        if (cb != "" && reader.HasRows)
                        {
                            float ccalBuf = float.Parse(dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString()) *
                               float.Parse(reader[0].ToString()) / 100;
                            dataGridView1.Rows[e.RowIndex].Cells[3].Value = Math.Round(ccalBuf, 2).ToString();
                        }
                        for (int j = 1; j <=3;j++)
                        {
                            if (reader[j].ToString() != "")
                            {
                                float ccalBelk = float.Parse(dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString()) *
                                    float.Parse(reader[j].ToString()) / (float)100;
                                dataGridView1.Rows[e.RowIndex].Cells[j+3].Value = Math.Round(ccalBelk, 2).ToString();
                            }
                            else
                                dataGridView1.Rows[e.RowIndex].Cells[j+3].Value = "0";
                        }
                        reader.Close();
                        CalculatingTheTotal();
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show(exp.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Удаление строки из таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                char number = e.KeyChar;
                if (e.KeyChar == 127)
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Выбрана пустая строка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Изменение приема пищи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsLoad) 
                SavingTableChanges();
            IsLoad = true;
            dataGridView1.Rows.Clear();
            selectIndexComboBox = comboBox1.SelectedItem.ToString();

            string query = $"SELECT FCategories.category, Products.product, Diet.weight, Products.kkal, Products.squirrels, Products.fats, Products.carbohydrates " +
                $"FROM(FCategories INNER JOIN Products ON FCategories.id_fc = Products.id_fc) " +
                $"INNER JOIN(Meals INNER JOIN Diet ON Meals.id_m = Diet.id_m) ON Products.id_p = Diet.id_p " +
                $"WHERE((Diet.ddate =#{selectIndexDateTimePicker}#) AND (Meals.meal=\"{selectIndexComboBox}\"))";
            OleDbCommand command = new OleDbCommand(query, myConnection);
            OleDbDataReader reader = command.ExecuteReader();
            //Если дань продолжается, то заполнить имеющимися значениями
            if (reader.HasRows)
            {
                int i = 0;
                while (reader.Read())
                {
                    dataGridView1.Rows.Add("", "", "", "", "", "", "");
                    dataGridView1.Rows[i].Cells[0].Value = reader[0].ToString();
                    dataGridView1.Rows[i].Cells[1].Value = reader[1].ToString();
                    dataGridView1.Rows[i].Cells[2].Value = reader[2].ToString();
                    for (int j = 3; j <= 6; j++)
                    {
                        dataGridView1.Rows[i].Cells[j].Value = Math.Round(float.Parse(reader[j].ToString()) * float.Parse(reader[2].ToString()) / 100, 2).ToString();
                    }
                    i++;
                }
                reader.Close();
            }
            command = null;
            CalculatingTheTotal();

        }

        /// <summary>
        /// Изменение даты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            SavingTableChanges();
            selectIndexDateTimePicker = dateTimePicker1.Value.Date.ToString("MM/dd/yyy").Replace('.', '/');
            selectIndexDateTimePickerInsert = dateTimePicker1.Value.Date.ToString("dd/MM/yyy").Replace('.', '/');
            dataGridView1.Rows.Clear();
            IsLoad = false;
            comboBox1.SelectedIndex = 1;
            comboBox1.SelectedIndex = 0;
        }

        /// <summary>
        /// Закрытие формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDietForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavingTableChanges();
            myConnection.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Отчистка приема пищи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            string query;
            OleDbCommand command;

            //Получение индекса приема пищи
            string indexMeal = GetIndexMeal();

            //Удаление продуктов конкретной даты и категории
            query = $"DELETE * FROM Diet WHERE ((Diet.ddate=#{selectIndexDateTimePicker}#) AND (Diet.id_m={indexMeal}))";
            command = new OleDbCommand(query, myConnection);
            command.ExecuteNonQuery();

            dataGridView1.Rows.Clear();
            SavingTableChanges();
        }

        /// <summary>
        /// отчистка дня
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            string query;
            OleDbCommand command;

            //Удаление продуктов конкретной даты и категории
            query = $"DELETE * FROM Diet WHERE (Diet.ddate=#{selectIndexDateTimePicker}#)";
            command = new OleDbCommand(query, myConnection);
            command.ExecuteNonQuery();

            dataGridView1.Rows.Clear();
            SavingTableChanges();
            CalculatingTheTotal();
        }

        /// <summary>
        /// Открытие отчета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            SavingTableChanges();
            CalculatingTheTotal();
            ReportForm form = new ReportForm(connectString);
            form.Show();
        }

        /// <summary>
        /// Открытие формы добавления категории
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            DeleteRows();
            CategoryForm form = new CategoryForm(connectString);
            form.ShowDialog();
            Filling();
        }

        /// <summary>
        /// Добавление продуктов 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            DeleteRows();
            AddingProductsForm form = new AddingProductsForm(connectString);
            form.ShowDialog();
            Filling();
        }

        /// <summary>
        /// Удаление строк с пустыми значениями
        /// </summary>
        public void DeleteRows()
        {

            for (int i = 0; i < dataGridView1.RowCount - 1; i++)
            {
                if (string.IsNullOrWhiteSpace(dataGridView1[1, i].Value?.ToString()))
                {
                    dataGridView1.Rows.RemoveAt(i);
                    i--;
                }
            }
            SavingTableChanges();
            CalculatingTheTotal();
        }

        /// <summary>
        /// Заполнение данных после закрытия дочерних форм
        /// </summary>
        public void Filling()
        {
            comboBox1.Items.Clear();
            Column1.Items.Clear();
            myConnection.Close();
            InitForm(comboBox1.SelectedIndex);
        }
    }
}
