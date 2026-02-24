using FormLibr;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
namespace FormLibr
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Data Source=KRN-20-202-C14\MSSQLSERVER02;Initial Catalog=Libraryy;Integrated Security=True";
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }
            // Запрос к таблице User 
            string query = "SELECT Id, RoleId FROM [User] WHERE Login = @login AND Password = @password";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Используем параметры, чтобы избежать SQL-инъекций
                        command.Parameters.AddWithValue("@login", login);
                        command.Parameters.AddWithValue("@password", password);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = reader.GetInt32(0); // Id
                                int roleId = reader.GetInt32(1); // RoleId
                                // В зависимости от роли открываем нужную форму
                                if (roleId == 1 || roleId == 2) //  Библиотекарь (2)
                                {
                                    // Открываем форму библиотекаря (Form5)
                                    Form5 librarianForm = new Form5(userId, roleId);
                                    librarianForm.Show();
                                }
                                else if (roleId == 3) // Читатель (3)
                                {
                                    // Открываем форму читателя (Form2)
                                    Form2 readerForm = new Form2(userId, roleId);
                                    readerForm.Show();
                                }
                                else
                                {
                                    MessageBox.Show("Неизвестная роль пользователя!");
                                    return;
                                }
                                this.Hide(); // Скрываем форму авторизации
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль!");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
