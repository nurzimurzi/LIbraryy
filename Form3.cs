using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace FormLibr
{
    public partial class Form3 : Form
    {
        private int bookId;
        private int userId;
        private int roleId;
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Libraryy;Integrated Security=True";

        public Form3(int bookId, int userId, int roleId)
        {
            InitializeComponent();
            this.bookId = bookId;
            this.userId = userId;
            this.roleId = roleId;

            LoadBookDetails();
            this.button1.Click += Button1_Click;
        }

        private void LoadBookDetails()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            b.NameBook,
                            b.ISBN,
                            b.PageCount,
                            b.RentalPrice,
                            b.TotalCopies,
                            b.Description,
                            b.CoverImage,
                            a.NameAuthor,
                            g.NameGenres,
                            p.NamePublisher,
                            al.Name AS AgeLimitName,
                            (SELECT COUNT(*) FROM BookCopies bc WHERE bc.BookId = b.Id) AS AvailableCopies
                        FROM Book b
                        LEFT JOIN Author a ON b.AuthorId = a.Id
                        LEFT JOIN Genres g ON b.GenresId = g.Id
                        LEFT JOIN Publisher p ON b.PublisherId = p.Id
                        LEFT JOIN AgeLimit al ON b.AgeLimitId = al.Id
                        WHERE b.Id = @bookId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@bookId", bookId);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        label1.Text = reader["NameBook"].ToString();
                        label2.Text = "Автор: " + reader["NameAuthor"].ToString();
                        label3.Text = "Жанр: " + reader["NameGenres"].ToString();
                        label4.Text = Convert.ToDecimal(reader["RentalPrice"]).ToString("C");
                        label5.Text = "Издательство: " + reader["NamePublisher"].ToString();
                        label6.Text = "Страниц: " + reader["PageCount"].ToString();
                        label7.Text = reader["Description"].ToString();
                        label8.Text = "ISBN: " + reader["ISBN"].ToString();
                        label9.Text = "Возраст: " + reader["AgeLimitName"].ToString();
                        label10.Text = "Всего экземпляров: " + reader["TotalCopies"].ToString();
                        label11.Text = "Доступно: " + reader["AvailableCopies"].ToString();

                        if (reader["CoverImage"] != DBNull.Value)
                        {
                            string coverPath = reader["CoverImage"].ToString();
                            string fullPath = System.IO.Path.Combine(Application.StartupPath, coverPath);

                            if (System.IO.File.Exists(fullPath))
                            {
                                pictureBox1.ImageLocation = fullPath;
                                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Книга не найдена!");
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string availableText = label11.Text.Replace("Доступно: ", "");
            if (int.TryParse(availableText, out int available) && available > 0)
            {
                MessageBox.Show($"Книга '{label1.Text}' успешно забронирована!", "Бронирование");
            }
            else
            {
                MessageBox.Show("Нет доступных экземпляров для бронирования.");
            }
        }
    }
}