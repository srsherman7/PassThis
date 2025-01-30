using System;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Security.Policy;

namespace PasswordManager
{
    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Data Source=passwords.db;Version=3;";
        private const string EncryptionKey = "PUTKEYHERE"; //change this to your own key
        public ObservableCollection<string> Sites { get; set; } = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
            InitializeMasterTable();
            checkMaster();            
            
        }
        private void checkMaster()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM MasterPass";
                var command = new SQLiteCommand(query, connection);
                int count = Convert.ToInt32(command.ExecuteScalar()) ;
                if (count == 0)                    
                    {
                    CreateMasterButton.Visibility = Visibility.Visible;
                    ErrorBox.Text = "Please Create a Master Password.";
                    }
                else
                {
                    CreateMasterButton.Visibility = Visibility.Hidden;
                    resetMasterPassButton.Visibility = Visibility.Visible;
                }



            }

        }
        private void AuthMaster(object sender, RoutedEventArgs e)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT Mpassword FROM MasterPass";
                var command = new SQLiteCommand(query, connection);
                var reader = command.ExecuteReader();
                reader.Read();
                string decPass = Decrypt(reader["Mpassword"].ToString());
                    if (MasterPassText.Password == decPass)
                    {
                        LockPanel.Visibility = Visibility.Hidden;                                             
                        LoadSites();
                        SitesListBox.ItemsSource = Sites;
                    }
                    else
                    {
                        ErrorBox.Text = "Wrong password or non exists!";
                    }


            }
        }

        private void InitializeMasterTable()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS MasterPass (id INTEGER PRIMARY KEY, Mpassword TEXT)";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS Passwords (id INTEGER PRIMARY KEY, site TEXT, username TEXT, password TEXT)";
                 using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        
        private void LoadSites()
        {
            Sites.Clear();
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT DISTINCT site FROM Passwords";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Sites.Add(reader["site"].ToString());
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string site = txtSite.Text;
            string username = txtUsername.Text;
            string password = Encrypt(txtPassword.Password);

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string insertQuery = "INSERT INTO Passwords (site, username, password) VALUES (@site, @username, @password)";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@site", site);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.ExecuteNonQuery();
                }
            }
            LoadSites();
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            string site = txtSite.Text;
            string username = txtUsername.Text;
            string password = Encrypt(txtPassword.Password);

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                var rowID = idBox.Text.ToString();
                string delQuery = "DELETE FROM Passwords where id = " + rowID ;
                var command = new SQLiteCommand(delQuery, connection);
                var rowDeleted = command.ExecuteNonQuery();
            }
            LoadSites();
        }
        private void BtnRetrieve_Click(object sender, RoutedEventArgs e)
        {
            string site = txtSite.Text;
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string selectQuery = "SELECT username, password, id FROM Passwords WHERE site = @site";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@site", site);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtUsername.Text = reader["username"].ToString();
                            txtPassword.Password = Decrypt(reader["password"].ToString());
                            idBox.Text = reader["id"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("No record found.");
                        }
                    }
                }
            }
        }

        private void BtnCopyPassword_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPassword.Password))
            {
                Clipboard.SetText(txtPassword.Password);
                MessageBox.Show("Password copied to clipboard.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SitesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SitesListBox.SelectedItem != null)
            {
                txtSite.Text = SitesListBox.SelectedItem.ToString();
                BtnRetrieve_Click(sender, e);
            }
        }

        private string Encrypt(string plainText)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32));
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.GenerateIV();
                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        private string Decrypt(string encryptedText)
        {
            string[] parts = encryptedText.Split(':');
            byte[] iv = Convert.FromBase64String(parts[0]);
            byte[] encryptedBytes = Convert.FromBase64String(parts[1]);
            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32));

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;
                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }

        private void MasterCreatePass(object sender, RoutedEventArgs e)
        {
            string password = Encrypt(MasterPassText.Password);

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string insertQuery = "INSERT INTO MasterPass (Mpassword) VALUES (@password)";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@password", password);
                    command.ExecuteNonQuery();
                }
                ErrorBox.Text = "Master Password Created.";
                CreateMasterButton.Visibility = Visibility.Hidden;
                resetMasterPassButton.Visibility = Visibility.Visible;
            }

        }

        private void resetMasterPass(object sender, RoutedEventArgs e)
        {
            MessageBoxResult Result = MessageBox.Show("Are you sure you want to reset the Master Password with the password in the input?", "Reset Master Pass?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (Result == MessageBoxResult.Yes)
            {
                string password = Encrypt(MasterPassText.Password);
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    string insertQuery = "UPDATE MasterPass SET Mpassword = @password";
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@password", password);
                        command.ExecuteNonQuery();
                    }
                    ErrorBox.Text = "Master Password Reset.";
                }

                }
            else if (Result == MessageBoxResult.No)
            {
               //do nothing, close prompt
            }
        }

        private void GenPass(object sender, RoutedEventArgs e)
        {
            int passwordLength = 15;
            string password = GenerateSecurePassword(passwordLength);
            GenOut.Text = password;
        }
        static string GenerateSecurePassword(int length)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_-+=<>?";
            char[] password = new char[length];
            byte[] randomBytes = new byte[length];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            for (int i = 0; i < length; i++)
            {
                int index = randomBytes[i] % validChars.Length;
                password[i] = validChars[index];
            }

            return new string(password);
        }
    }

       
    }
