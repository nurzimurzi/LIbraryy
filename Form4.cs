using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace FormLibr
{
    public partial class Form4 : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Libraryy;Integrated Security=True";
        private int userId;
        private int roleId;

        public Form4(int userId, int roleId)
        {
            InitializeComponent();
            this.userId = userId;
            this.roleId = roleId;

            // Настройка заголовков
            label1.Text = "ЛИЧНЫЙ ПРОФИЛЬ";

            // Загружаем данные
            LoadUserInfo();
            LoadCurrentLoans();
            LoadLoanHistory();
        }

        // Загрузка информации о читателе
        private void LoadUserInfo()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT u.Surname, u.Name, u.Patronmic, u.Phone, u.Login, r.NameRole
                        FROM [User] u
                        LEFT JOIN Role r ON u.RoleId = r.Id
                        WHERE u.Id = @userId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        // ФИО
                        string surname = reader["Surname"].ToString();
                        string name = reader["Name"].ToString();
                        string patronmic = reader["Patronmic"] != DBNull.Value ? reader["Patronmic"].ToString() : "";
                        label3.Text = $"{surname} {name} {patronmic}".Trim();

                        // Телефон
                        label6.Text = reader["Phone"].ToString();

                        // Логин
                        label7.Text = reader["Login"].ToString();

                        // Роль
                        label9.Text = reader["NameRole"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        // Загрузка текущих выдач (книги на руках)
        private void LoadCurrentLoans()
        {
            try
            {
                flowLayoutPanel1.Controls.Clear();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            l.Id as LoanId,
                            b.Id as BookId,
                            b.NameBook,
                            l.LoanDate,
                            l.DueDate,
                            DATEDIFF(day, GETDATE(), l.DueDate) as DaysLeft,
                            sl.Name as StatusName
                        FROM Loans l
                        JOIN BookCopies bc ON l.CopyId = bc.Id
                        JOIN Book b ON bc.BookId = b.Id
                        JOIN StatusLoan sl ON l.StatusLoanId = sl.Id
                        WHERE l.UserId = @userId 
                        AND l.StatusLoanId IN (1, 3) -- 1=Выдана, 3=Просрочена (по твоей БД)
                        ORDER BY l.DueDate";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);

                    SqlDataReader reader = command.ExecuteReader();
                    int count = 0;

                    while (reader.Read())
                    {
                        UserControl3 card = new UserControl3();

                        int loanId = Convert.ToInt32(reader["LoanId"]);
                        int bookId = Convert.ToInt32(reader["BookId"]);
                        string bookName = reader["NameBook"].ToString();
                        DateTime loanDate = Convert.ToDateTime(reader["LoanDate"]);
                        DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);

                        // Проверка на просрочку
                        bool isOverdue = dueDate < DateTime.Today;

                        card.SetLoanData(loanId, bookId, bookName, loanDate, dueDate, isOverdue);

                        // Подписываемся на событие продления
                        card.ExtendClicked += Card_ExtendClicked;

                        flowLayoutPanel1.Controls.Add(card);
                        count++;
                    }

                    // Обновляем заголовок
                    label4.Text = $"КНИГИ НА РУКАХ ({count})";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки книг: " + ex.Message);
            }
        }

        // Загрузка истории выдач
        private void LoadLoanHistory()
        {
            try
            {
                listBox1.Items.Clear();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT TOP 20
                            b.NameBook,
                            l.LoanDate,
                            l.DueDate,
                            sl.Name as StatusName
                        FROM Loans l
                        JOIN BookCopies bc ON l.CopyId = bc.Id
                        JOIN Book b ON bc.BookId = b.Id
                        JOIN StatusLoan sl ON l.StatusLoanId = sl.Id
                        WHERE l.UserId = @userId 
                        ORDER BY l.LoanDate DESC";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string bookName = reader["NameBook"].ToString();
                        DateTime loanDate = Convert.ToDateTime(reader["LoanDate"]);
                        string status = reader["StatusName"].ToString();

                        listBox1.Items.Add($"{bookName} - {loanDate:dd.MM.yyyy} ({status})");
                    }

                    label10.Text = $"ИСТОРИЯ ВЫДАЧ \n ({listBox1.Items.Count})";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки истории: " + ex.Message);
            }
        }

        // Обработчик кнопки "Продлить"
        private void Card_ExtendClicked(object sender, ExtendLoanEventArgs e)
        {
            DialogResult result = MessageBox.Show(
                $"Продлить книгу до {e.DueDate.AddDays(14):dd.MM.yyyy}?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Обновляем дату возврата (+14 дней)
                        string query = @"
                            UPDATE Loans 
                            SET DueDate = DATEADD(day, 14, DueDate)
                            WHERE Id = @loanId";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@loanId", e.LoanId);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Срок возврата продлён!");

                        // Перезагружаем список
                        LoadCurrentLoans();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        // Обработчик клика по истории
        private void label10_Click(object sender, EventArgs e)
        {
            // Можно расширить историю или показать детали
        }

        // При загрузке формы
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Настройка FlowLayoutPanel
            flowLayoutPanel1.AutoScroll = true;

            // Настройка ListBox
            listBox1.Font = new Font("Microsoft Sans Serif", 10);
            listBox1.BackColor = Color.White;
            listBox1.BorderStyle = BorderStyle.FixedSingle;

            // Настройка панелей (убираем лишние)
            pictureBox3.Visible = false; // Скрываем, если не используется
        }
    }
}