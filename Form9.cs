using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace FormLibr
{
    public partial class Form9 : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Libraryy;Integrated Security=True";
        private int userId;
        private int roleId;

        // Для выдачи
        private int selectedReaderId = 0;
        private int selectedBookId = 0;
        private int selectedCopyId = 0;

        // Для возврата
        private int selectedLoanId = 0;

        public Form9(int userId, int roleId)
        {
            InitializeComponent();
            this.userId = userId;
            this.roleId = roleId;

            // Настройка текстовых полей
            SetupTextFields();

            // Настройка дат
            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now.AddDays(14);

            // Подключение событий
            SubscribeEvents();
        }

        private void SetupTextFields()
        {
            // Поиск читателя
            textBox1.Text = "Поиск...";
            textBox1.ForeColor = Color.Gray;
            textBox1.GotFocus += (s, e) =>
            {
                if (textBox1.Text == "Поиск...")
                {
                    textBox1.Text = "";
                    textBox1.ForeColor = Color.Black;
                }
            };
            textBox1.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    textBox1.Text = "Поиск...";
                    textBox1.ForeColor = Color.Gray;
                }
            };

            // Поиск книги
            textBox2.Text = "Поиск...";
            textBox2.ForeColor = Color.Gray;
            textBox2.GotFocus += (s, e) =>
            {
                if (textBox2.Text == "Поиск...")
                {
                    textBox2.Text = "";
                    textBox2.ForeColor = Color.Black;
                }
            };
            textBox2.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    textBox2.Text = "Поиск...";
                    textBox2.ForeColor = Color.Gray;
                }
            };

            // Поиск по инвентарному номеру
            textBox4.Text = "Поиск...";
            textBox4.ForeColor = Color.Gray;
            textBox4.GotFocus += (s, e) =>
            {
                if (textBox4.Text == "Поиск...")
                {
                    textBox4.Text = "";
                    textBox4.ForeColor = Color.Black;
                }
            };
            textBox4.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox4.Text))
                {
                    textBox4.Text = "Поиск...";
                    textBox4.ForeColor = Color.Gray;
                }
            };
        }

        private void SubscribeEvents()
        {
            // Кнопки навигации
            button5.Click += BtnBack_Click;

            // Вкладка "Выдача"
            button1.Click += BtnSearchReader_Click;
            button2.Click += BtnSearchBook_Click;
            button3.Click += BtnIssueBook_Click;

            // Вкладка "Возврат"
            button6.Click += BtnSearchReturn_Click;
            button4.Click += BtnReturnBook_Click;
        }

        // ========== ВЫДАЧА КНИГИ ==========
        private void BtnSearchReader_Click(object sender, EventArgs e)
        {
            string searchText = textBox1.Text;
            if (searchText == "Поиск..." || string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Введите фамилию читателя!");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT Id, Surname, Name, Patronmic
                        FROM [User]
                        WHERE RoleId = 3 
                        AND (Surname LIKE @search OR Name LIKE @search OR Patronmic LIKE @search)
                        ORDER BY Surname";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        selectedReaderId = reader.GetInt32(0);
                        string fullName = $"{reader["Surname"]} {reader["Name"]} {reader["Patronmic"]}".Trim();
                        label1.Text = fullName;
                        MessageBox.Show("Читатель найден!");
                    }
                    else
                    {
                        MessageBox.Show("Читатель не найден!");
                        selectedReaderId = 0;
                        label1.Text = "ФИО читателя";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message);
            }
        }

        private void BtnSearchBook_Click(object sender, EventArgs e)
        {
            string searchText = textBox2.Text;
            if (searchText == "Поиск..." || string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Введите название книги!");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            b.Id,
                            b.NameBook,
                            a.NameAuthor,
                            (SELECT COUNT(*) FROM BookCopies bc 
                             WHERE bc.BookId = b.Id AND bc.StatusCopiesId = 1) as AvailableCopies
                        FROM Book b
                        LEFT JOIN Author a ON b.AuthorId = a.Id
                        WHERE b.NameBook LIKE @search
                        ORDER BY b.NameBook";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        selectedBookId = reader.GetInt32(0);
                        string bookName = reader["NameBook"].ToString();
                        string author = reader["NameAuthor"].ToString();
                        int available = reader["AvailableCopies"] != DBNull.Value ?
                                       Convert.ToInt32(reader["AvailableCopies"]) : 0;

                        label5.Text = $"{bookName} (доступно: {available})";

                        if (available > 0)
                        {
                            // Получаем первый доступный экземпляр
                            reader.Close();

                            string copyQuery = @"
                                SELECT Id FROM BookCopies 
                                WHERE BookId = @bookId AND StatusCopiesId = 1";

                            SqlCommand copyCmd = new SqlCommand(copyQuery, connection);
                            copyCmd.Parameters.AddWithValue("@bookId", selectedBookId);

                            selectedCopyId = (int)copyCmd.ExecuteScalar();
                        }
                        else
                        {
                            MessageBox.Show("Нет доступных экземпляров!");
                            selectedBookId = 0;
                            selectedCopyId = 0;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Книга не найдена!");
                        selectedBookId = 0;
                        selectedCopyId = 0;
                        label5.Text = "Название книги (доступно: )";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message);
            }
        }

        private void BtnIssueBook_Click(object sender, EventArgs e)
        {
            if (selectedReaderId == 0)
            {
                MessageBox.Show("Выберите читателя!");
                return;
            }

            if (selectedCopyId == 0)
            {
                MessageBox.Show("Выберите книгу!");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Получаем новый ID для выдачи
                    string getIdQuery = "SELECT ISNULL(MAX(Id), 0) + 1 FROM Loans";
                    SqlCommand getIdCmd = new SqlCommand(getIdQuery, connection);
                    int newId = (int)getIdCmd.ExecuteScalar();

                    // Создаем выдачу
                    string insertQuery = @"
                        INSERT INTO Loans (Id, LoanDate, DueDate, CopyId, UserId, LibrarianId, StatusLoanId)
                        VALUES (@id, @loanDate, @dueDate, @copyId, @userId, @librarianId, 1)";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@id", newId);
                    insertCmd.Parameters.AddWithValue("@loanDate", dateTimePicker1.Value);
                    insertCmd.Parameters.AddWithValue("@dueDate", dateTimePicker2.Value);
                    insertCmd.Parameters.AddWithValue("@copyId", selectedCopyId);
                    insertCmd.Parameters.AddWithValue("@userId", selectedReaderId);
                    insertCmd.Parameters.AddWithValue("@librarianId", this.userId);

                    insertCmd.ExecuteNonQuery();

                    // Обновляем статус экземпляра
                    string updateCopyQuery = "UPDATE BookCopies SET StatusCopiesId = 2 WHERE Id = @copyId";
                    SqlCommand updateCmd = new SqlCommand(updateCopyQuery, connection);
                    updateCmd.Parameters.AddWithValue("@copyId", selectedCopyId);
                    updateCmd.ExecuteNonQuery();

                    MessageBox.Show("Книга выдана!");

                    // Сброс
                    selectedReaderId = 0;
                    selectedCopyId = 0;
                    selectedBookId = 0;
                    label1.Text = "ФИО читателя";
                    label5.Text = "Название книги (доступно: )";
                    textBox1.Text = "Поиск...";
                    textBox2.Text = "Поиск...";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выдаче: " + ex.Message);
            }
        }

        // ========== ВОЗВРАТ КНИГИ ==========
        private void BtnSearchReturn_Click(object sender, EventArgs e)
        {
            string invNumber = textBox4.Text;
            if (invNumber == "Поиск..." || string.IsNullOrWhiteSpace(invNumber))
            {
                MessageBox.Show("Введите инвентарный номер!");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            l.Id,
                            b.NameBook,
                            u.Surname + ' ' + u.Name + ' ' + ISNULL(u.Patronmic, '') as ReaderName,
                            l.LoanDate,
                            l.DueDate,
                            DATEDIFF(day, l.DueDate, GETDATE()) as OverdueDays
                        FROM Loans l
                        JOIN BookCopies bc ON l.CopyId = bc.Id
                        JOIN Book b ON bc.BookId = b.Id
                        JOIN [User] u ON l.UserId = u.Id
                        WHERE bc.InventoryNumber = @inv 
                        AND l.StatusLoanId IN (1, 3)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@inv", invNumber);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        selectedLoanId = reader.GetInt32(0);

                        label8.Text = $"Название книги: {reader["NameBook"]}";
                        label9.Text = $"Читатель: {reader["ReaderName"]}";
                        label10.Text = $"Выдана: {Convert.ToDateTime(reader["LoanDate"]):dd.MM.yyyy}";
                        label11.Text = $"Должна быть: {Convert.ToDateTime(reader["DueDate"]):dd.MM.yyyy}";

                        int overdue = reader["OverdueDays"] != DBNull.Value ?
                                      Convert.ToInt32(reader["OverdueDays"]) : 0;

                        if (overdue > 0)
                        {
                            label13.Text = $"Просрочка: {overdue} дней";
                            label13.ForeColor = Color.Red;
                        }
                        else
                        {
                            label13.Text = "Просрочка: нет";
                            label13.ForeColor = SystemColors.ControlDarkDark;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Активная выдача не найдена!");
                        ClearReturnLabels();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message);
            }
        }

        private void BtnReturnBook_Click(object sender, EventArgs e)
        {
            if (selectedLoanId == 0)
            {
                MessageBox.Show("Сначала найдите выдачу!");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Обновляем статус выдачи
                    string updateLoanQuery = "UPDATE Loans SET StatusLoanId = 2 WHERE Id = @loanId";
                    SqlCommand updateLoanCmd = new SqlCommand(updateLoanQuery, connection);
                    updateLoanCmd.Parameters.AddWithValue("@loanId", selectedLoanId);
                    updateLoanCmd.ExecuteNonQuery();

                    // Обновляем статус экземпляра
                    string updateCopyQuery = @"
                        UPDATE BookCopies 
                        SET StatusCopiesId = 1 
                        WHERE Id = (SELECT CopyId FROM Loans WHERE Id = @loanId)";

                    SqlCommand updateCopyCmd = new SqlCommand(updateCopyQuery, connection);
                    updateCopyCmd.Parameters.AddWithValue("@loanId", selectedLoanId);
                    updateCopyCmd.ExecuteNonQuery();

                    MessageBox.Show("Книга возвращена!");

                    // Сброс
                    selectedLoanId = 0;
                    textBox4.Text = "Поиск...";
                    ClearReturnLabels();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при возврате: " + ex.Message);
            }
        }

        private void ClearReturnLabels()
        {
            label8.Text = "Название книги: []";
            label9.Text = "Читатель: []";
            label10.Text = "Выдана: []";
            label11.Text = "Должна быть: []";
            label13.Text = "Просрочка: []";
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            Form5 librarianForm = new Form5(userId, roleId);
            librarianForm.Show();
            this.Close();
        }

        // Пустые обработчики
        private void label5_Click(object sender, EventArgs e) { }
        private void tabPage2_Click(object sender, EventArgs e) { }
    }
}