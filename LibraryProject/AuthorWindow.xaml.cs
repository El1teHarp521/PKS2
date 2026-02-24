using System;
using System.Linq;
using System.Windows;
using LibraryProject.Models;

namespace LibraryProject
{
    public partial class AuthorWindow : Window
    {
        private Data.LibraryContext _db;
        public AuthorWindow(Data.LibraryContext db)
        {
            InitializeComponent();
            _db = db;
            Load();
        }
        private void Load() => lstAuthors.ItemsSource = _db.Authors.ToList();

        private void Add_Click(object sender, RoutedEventArgs e) {
            string first = txtFirst.Text.Trim();
            string last = txtLast.Text.Trim();

            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last)) return;

            // 1. Проверка на заглавные буквы
            if (!char.IsUpper(first[0]) || !char.IsUpper(last[0])) {
                MessageBox.Show("Имя и Фамилия должны начинаться с ЗАГЛАВНОЙ буквы!");
                return;
            }

            // 2. Проверка на дубликат без учета регистра
            bool exists = _db.Authors.AsEnumerable().Any(a => 
                a.FirstName.Equals(first, StringComparison.OrdinalIgnoreCase) && 
                a.LastName.Equals(last, StringComparison.OrdinalIgnoreCase));

            if (exists) {
                MessageBox.Show("Такой автор уже есть (даже если буквы другие)!", "Дубликат");
                return;
            }

            _db.Authors.Add(new Author { FirstName = first, LastName = last, Country="Russia" });
            _db.SaveChanges();
            txtFirst.Clear(); txtLast.Clear();
            Load();
        }

        private void Delete_Click(object sender, RoutedEventArgs e) {
            if (lstAuthors.SelectedItem is Author a) {
                _db.Authors.Remove(a);
                _db.SaveChanges();
                Load();
            }
        }
    }
}