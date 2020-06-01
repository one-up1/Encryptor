using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Encryptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly byte[] ENCRYPTION_SALT = new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };
       
        public MainWindow()
        {
            InitializeComponent();
        }

        private void bEncrypt_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Length == 0)
            {
                MessageBox.Show("Geen text opgegeven", "Encryptor",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                textBox.Focus();
                return;
            }

            if (passwordBox.Password.Length == 0)
            {
                MessageBox.Show("Geen wachtwoord opgegeven", "Encryptor",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                passwordBox.Focus();
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == true) // Waarom is "== true" nodig? ShowDialog() geeft toch een boolean terug en dat kan toch zo in een if statement? ( "if (sfd.ShowDialog())" )
            {
                try
                {
                    Encrypt(sfd.FileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    MessageBox.Show("Encryptie mislukt", "Encryptor",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void bDecrypt_Click(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password.Length == 0)
            {
                MessageBox.Show("Geen wachtwoord opgegeven", "Encryptor",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                passwordBox.Focus();
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    Decrypt(ofd.FileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    MessageBox.Show("Decryptie mislukt", "Encryptor",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Encrypt(string path)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(textBox.Text);
            using (Aes aes = Aes.Create())
            {
                SetEncryptionKey(aes);
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    using (CryptoStream cs = new CryptoStream(fs,
                        aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytes, 0, bytes.Length);
                        cs.Close();
                    }
                }
            }
        }

        private void Decrypt(String path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            using (Aes aes = Aes.Create())
            {
                SetEncryptionKey(aes);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms,
                        aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytes, 0, bytes.Length);
                        cs.Close();
                    }
                    textBox.Text = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }

        private void SetEncryptionKey(Aes aes)
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(
                    passwordBox.Password, ENCRYPTION_SALT);
            aes.Key = pdb.GetBytes(32);
            aes.IV = pdb.GetBytes(16);
        }
    }
}
