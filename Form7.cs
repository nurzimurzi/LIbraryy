using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace FormLibr
{
    public partial class Form7 : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Libraryy;Integrated Security=True";
        private int? editBookId = null; // если null - добавление, если есть значение - редактирование
        private string selectedImagePath = null;

        // Конструктор для добавления новой книги
        public Form7()
        {
            InitializeComponent();
            InitializeForm();
            LoadComboBoxes();
            label4.Text = "ДОБАВЛЕНИЕ КНИГИ";
        }

        // Конструктор для редактирования существующей книги
        public Form7(int bookId)
        {
            InitializeComponent();
            InitializeForm();
            this.editBookId = bookId;
            label4.Text = "РЕДАКТИРОВАНИЕ КНИГИ";
            LoadComboBoxes();
            LoadBookData(bookId);
        }

        private void InitializeForm()
        {
            // Настройка текстовых полей
            SetPlaceholder(textBox1, "Введите название книги");
            SetPlaceholder(textBox2, "Введите ISBN");
            SetPlaceholder(textBox3, "Введите количество страниц");
            SetPlaceholder(textBox4, "Введите количество экземпляров");
            SetPlaceholder(textBox5, "Введите описание книги");

            // Настройка числовых полей
            textBox3.KeyPress += NumericTextBox_KeyPress;
            textBox4.KeyPress += NumericTextBox_KeyPress;

            // Подписка на события
            button1.Click += BtnLoadImage_Click;
            button2.Click += BtnSave_Click;
            button3.Click += BtnCancel_Click;
            button7.Click += BtnBack_Click;

            button5.Click += (s, e) => AddNewItem("Author", comboBox2);
            button4.Click += (s, e) => AddNewItem("Publisher", comboBox3);
            button6.Click += (s, e) => AddNewItem("Genres", comboBox1);
        }

        private void SetPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;
            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                }
            };
            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // Загрузка данных в комбобоксы
        private void LoadComboBoxes()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Загрузка жанров
                    DataTable genresTable = new DataTable();
                    new SqlDataAdapter("SELECT Id, NameGenres FROM Genres ORDER BY NameGenres", connection).Fill(genresTable);
                    comboBox1.DisplayMember = "NameGenres";
                    comboBox1.ValueMember = "Id";
                    comboBox1.DataSource = genresTable;
                    comboBox1.SelectedIndex = -1;

                    // Загрузка авторов
                    DataTable authorsTable = new DataTable();
                    new SqlDataAdapter("SELECT Id, NameAuthor FROM Author ORDER BY NameAuthor", connection).Fill(authorsTable);
                    comboBox2.DisplayMember = "NameAuthor";
                    comboBox2.ValueMember = "Id";
                    comboBox2.DataSource = authorsTable;
                    comboBox2.SelectedIndex = -1;

                    // Загрузка издательств
                    DataTable publishersTable = new DataTable();
                    new SqlDataAdapter("SELECT Id, NamePublisher FROM Publisher ORDER BY NamePublisher", connection).Fill(publishersTable);
                    comboBox3.DisplayMember = "NamePublisher";
                    comboBox3.ValueMember = "Id";
                    comboBox3.DataSource = publishersTable;
                    comboBox3.SelectedIndex = -1;

                    // Загрузка возрастных ограничений
                    DataTable ageLimitTable = new DataTable();
                    new SqlDataAdapter("SELECT Id, Name FROM AgeLimit ORDER BY Id", connection).Fill(ageLimitTable);
                    comboBox4.DisplayMember = "Name";
                    comboBox4.ValueMember = "Id";
                    comboBox4.DataSource = ageLimitTable;
                    comboBox4.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки справочников: " + ex.Message);
            }
        }

        // Загрузка данных книги для редактирования
        private void LoadBookData(int bookId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT * FROM Book WHERE Id = @bookId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@bookId", bookId);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        textBox1.Text = reader["NameBook"].ToString();
                        textBox2.Text = reader["ISBN"].ToString();
                        textBox3.Text = reader["PageCount"].ToString();
                        textBox4.Text = reader["TotalCopies"].ToString();
                        textBox5.Text = reader["Description"].ToString();

                        comboBox1.SelectedValue = reader["GenresId"];
                        comboBox2.SelectedValue = reader["AuthorId"];
                        comboBox3.SelectedValue = reader["PublisherId"];
                        comboBox4.SelectedValue = reader["AgeLimitId"];

                        if (reader["CoverImage"] != DBNull.Value)
                        {
                            selectedImagePath = reader["CoverImage"].ToString();
                            if (System.IO.File.Exists(selectedImagePath))
                            {
                                pictureBox1.ImageLocation = selectedImagePath;
                                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки книги: " + ex.Message);
            }
        }

        // Загрузка обложки
        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            openFileDialog.Title = "Выберите обложку книги";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = openFileDialog.FileName;
                pictureBox1.ImageLocation = selectedImagePath;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        // Добавление нового элемента в справочник
        private void AddNewItem(string tableName, ComboBox comboBox)
        {
            string prompt = "";
            string fieldName = "";

            switch (tableName)
            {
                case "Author":
                    prompt = "Введите имя автора:";
                    fieldName = "NameAuthor";
                    break;
                case "Publisher":
                    prompt = "Введите название издательства:";
                    fieldName = "NamePublisher";
                    break;
                case "Genres":
                    prompt = "Введите название жанра:";
                    fieldName = "NameGenres";
                    break;
            }

            string newValue = Microsoft.VisualBasic.Interaction.InputBox(prompt, "Добавление", "");

            if (!string.IsNullOrWhiteSpace(newValue))
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Получаем новый ID
                        string getIdQuery = $"SELECT ISNULL(MAX(Id), 0) + 1 FROM {tableName}";
                        SqlCommand getIdCmd = new SqlCommand(getIdQuery, connection);
                        int newId = (int)getIdCmd.ExecuteScalar();

                        // Вставляем новую запись
                        string insertQuery = $"INSERT INTO {tableName} (Id, {fieldName}) VALUES (@id, @name)";
                        SqlCommand insertCmd = new SqlCommand(insertQuery, connection);
                        insertCmd.Parameters.AddWithValue("@id", newId);
                        insertCmd.Parameters.AddWithValue("@name", newValue);
                        insertCmd.ExecuteNonQuery();

                        MessageBox.Show("Запись добавлена!");

                        // Перезагружаем комбобокс
                        LoadComboBoxes();

                        // Выбираем новый элемент
                        comboBox.SelectedValue = newId;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении: " + ex.Message);
                }
            }
        }

        // Сохранение книги
        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Проверка заполнения
            if (textBox1.Text == "Введите название книги" || string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Введите название книги!");
                return;
            }

            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите жанр!");
                return;
            }

            if (comboBox2.SelectedValue == null)
            {
                MessageBox.Show("Выберите автора!");
                return;
            }

            if (comboBox3.SelectedValue == null)
            {
                MessageBox.Show("Выберите издательство!");
                return;
            }

            if (comboBox4.SelectedValue == null)
            {
                MessageBox.Show("Выберите возрастное ограничение!");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (editBookId == null) // Добавление новой книги
                    {
                        // Получаем новый ID
                        string getIdQuery = "SELECT ISNULL(MAX(Id), 0) + 1 FROM Book";
                        SqlCommand getIdCmd = new SqlCommand(getIdQuery, connection);
                        int newId = (int)getIdCmd.ExecuteScalar();

                        string insertQuery = @"
                            INSERT INTO Book (Id, ISBN, NameBook, PageCount, AgeLimitId, RentalPrice, 
                                            PublisherId, AuthorId, GenresId, TotalCopies, Description, CoverImage)
                            VALUES (@id, @isbn, @name, @pages, @age, @price, 
                                    @publisher, @author, @genre, @copies, @description, @image)";

                        SqlCommand command = new SqlCommand(insertQuery, connection);
                        command.Parameters.AddWithValue("@id", newId);
                        command.Parameters.AddWithValue("@isbn", textBox2.Text == "Введите ISBN" ? "" : textBox2.Text);
                        command.Parameters.AddWithValue("@name", textBox1.Text);
                        command.Parameters.AddWithValue("@pages", Convert.ToInt32(textBox3.Text == "Введите количество страниц" ? "0" : textBox3.Text));
                        command.Parameters.AddWithValue("@age", comboBox4.SelectedValue);
                        command.Parameters.AddWithValue("@price", 0); // Временно
                        command.Parameters.AddWithValue("@publisher", comboBox3.SelectedValue);
                        command.Parameters.AddWithValue("@author", comboBox2.SelectedValue);
                        command.Parameters.AddWithValue("@genre", comboBox1.SelectedValue);
                        command.Parameters.AddWithValue("@copies", Convert.ToInt32(textBox4.Text == "Введите количество экземпляров" ? "0" : textBox4.Text));
                        command.Parameters.AddWithValue("@description", textBox5.Text == "Введите описание книги" ? "" : textBox5.Text);
                        command.Parameters.AddWithValue("@image", selectedImagePath ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                        MessageBox.Show("Книга успешно добавлена!");
                    }
                    else // Редактирование книги
                    {
                        string updateQuery = @"
                            UPDATE Book SET 
                                ISBN = @isbn,
                                NameBook = @name,
                                PageCount = @pages,
                                AgeLimitId = @age,
                                PublisherId = @publisher,
                                AuthorId = @author,
                                GenresId = @genre,
                                TotalCopies = @copies,
                                Description = @description,
                                CoverImage = @image
                            WHERE Id = @id";

                        SqlCommand command = new SqlCommand(updateQuery, connection);
                        command.Parameters.AddWithValue("@id", editBookId);
                        command.Parameters.AddWithValue("@isbn", textBox2.Text == "Введите ISBN" ? "" : textBox2.Text);
                        command.Parameters.AddWithValue("@name", textBox1.Text);
                        command.Parameters.AddWithValue("@pages", Convert.ToInt32(textBox3.Text == "Введите количество страниц" ? "0" : textBox3.Text));
                        command.Parameters.AddWithValue("@age", comboBox4.SelectedValue);
                        command.Parameters.AddWithValue("@price", 0);
                        command.Parameters.AddWithValue("@publisher", comboBox3.SelectedValue);
                        command.Parameters.AddWithValue("@author", comboBox2.SelectedValue);
                        command.Parameters.AddWithValue("@genre", comboBox1.SelectedValue);
                        command.Parameters.AddWithValue("@copies", Convert.ToInt32(textBox4.Text == "Введите количество экземпляров" ? "0" : textBox4.Text));
                        command.Parameters.AddWithValue("@description", textBox5.Text == "Введите описание книги" ? "" : textBox5.Text);
                        command.Parameters.AddWithValue("@image", selectedImagePath ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                        MessageBox.Show("Книга успешно обновлена!");
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox3_TextChanged(object sender, EventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}