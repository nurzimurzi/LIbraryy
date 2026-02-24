using System;
using System.Drawing;
using System.Windows.Forms;

namespace FormLibr
{
    public partial class UserControl2 : UserControl
    {
        // Свойства для хранения данных книги
        public int BookId { get; private set; }
        public int TotalCopies { get; private set; }
        public int AvailableCopies { get; private set; }

        // Событие для кнопки действий (⚙️)
        public event EventHandler<ActionEventArgs> ActionClicked;

        public UserControl2()
        {
            InitializeComponent();

            // Добавляем обработчик клика на всю карточку (кроме кнопки)
            this.Click += UserControl2_Click;

            // Чтобы клик работал на всех элементах, кроме кнопки
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl != button3) // Кроме кнопки ⚙️
                {
                    ctrl.Click += UserControl2_Click;
                }
            }
        }

        // Метод для заполнения данных книги
        public void SetBookData(int id, string title, string author, string genre,
                               decimal price, int total, int available, string coverPath)
        {
            BookId = id;
            TotalCopies = total;
            AvailableCopies = available;

            label1.Text = title;
            label2.Text = "Автор: " + author;
            label3.Text = "Жанр: " + genre;
            label4.Text = $"Экземпляры: {available}/{total}";

            // Меняем цвет если нет в наличии
            if (available == 0)
            {
                label4.ForeColor = Color.Red;
                this.BackColor = Color.FromArgb(255, 240, 240); // Светло-розовый фон
            }
            else
            {
                label4.ForeColor = SystemColors.ControlDark;
                this.BackColor = Color.White;
            }

            // Загружаем обложку, если есть
            if (!string.IsNullOrEmpty(coverPath) && System.IO.File.Exists(coverPath))
            {
                pictureBox1.ImageLocation = coverPath;
                pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            }
        }

        // Клик по карточке (открыть детали/редактирование)
        private void UserControl2_Click(object sender, EventArgs e)
        {
            Form6 parentForm = this.FindForm() as Form6;
            if (parentForm != null)
            {
                // Открываем форму редактирования книги
                // Form7_Edit editForm = new Form7_Edit(BookId);
                // editForm.ShowDialog();
                // parentForm.LoadBooks(); // Перезагружаем список после редактирования

                MessageBox.Show($"Редактировать книгу: {label1.Text}");
            }
        }

        // Клик по кнопке ⚙️ (открыть меню действий)
        private void button3_Click(object sender, EventArgs e)
        {
            // Вызываем событие и передаем ID книги и позицию кнопки
            ActionClicked?.Invoke(this, new ActionEventArgs(BookId, button3.PointToScreen(Point.Empty)));
        }

        // Метод для закругления углов карточки (будем вызывать из Form6)
        public void MakeRounded(int radius)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            this.Region = new System.Drawing.Region(path);
        }
    }

    // Класс для передачи данных события
    public class ActionEventArgs : EventArgs
    {
        public int BookId { get; private set; }
        public Point ButtonLocation { get; private set; }

        public ActionEventArgs(int bookId, Point buttonLocation)
        {
            BookId = bookId;
            ButtonLocation = buttonLocation;
        }
    }
}