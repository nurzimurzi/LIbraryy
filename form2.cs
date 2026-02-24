using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace FormLibr
{
    public partial class Form2 : Form
    {
        private int userId;
        private int roleId;
        private string connectionString = @"Data Source=KRN-20-202-C14\MSSQLSERVER02;Initial Catalog=Libraryy;Integrated Security=True";

        public int UserId { get { return userId; } }
        public int RoleId { get { return roleId; } }

        public Form2(int userId, int roleId)
        {
            InitializeComponent();
            this.userId = userId;
            this.roleId = roleId;

            textBox1.Text = "Введите название книги";
            textBox1.GotFocus += TextBox1_GotFocus;
            textBox1.LostFocus += TextBox1_LostFocus;

            LoadGenres();
            LoadBooks();

            this.button1.Click += BtnSearch_Click;
            this.button2.Click += BtnProfile_Click;
            this.button3.Click += BtnBack_Click;
        }

        private void TextBox1_GotFocus(object sender, EventArgs e)
        {
            if (textBox1.Text == "Введите название книги")
            {
                textBox1.Text = "";
                textBox1.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void TextBox1_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Введите название книги";
                textBox1.ForeColor = System.Drawing.Color.Gray;
            }
        }

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
                            b.CoverImage
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
                        UserControl1 bookCard = new UserControl1();

                        bookCard.SetBookData(
                            Convert.ToInt32(reader["Id"]),
                            reader["NameBook"].ToString(),
                            reader["NameAuthor"].ToString(),
                            reader["NameGenres"].ToString(),
                            Convert.ToDecimal(reader["RentalPrice"]),
                            reader["CoverImage"] != DBNull.Value ? reader["CoverImage"].ToString() : null
                        );

                        flowLayoutPanel1.Controls.Add(bookCard);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки книг: " + ex.Message);
            }
        }
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchText = textBox1.Text;
            int genreId = comboBox1.SelectedValue != null ? (int)comboBox1.SelectedValue : 0;
            LoadBooks(searchText, genreId);
        }
        // открытие FORM4
        private void BtnProfile_Click(object sender, EventArgs e)
        {
            Form4 profileForm = new Form4(userId, roleId);
            profileForm.Show();
            this.Hide();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            Form1 loginForm = new Form1();
            loginForm.Show();
            this.Close();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
