using System;
using System.Windows.Forms;

namespace FormLibr
{
    public partial class UserControl1 : UserControl
    {
        // Свойства для хранения данных книги
        public int BookId { get; private set; }

        public string BookName
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        public string BookAuthor
        {
            get { return label2.Text; }
            set { label2.Text = value; }
        }

        public string BookGenre
        {
            get { return label3.Text; }
            set { label3.Text = value; }
        }

        public string BookPrice
        {
            get { return label4.Text; }
            set { label4.Text = value; }
        }

        public string CoverImagePath
        {
            set
            {
                if (!string.IsNullOrEmpty(value) && System.IO.File.Exists(value))
                {
                    pictureBox1.ImageLocation = value;
                }
            }
        }

        // Конструктор
        public UserControl1()
        {
            InitializeComponent();

            // Добавляем обработчик клика на всю карточку
            this.Click += UserControl1_Click;
            foreach (Control ctrl in this.Controls)
            {
                ctrl.Click += UserControl1_Click;
            }
        }

        // Метод для заполнения данных книги
        public void SetBookData(int id, string name, string author, string genre, decimal price, string coverPath)
        {
            BookId = id;
            label1.Text = name;
            label2.Text = "Автор: " + author;
            label3.Text = "Жанр: " + genre;
            label4.Text = price.ToString("C");

            if (!string.IsNullOrEmpty(coverPath) && System.IO.File.Exists(coverPath))
            {
                pictureBox1.ImageLocation = coverPath;
            }
        }

        // 👇 ЭТОТ МЕТОД ПОЛНОСТЬЮ ИЗМЕНЁН
        private void UserControl1_Click(object sender, EventArgs e)
        {
            // Находим родительскую форму (Form2)
            Form2 parentForm = this.FindForm() as Form2;

            if (parentForm != null)
            {
                // Открываем Form3 с деталями книги
                Form3 detailsForm = new Form3(BookId, parentForm.UserId, parentForm.RoleId);
                detailsForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Ошибка: не удалось найти родительскую форму");
            }
        }
    }
}