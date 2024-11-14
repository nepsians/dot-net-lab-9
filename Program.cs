using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace LibraryApp
{
    class Program
    {
        static string connectionString = "datasource = localhost; port = 3306; username = root; password = root; database = bca";
        static MySqlConnection connection;

        static void Main(string[] args)
        {
            connection = new MySqlConnection(connectionString);
            connection.Open();
            
            while (true)
            {
                Console.WriteLine("1. Add a New Book");
                Console.WriteLine("2. View Books");
                Console.WriteLine("3. Update Book Price");
                Console.WriteLine("4. Delete Book");
                Console.WriteLine("5. Exit");
                Console.Write("Please choose an option (1-5): ");
                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        AddBook();
                        break;
                    case "2":
                        ViewBooks();
                        break;
                    case "3":
                        UpdateBookPrice();
                        break;
                    case "4":
                        DeleteBook();
                        break;
                    case "5":
                        connection.Close();
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        // 1. Add a New Book
        static void AddBook()
        {
            try
            {
                Console.Write("Enter Book Title: ");
                string title = Console.ReadLine();
                if (string.IsNullOrEmpty(title) || title.Length > 100)
                {
                    Console.WriteLine("Invalid Book Title. Title should not be empty and must be less than 100 characters.");
                    return;
                }

                Console.Write("Enter Author Name: ");
                string author = Console.ReadLine();
                if (string.IsNullOrEmpty(author) || author.Length > 50)
                {
                    Console.WriteLine("Invalid Author Name. Name should not be empty and must be less than 50 characters.");
                    return;
                }

                Console.Write("Enter Price: ");
                decimal price;
                if (!decimal.TryParse(Console.ReadLine(), out price) || price <= 0)
                {
                    Console.WriteLine("Invalid Price. Price must be a positive number.");
                    return;
                }

                Console.Write("Enter Publish Date (yyyy-mm-dd): ");
                DateTime publishDate;
                if (!DateTime.TryParse(Console.ReadLine(), out publishDate) || publishDate > DateTime.Now)
                {
                    Console.WriteLine("Invalid Publish Date. Date cannot be in the future.");
                    return;
                }

                string query = "INSERT INTO Books (BookTitle, Author, Price, PublishDate) VALUES (@BookTitle, @Author, @Price, @PublishDate)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@BookTitle", title);
                cmd.Parameters.AddWithValue("@Author", author);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@PublishDate", publishDate);

                int rowsAffected = cmd.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} book(s) added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // 2. View Books
        static void ViewBooks()
        {
            try
            {
                Console.WriteLine("Filter by Author (leave empty to skip): ");
                string authorFilter = Console.ReadLine();

                Console.WriteLine("Enter Minimum Price (or press Enter to skip): ");
                string priceMinInput = Console.ReadLine();
                decimal? priceMin = string.IsNullOrEmpty(priceMinInput) ? (decimal?)null : decimal.Parse(priceMinInput);

                Console.WriteLine("Enter Maximum Price (or press Enter to skip): ");
                string priceMaxInput = Console.ReadLine();
                decimal? priceMax = string.IsNullOrEmpty(priceMaxInput) ? (decimal?)null : decimal.Parse(priceMaxInput);

                Console.WriteLine("Enter Start Date (yyyy-mm-dd, or press Enter to skip): ");
                string startDateInput = Console.ReadLine();
                DateTime? startDate = string.IsNullOrEmpty(startDateInput) ? (DateTime?)null : DateTime.Parse(startDateInput);

                Console.WriteLine("Enter End Date (yyyy-mm-dd, or press Enter to skip): ");
                string endDateInput = Console.ReadLine();
                DateTime? endDate = string.IsNullOrEmpty(endDateInput) ? (DateTime?)null : DateTime.Parse(endDateInput);

                string query = "SELECT * FROM Books WHERE (@Author IS NULL OR Author LIKE @Author) " +
                               "AND (@PriceMin IS NULL OR Price >= @PriceMin) " +
                               "AND (@PriceMax IS NULL OR Price <= @PriceMax) " +
                               "AND (@StartDate IS NULL OR PublishDate >= @StartDate) " +
                               "AND (@EndDate IS NULL OR PublishDate <= @EndDate)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Author", string.IsNullOrEmpty(authorFilter) ? (object)DBNull.Value : "%" + authorFilter + "%");
                cmd.Parameters.AddWithValue("@PriceMin", priceMin ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PriceMax", priceMax ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@StartDate", startDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@EndDate", endDate ?? (object)DBNull.Value);

                MySqlDataReader reader = cmd.ExecuteReader();
                
                Console.WriteLine("\n\n\n");

                Console.WriteLine("\n{0,-5} {1,-50} {2,-20} {3,-10} {4,-15}", "ID", "Title", "Author", "Price", "Publish Date");
                Console.WriteLine(new string('-', 100));

                while (reader.Read())
                {
                    // Console.WriteLine($"{reader["BookId"]} | {reader["BookTitle"]} | {reader["Author"]} | {reader["Price"]} | {reader["PublishDate"]}");
                    Console.WriteLine("{0,-5} {1,-50} {2,-20} {3,-10:C} {4,-15:yyyy-MM-dd}",
                                        reader["BookId"],
                                        reader["BookTitle"],
                                        reader["Author"],
                                        reader["Price"],
                                        reader["PublishDate"]);
                }
                Console.WriteLine("\n\n\n");
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // 3. Update Book Price
        static void UpdateBookPrice()
        {
            try
            {
                Console.Write("Enter BookId to Update: ");
                int bookId = int.Parse(Console.ReadLine());

                Console.Write("Enter New Price: ");
                decimal price;
                if (!decimal.TryParse(Console.ReadLine(), out price) || price <= 0)
                {
                    Console.WriteLine("Invalid Price. Price must be a positive number.");
                    return;
                }

                string query = "UPDATE Books SET Price = @Price WHERE BookId = @BookId";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@BookId", bookId);

                int rowsAffected = cmd.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} record(s) updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // 4. Delete Book
        static void DeleteBook()
        {
            try
            {
                Console.Write("Enter BookId to Delete: ");
                int bookId = int.Parse(Console.ReadLine());

                string query = "DELETE FROM Books WHERE BookId = @BookId";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@BookId", bookId);

                int rowsAffected = cmd.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} record(s) deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
