using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FormLibr
{
    public partial class Form5 : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Libraryy;Integrated Security=True";
        private int userId;
        private int roleId;

        public Form5(int userId, int roleId)
        {
            InitializeComponent();
            this.userId = userId;
            this.roleId = roleId;

            // Подключаем события для эффектов при наведении
            button3.MouseEnter += button_MouseEnter;
            button3.MouseLeave += button_MouseLeave;
            button2.MouseEnter += button_MouseEnter;
            button2.MouseLeave += button_MouseLeave;
            button4.MouseEnter += button_MouseEnter;
            button4.MouseLeave += button_MouseLeave;
            button5.MouseEnter += button_MouseEnter;
            button5.MouseLeave += button_MouseLeave;

            // Загружаем имя пользователя
            LoadUserInfo();
        }

        // Загрузка информации о пользователе из БД
        private void LoadUserInfo()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT Surname, Name, Patronmic 
                        FROM [User] 
                        WHERE Id = @userId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string surname = reader["Surname"].ToString();
                        string name = reader["Name"].ToString();
                        string patronmic = reader["Patronmic"] != DBNull.Value ? reader["Patronmic"].ToString() : "";

                        // Формируем ФИО (Иванов Иван Иванович)
                        string fullName = $"{surname} {name} {patronmic}".Trim();

                        // Определяем роль
                        string roleName = roleId == 1 ? "Администратор" : "Библиотекарь";

                        // Выводим в label4
                        label4.Text = $"Добро пожаловать, {fullName}! \n ({roleName})";
                    }
                    else
                    {
                        label4.Text = "Добро пожаловать, Библиотекарь!";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных пользователя: " + ex.Message);
                label4.Text = "Добро пожаловать, !";
            }
        }

        // Добавляем метод для закругления кнопок
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

        // Добавляем метод для закругления панели
        private void MakePanelRounded(Panel pnl, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(pnl.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(pnl.Width - radius, pnl.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, pnl.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            pnl.Region = new Region(path);
        }

        // Переопределяем OnLoad - здесь применяем закругления
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Закругляем панель (радиус 30)
            MakePanelRounded(panel1, 30);

            // Закругляем все кнопки
            MakeButtonRounded(button3, 20); // Управление книгами
         
            MakeButtonRounded(button2, 20); // Выдача и возврат
            MakeButtonRounded(button4, 20); // Журнал выдач
            MakeButtonRounded(button5, 30); // Выход (чуть более круглые)
        }

        // Кнопка "Управление книгами" - переход на Form6
        private void button3_Click(object sender, EventArgs e)
        {
            Form6 booksForm = new Form6(userId, roleId);
            booksForm.Show();
            this.Hide();
        }

       

        private void button2_Click(object sender, EventArgs e)
        {
            Form9 issueForm = new Form9(userId, roleId);
            issueForm.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form10 journalForm = new Form10(userId, roleId);
            journalForm.Show();
            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Выход
            DialogResult result = MessageBox.Show("Вы действительно хотите выйти из системы?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Form1 loginForm = new Form1();
                loginForm.Show();
                this.Close();
            }
        }

        // Добавляем эффекты при наведении
        private void button_MouseEnter(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                btn.BackColor = SystemColors.ControlDark; // Светлее при наведении
            }
        }

        private void button_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                // Возвращаем исходный цвет
                if (btn == button5)
                    btn.BackColor = Color.FromArgb(192, 64, 64); // Красный для выхода
                else
                    btn.BackColor = SystemColors.ControlDarkDark; // Темно-серый для остальных
            }
        }

        private void Form5_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Не закрываем приложение полностью
        }
    }
}