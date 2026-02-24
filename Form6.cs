using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FormLibr
{
    public partial class Form6 : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Libraryy;Integrated Security=True";
        private int userId;
        private int roleId;

        // Конструктор с параметрами
        public Form6(int userId, int roleId)
        {
            InitializeComponent();
            this.userId = userId;
            this.roleId = roleId;

            // Настраиваем текст в поиске
            textBox1.Text = "Введите название книги";
            textBox1.ForeColor = Color.Gray;
            textBox1.GotFocus += TextBox1_GotFocus;
            textBox1.LostFocus += TextBox1_LostFocus;

            // Загружаем жанры и книги
            LoadGenres();
            LoadBooks();
        }

        // ========== ЗАКРУГЛЕНИЕ КНОПОК ==========
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Закругляем все кнопки
            MakeButtonRounded(button1, 20); // Найти
            MakeButtonRounded(button2, 25); // Добавить
            MakeButtonRounded(button3, 20); // Назад

            // Можно сделать разные цвета
            button2.BackColor = Color.FromArgb(76, 175, 80); // Зеленая кнопка Добавить
        }

        private void MakeButtonRounded(Button btn, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            btn.Region = new Region(path);
        }

        // ========== РАБОТА С ТЕКСТОВЫМ ПОЛЕМ ==========
        private void TextBox1_GotFocus(object sender, EventArgs e)
        {
            if (textBox1.Text == "Введите название книги")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
            }
        }

        private void TextBox1_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Введите название книги";
                textBox1.ForeColor = Color.Gray;
            }
        }

        // ========== ЗАГРУЗКА ЖАНРОВ ==========
        private void LoadGenres()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Id, NameGenres FROM Genres ORDER BY NameGenres";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    DataTable genresTable = new DataTable();
                    genresTable.Load(reader);

                    DataRow allRow = genresTable.NewRow();
                    allRow["Id"] = 0;
                    allRow["NameGenres"] = "Все жанры";
                    genresTable.Rows.InsertAt(allRow, 0);

                    comboBox1.DisplayMember = "NameGenres";
                    comboBox1.ValueMember = "Id";
                    comboBox1.DataSource = genresTable;

                    comboBox1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки жанров: " + ex.Message);
            }
        }

        // ========== ЗАГРУЗКА КНИГ ==========
        private void LoadBooks(string searchText = "", int genreId = 0)
        {
            try
            {
                flowLayoutPanel1.Controls.Clear();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            b.Id,
                            b.NameBook,
                            a.NameAuthor,
                            g.NameGenres,
                            b.RentalPrice,
                            b.TotalCopies,
                            b.CoverImage,
                            (SELECT COUNT(*) FROM BookCopies bc WHERE bc.BookId = b.Id AND bc.StatusCopiesId = 1) as AvailableCopies
                        FROM Book b
                        LEFT JOIN Author a ON b.AuthorId = a.Id
                        LEFT JOIN Genres g ON b.GenresId = g.Id
                        WHERE 1=1";

                    if (!string.IsNullOrEmpty(searchText) && searchText != "Введите название книги")
                    {
                        query += " AND (b.NameBook LIKE @search OR a.NameAuthor LIKE @search)";
                    }

                    if (genreId > 0)
                    {
                        query += " AND b.GenresId = @genreId";
                    }

                    SqlCommand command = new SqlCommand(query, connection);

                    if (!string.IsNullOrEmpty(searchText) && searchText != "Введите название книги")
                    {
                        command.Parameters.AddWithValue("@search", "%" + searchText + "%");
                    }

                    if (genreId > 0)
                    {
                        command.Parameters.AddWithValue("@genreId", genreId);
                    }

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        UserControl2 bookCard = new UserControl2();

                        int total = Convert.ToInt32(reader["TotalCopies"]);
                        int available = reader["AvailableCopies"] != DBNull.Value ?
                                       Convert.ToInt32(reader["AvailableCopies"]) : 0;

                        bookCard.SetBookData(
                            Convert.ToInt32(reader["Id"]),
                            reader["NameBook"].ToString(),
                            reader["NameAuthor"].ToString(),
                            reader["NameGenres"].ToString(),
                            Convert.ToDecimal(reader["RentalPrice"]),
                            total,
                            available,
                            reader["CoverImage"] != DBNull.Value ? reader["CoverImage"].ToString() : null
                        );

                        // Подписываемся на событие клика по кнопке действий
                        bookCard.ActionClicked += BookCard_ActionClicked;

                        flowLayoutPanel1.Controls.Add(bookCard);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки книг: " + ex.Message);
            }
        }

        // ========== ОБРАБОТЧИКИ КНОПОК ==========
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchText = textBox1.Text;
            int genreId = comboBox1.SelectedValue != null ? (int)comboBox1.SelectedValue : 0;
            LoadBooks(searchText, genreId);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Можно автоматически обновлять при смене жанра
            // BtnSearch_Click(sender, e);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            Form7 addForm = new Form7();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadBooks(); // Перезагружаем список после добавления
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            Form5 librarianForm = new Form5(userId, roleId); // Передаем userId и roleId обратно
            librarianForm.Show();
            this.Close();
        }

        // ========== ОБРАБОТКА КЛИКА ПО КНОПКЕ ДЕЙСТВИЙ НА КАРТОЧКЕ ==========
        private void BookCard_ActionClicked(object sender, ActionEventArgs e)
        {
            // Создаем контекстное меню
            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem editItem = new ToolStripMenuItem("✏️ Изменить книгу");
            editItem.Click += (s, ev) => EditBook(e.BookId);

            ToolStripMenuItem copiesItem = new ToolStripMenuItem("📚 Управление экземплярами");
            copiesItem.Click += (s, ev) => ManageCopies(e.BookId);

            ToolStripMenuItem deleteItem = new ToolStripMenuItem("🗑️ Удалить книгу");
            deleteItem.ForeColor = Color.Red;
            deleteItem.Click += (s, ev) => DeleteBook(e.BookId);

            menu.Items.Add(editItem);
            menu.Items.Add(copiesItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(deleteItem);

            // Показываем меню рядом с кнопкой
            menu.Show(this, e.ButtonLocation);
        }

        private void EditBook(int bookId)
        {
            Form7 editForm = new Form7(bookId);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadBooks(); // Перезагружаем список после редактирования
            }
        }

        private void ManageCopies(int bookId)
        {
            // Получаем название книги для заголовка
            string bookName = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT NameBook FROM Book WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", bookId);
                    bookName = cmd.ExecuteScalar()?.ToString() ?? "";
                }
            }
            catch { }

            Form8 copiesForm = new Form8(bookId, bookName);
            copiesForm.ShowDialog();
            LoadBooks(); // Перезагружаем список (обновляем количество экземпляров)
        }

        private void DeleteBook(int bookId)
        {
            DialogResult result = MessageBox.Show("Вы действительно хотите удалить книгу?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Сначала проверяем, есть ли экземпляры
                        string checkQuery = "SELECT COUNT(*) FROM BookCopies WHERE BookId = @bookId";
                        SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                        checkCmd.Parameters.AddWithValue("@bookId", bookId);

                        int copiesCount = (int)checkCmd.ExecuteScalar();

                        if (copiesCount > 0)
                        {
                            MessageBox.Show("Нельзя удалить книгу, у которой есть экземпляры!");
                            return;
                        }

                        // Удаляем книгу
                        string deleteQuery = "DELETE FROM Book WHERE Id = @bookId";
                        SqlCommand deleteCmd = new SqlCommand(deleteQuery, connection);
                        deleteCmd.Parameters.AddWithValue("@bookId", bookId);
                        deleteCmd.ExecuteNonQuery();

                        MessageBox.Show("Книга удалена!");
                        LoadBooks(); // Перезагружаем список
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message);
                }
            }
        }

        private void Form6_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Не закрываем приложение полностью, только форму
        }
    }
}