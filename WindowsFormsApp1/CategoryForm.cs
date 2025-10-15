using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.OleDb;

namespace WindowsFormsApp1
{
    public partial class CategoryForm : Form
    {
        /// <summary>
        /// Поле - ссылка на экземпляр класса OleDbConnection для соединения с БД
        /// </summary>
        public OleDbConnection myConnection;

        Point LocationTextBox;
        Point LocationComboBox;
        Point LocationPictureBox;
        int deltaX;
        int deltaY;

        public CategoryForm(string connectString)
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

            //Заполнение списка приемов пищи
            string query = "SELECT category FROM FCategories ORDER BY FCategories.category";
            OleDbCommand command = new OleDbCommand(query, myConnection);
            OleDbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                comboBox1.Items.Add(reader[0].ToString());
            }
            reader.Close();

            label2.Visible = false;
            label3.Visible = false;
            comboBox1.Visible = false;
            textBox1.Visible = false;
            //Данные о местоположениях объектов
            LocationTextBox = textBox1.Location;
            LocationComboBox = comboBox1.Location;
            LocationPictureBox = pictureBox2.Location;
            deltaX = LocationTextBox.X - LocationPictureBox.X;
            deltaY = LocationTextBox.Y - LocationPictureBox.Y;
        }

        /// <summary>
        /// Выбор добавления элемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            label2.Visible = true;
            label3.Visible = false;
            textBox1.Visible = true;
            comboBox1.Visible = false;
            pictureBox2.Visible = true;
            pictureBox2.Location = LocationComboBox;
            textBox1.Location = new Point(LocationComboBox.X + deltaX, LocationComboBox.Y + deltaY);
            button1.Text = "Добавить";
            label2.Text = "Новая категория:";
        }

        /// <summary>
        /// Выбор удаления элемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            label2.Visible = true;
            label3.Visible = false;
            textBox1.Visible = false;
            comboBox1.Visible = true;
            pictureBox2.Visible = false;
            button1.Text = "Удалить";
            label2.Text = "Выбор категории:";
        }

        /// <summary>
        /// Выбор переименования элемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            label2.Visible = true;
            label3.Visible = true;
            textBox1.Visible = true;
            comboBox1.Visible = true;
            pictureBox2.Visible = true;
            pictureBox2.Location = LocationPictureBox;
            textBox1.Location = LocationTextBox;
            button1.Text = "Заменить";
            label2.Text = "Выбор категории:";
            label3.Text = "Заменить на:";
        }

        /// <summary>
        /// Нажатие кнопки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string query;
                OleDbCommand command;
                OleDbDataReader reader;
                //Добавление
                if (radioButton1.Checked)
                {
                    if (textBox1.Text == "")
                        throw new Exception("Введите данные");
                    query = $"SELECT FCategories.category FROM FCategories WHERE(FCategories.category = \"{textBox1.Text}\")";
                    command = new OleDbCommand(query, myConnection);
                    reader = command.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        query = $"INSERT INTO FCategories ( category ) VALUES (\"{textBox1.Text}\" )";
                        command = new OleDbCommand(query, myConnection);
                        command.ExecuteNonQuery();
                    }
                    reader.Close();
                }
                //Удаление
                else if (radioButton2.Checked)
                {
                    if (comboBox1.SelectedItem.ToString() == "")
                        throw new Exception("Введите данные");
                    query = $"DELETE * FROM FCategories WHERE category = \"{comboBox1.SelectedItem.ToString()}\"";
                    command = new OleDbCommand(query, myConnection);
                    command.ExecuteNonQuery();
                }
                //Изменение
                else if (radioButton3.Checked)
                {
                    if (textBox1.Text == "" || comboBox1.SelectedItem.ToString() == "")
                        throw new Exception("Введите данные");
                    query = "";
                    command = new OleDbCommand(query, myConnection);
                    command.CommandText = ($"UPDATE FCategories SET " +
                                    $"category = \"{textBox1.Text}\" " +
                                    $" WHERE category=\"{comboBox1.SelectedItem.ToString()}\"");
                    
                }
                myConnection.Close();
                Close();
                
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Закрытие формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            myConnection.Close();
            Close();
        }

        /// <summary>
        /// Поиск в поле со списком
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
    }
}
