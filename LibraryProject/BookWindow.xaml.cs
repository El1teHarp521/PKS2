using System;
using System.Linq;
using System.Windows;
using LibraryProject.Models;
using System.Collections.Generic;

namespace LibraryProject
{
    public partial class BookWindow : Window
    {
        public Book Book { get; set; }
        private Data.LibraryContext _db;

        public BookWindow(Book book, Data.LibraryContext db)
        {
            InitializeComponent();
            _db = db;
            Book = book;
            
            lstAuthors.ItemsSource = _db.Authors.ToList();
            lstGenres.ItemsSource = _db.Genres.ToList();

            if (Book.Id != 0) {
                txtTitle.Text = Book.Title;
                txtISBN.Text = Book.ISBN;
                txtYear.Text = Book.PublishYear.ToString();
                txtQuantity.Text = Book.QuantityInStock.ToString();
                
                foreach (var author in Book.Authors)
                    lstAuthors.SelectedItems.Add(_db.Authors.Find(author.Id));
                foreach (var genre in Book.Genres)
                    lstGenres.SelectedItems.Add(_db.Genres.Find(genre.Id));
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string isbn = txtISBN.Text.Trim();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(isbn) ||
                lstAuthors.SelectedItems.Count == 0 || lstGenres.SelectedItems.Count == 0) {
                MessageBox.Show("Заполните все поля и выберите хотя бы одного автора и жанр!");
                return;
            }

            // Проверка заглавной буквы
            if (!char.IsUpper(title[0])) {
                MessageBox.Show("Название книги должно быть с большой буквы!");
                return;
            }

            // ПРОВЕРКА ISBN (13 цифр, только числа)
            if (isbn.Length != 13 || !isbn.All(char.IsDigit)) {
                MessageBox.Show("ISBN должен состоять ровно из 13 ЦИФР без букв и знаков!");
                return;
            }

            try {
                Book.Title = title;
                Book.ISBN = isbn;
                Book.PublishYear = int.Parse(txtYear.Text);
                Book.QuantityInStock = int.Parse(txtQuantity.Text);

                Book.Authors.Clear();
                foreach (Author a in lstAuthors.SelectedItems) Book.Authors.Add(a);

                Book.Genres.Clear();
                foreach (Genre g in lstGenres.SelectedItems) Book.Genres.Add(g);

                DialogResult = true;
            } catch {
                MessageBox.Show("Ошибка в числовых полях!");
            }
        }
    }
}